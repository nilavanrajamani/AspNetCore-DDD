using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

using DDD.Domain.Providers.Hubs;
using DDD.Infra.CrossCutting.IoC;
using DDD.Services.Api.Configurations;
using DDD.Services.Api.StartupExtensions;

using MediatR;

using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DDD.Infra.CrossCutting.Identity.Authorization;

var builder = WebApplication.CreateBuilder(args);

// START: Variables
// END: Variables

// START: Custom services
// ----- Database -----
builder.Services.AddCustomizedDatabase(builder.Configuration, builder.Environment);

// ----- Auth -----
builder.Services.AddCustomizedAuth(builder.Configuration);

// ----- Http -----
builder.Services.AddCustomizedHttp(builder.Configuration);

// ----- AutoMapper -----
builder.Services.AddAutoMapperSetup();

// Adding MediatR for Domain Events and Notifications
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// ----- Hash -----
builder.Services.AddCustomizedHash(builder.Configuration);

// ----- SignalR -----
builder.Services.AddCustomizedSignalR();

// ----- Quartz -----
builder.Services.AddCustomizedQuartz(builder.Configuration);

// .NET Native DI Abstraction
NativeInjectorBootStrapper.RegisterServices(builder.Services);

builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        // x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("x-api-version"));
});

// Add ApiExplorer to discover versions
builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();

// ----- Swagger UI -----
builder.Services.AddCustomizedSwagger(builder.Environment);

// ----- Health check -----
builder.Services.AddCustomizedHealthCheck(builder.Configuration, builder.Environment);
// END: Custom services

var app = builder.Build();

// Configure the HTTP request pipeline.

// START: Custom middlewares

if (app.Environment.IsDevelopment())
{
    // ----- Error Handling -----
    app.UseCustomizedErrorHandling();
}

app.UseRouting();

// ----- CORS -----
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// ----- Auth -----
app.UseCustomizedAuth();

// ----- SignalR -----
app.UseCustomizedSignalR();

// ----- Quartz -----
app.UseCustomizedQuartz();

// ----- Controller -----
app.MapControllers();

// ----- SignalR -----
app.MapHub<NotificationHub>($"/hub{HubRoutes.Notification}");

// ----- Health check -----
HealthCheckExtension.UseCustomizedHealthCheck(app, builder.Environment);

// ----- Swagger UI -----
app.UseCustomizedSwagger(builder.Environment);
// END: Custom middlewares

// ----- Database Migration -----
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var applicationDbContext = services.GetRequiredService<DDD.Infra.Data.Context.ApplicationDbContext>();
        var eventStoreContext = services.GetRequiredService<DDD.Infra.Data.Context.EventStoreSqlContext>();
        var authDbContext = services.GetRequiredService<DDD.Infra.CrossCutting.Identity.Data.AuthDbContext>();

        // Apply database migrations (this will create the database if it doesn't exist)
        logger.LogInformation("Checking for pending database migrations...");
        
        try
        {
            var pendingMigrations = applicationDbContext.Database.GetPendingMigrations();
            var pendingMigrationsList = pendingMigrations.ToList();
            
            if (pendingMigrationsList.Any())
            {
                logger.LogInformation("Found {Count} pending migrations: {Migrations}",
                    pendingMigrationsList.Count,
                    string.Join(", ", pendingMigrationsList));
                logger.LogInformation("Applying database migrations...");
                
                applicationDbContext.Database.Migrate();
                
                logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                logger.LogInformation("Database is up to date - no pending migrations");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while applying database migrations: {Message}", ex.Message);
            throw;
        }

        // Seed venues
        await SeedVenuesAsync(applicationDbContext, logger);

        // Create event store tables
        eventStoreContext.Database.EnsureCreated();

        // Ensure StoredEvent table exists (workaround for multiple context issue)
        await EnsureStoredEventTableAsync(eventStoreContext, logger);

        // Verify Identity tables exist
        logger.LogInformation("Verifying Identity database schema...");

        try
        {
            var userCount = await authDbContext.Users.CountAsync();
            logger.LogInformation("Identity tables are available. User count: {Count}", userCount);

            // Seed default roles
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            await SeedRolesAsync(roleManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Identity tables are not available: {Message}", ex.Message);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

app.Run();

static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
{
    logger.LogInformation("Seeding default roles...");

    var roles = new[] { Roles.Admin, Roles.User };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
            {
                logger.LogInformation("Role '{Role}' created successfully", role);
            }
            else
            {
                logger.LogError("Failed to create role '{Role}': {Errors}", role, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Role '{Role}' already exists", role);
        }
    }
}

static async Task SeedVenuesAsync(DDD.Infra.Data.Context.ApplicationDbContext context, ILogger logger)
{
    logger.LogInformation("Seeding venues...");

    if (await context.Venues.AnyAsync())
    {
        logger.LogInformation("Venues already exist, skipping seed");
        return;
    }

    var venues = new[]
    {
        new DDD.Domain.Models.Venue(Guid.NewGuid(), "Conference Center A", "123 Main Street, Downtown", 500),
        new DDD.Domain.Models.Venue(Guid.NewGuid(), "Grand Ballroom", "456 Oak Avenue, Business District", 300),
        new DDD.Domain.Models.Venue(Guid.NewGuid(), "Tech Hub Auditorium", "789 Pine Street, Tech Quarter", 150),
        new DDD.Domain.Models.Venue(Guid.NewGuid(), "Meeting Room Alpha", "321 Elm Street, Corporate Plaza", 50),
        new DDD.Domain.Models.Venue(Guid.NewGuid(), "Exhibition Hall", "654 Cedar Avenue, Convention Center", 1000),
    };

    context.Venues.AddRange(venues);
    await context.SaveChangesAsync();

    logger.LogInformation("Successfully seeded {Count} venues", venues.Length);
}

static async Task EnsureStoredEventTableAsync(DDD.Infra.Data.Context.EventStoreSqlContext context, ILogger logger)
{
    logger.LogInformation("Ensuring StoredEvent table exists...");

    try
    {
        // Try to query the StoredEvent table to see if it exists
        await context.StoredEvent.AnyAsync();
        logger.LogInformation("StoredEvent table exists and is accessible");
    }
    catch (Exception)
    {
        logger.LogWarning("StoredEvent table does not exist, creating it...");

        // Create the StoredEvent table using raw SQL with correct column names from StoredEventMap
        var createTableSql = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='StoredEvent' AND xtype='U')
            BEGIN
                CREATE TABLE [StoredEvent] (
                    [Id] uniqueidentifier NOT NULL,
                    [AggregateId] uniqueidentifier NOT NULL,
                    [Action] varchar(100) NULL,
                    [Data] nvarchar(max) NULL,  
                    [User] nvarchar(max) NULL,
                    [CreationDate] datetime2 NOT NULL,
                    CONSTRAINT [PK_StoredEvent] PRIMARY KEY ([Id])
                );
            END";

        await context.Database.ExecuteSqlRawAsync(createTableSql);
        logger.LogInformation("StoredEvent table created successfully");
    }
}

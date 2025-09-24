using EventManagement.Web.Services;
using EventManagement.Web.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configure JWT validation - these should match the API's JWT settings
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // Set to true in production with proper issuer
        ValidateAudience = false, // Set to true in production with proper audience
        ValidateLifetime = true,
        ValidateIssuerSigningKey = false, // We'll validate tokens via API calls
        ClockSkew = TimeSpan.Zero
    };
    
    // Allow token from session instead of header for web app
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.HttpContext.Session.GetString("access_token");
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Allow anonymous access to authentication pages and public pages
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AllowAnonymousToPage("/");
    options.Conventions.AllowAnonymousToPage("/Privacy");
    
    // For now, allow anonymous access to Events pages as well
    // TODO: Implement proper JWT-based authorization middleware
    options.Conventions.AllowAnonymousToFolder("/Events");
    options.Conventions.AllowAnonymousToFolder("/Account");
});

// Configure API HttpClient for backend communication
builder.Services.AddHttpClient("EventAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7200/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure Authentication HttpClient for DDD.Services.Api authentication endpoints
builder.Services.AddHttpClient<AuthenticationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7200/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register custom services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IEventApiService, EventApiService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Add SignalR for real-time features
builder.Services.AddSignalR();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// Use custom JWT authentication middleware instead of built-in JWT middleware
app.UseMiddleware<EventManagement.Web.Middleware.JwtAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Map SignalR hubs
app.MapHub<EventNotificationHub>("/hubs/eventNotification");

app.Run();

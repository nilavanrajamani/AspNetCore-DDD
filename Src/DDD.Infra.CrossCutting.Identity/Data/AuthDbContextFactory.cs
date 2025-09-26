using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DDD.Infra.CrossCutting.Identity.Data;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        // Use the connection string from arguments or default LocalDB
        var connectionString = args.Length > 0 ? args[0] :
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DDD_Db;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        optionsBuilder.UseSqlServer(connectionString);

        return new AuthDbContext(optionsBuilder.Options);
    }
}
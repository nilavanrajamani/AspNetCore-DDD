using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DDD.Infra.Data.Context;

public class EventStoreSqlContextFactory : IDesignTimeDbContextFactory<EventStoreSqlContext>
{
    public EventStoreSqlContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventStoreSqlContext>();

        // Use the connection string from arguments or default LocalDB
        var connectionString = args.Length > 0 ? args[0] :
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DDD_Db;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        optionsBuilder.UseSqlServer(connectionString);

        return new EventStoreSqlContext(optionsBuilder.Options);
    }
}
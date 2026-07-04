using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Wallet.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
  public ApplicationDbContext CreateDbContext(string[] args)
  {
    var basePath = Directory.GetCurrentDirectory();

    var configuration = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    var connectionString = configuration.GetConnectionString("DefaultConnection");
    var databaseType = configuration.GetValue<string>("ConnectionStrings:Type");

    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    if (databaseType == "SqlServer")
    {
      optionsBuilder.UseSqlServer(connectionString,
        sqlOptions =>
        {
          sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        });
    }
    else if (databaseType == "Npgsql")
    {
      optionsBuilder.UseNpgsql(
        connectionString,
        sqlOptions =>
        {
          sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        });
    }
    else
    {
      throw new InvalidOperationException($"Unsupported database type: {databaseType}");
    }

    return new ApplicationDbContext(optionsBuilder.Options);
  }
}
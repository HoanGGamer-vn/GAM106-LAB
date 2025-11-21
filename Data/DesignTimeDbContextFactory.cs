using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GAM106_LAB.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();
            var conn = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            if (!string.IsNullOrWhiteSpace(conn))
            {
                if (conn.Trim().StartsWith("Data Source=", System.StringComparison.OrdinalIgnoreCase) || conn.Trim().EndsWith('.' + "db", System.StringComparison.OrdinalIgnoreCase))
                {
                    optionsBuilder.UseSqlite(conn);
                }
                else
                {
                    optionsBuilder.UseSqlServer(conn);
                }
            }
            else
            {
                optionsBuilder.UseSqlite("Data Source=GAM106-LAB.db");
            }

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

using Interview.Domain.Events.ChangeEntityProcessors;
using Interview.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Interview.Migrations.Postgres
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var builder = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=localhost;Username=devpav;Password=devpav;Database=postgres", b => b.MigrationsAssembly(typeof(AppDbContextFactory).Assembly.FullName));
            return new AppDbContext(builder.Options);
        }
    }
}

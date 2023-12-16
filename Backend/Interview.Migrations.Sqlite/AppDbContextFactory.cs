using System;
using Interview.Domain.Events.ChangeEntityProcessors;
using Interview.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Interview.Migrations.Sqlite
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=app.db", b => b.MigrationsAssembly(typeof(AppDbContextFactory).Assembly.FullName));
            return new AppDbContext(builder.Options);
        }
    }
}

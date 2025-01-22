using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Users;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Moq;

namespace Interview.Test;

public class TestAppDbContextFactory
{
    public AppDbContext Create(ISystemClock clock)
    {
        var sqliteConnection = new SqliteConnection("Data Source=:memory:");
        sqliteConnection.Open();

        var option = new DbContextOptionsBuilder().UseSqlite(
            sqliteConnection
        );

        var serviceCollection = new ServiceCollection()
            .AddSingleton<IEntityPreProcessor>(new DateEntityPreProcessor(Mock.Of<ICurrentUserAccessor>(), clock)
            {
                TestEnv = true,
            });

        var context = new AppDbContext(option.Options)
        {
            SystemClock = clock,
            Processors = new LazyPreProcessors(serviceCollection.BuildServiceProvider())
        };

        context.Database.EnsureCreated();
        AddUser(context, Guid.Empty, "UnitTests");
        return context;
    }

    public static void AddUser(AppDbContext db, Guid id, string nickname, bool saveChanges = true)
    {
        db.Users.Add(new User(id, nickname, nickname + "Identity"));
        if (saveChanges)
        {
            db.SaveChanges();
            db.ChangeTracker.Clear();   
        }
    }
}

using Interview.Domain.Categories;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events;
using Interview.Domain.Invites;
using Interview.Domain.Questions;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Reactions;
using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.RoomTimers;
using Interview.Domain.Tags;
using Interview.Domain.Users;
using Interview.Domain.Users.Permissions;
using Interview.Domain.Users.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Internal;
using RoomConfiguration = Interview.Domain.Rooms.RoomConfigurations.RoomConfiguration;

namespace Interview.Domain.Database;

public class AppDbContext : DbContext
{
    public ISystemClock SystemClock { get; set; } = new SystemClock();

    public LazyPreProcessors Processors { get; set; } = null!;

    public AppDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; private set; } = null!;

    public DbSet<Question> Questions { get; private set; } = null!;

    public DbSet<Room> Rooms { get; private set; } = null!;

    public DbSet<Role> Roles { get; private set; } = null!;

    public DbSet<Reaction> Reactions { get; private set; } = null!;

    public DbSet<RoomParticipant> RoomParticipants { get; private set; } = null!;

    public DbSet<RoomQuestion> RoomQuestions { get; private set; } = null!;

    public DbSet<RoomQuestionReaction> RoomQuestionReactions { get; private set; } = null!;

    public DbSet<RoomConfiguration> RoomConfiguration { get; private set; } = null!;

    public DbSet<Tag> Tag { get; private set; } = null!;

    public DbSet<Permission> Permissions { get; private set; } = null!;

    public DbSet<AppEvent> AppEvent { get; private set; } = null!;

    public DbSet<RoomState> RoomStates { get; private set; } = null!;

    public DbSet<QueuedRoomEvent> QueuedRoomEvents { get; private set; } = null!;

    public DbSet<DbRoomEvent> RoomEvents { get; private set; } = null!;

    public DbSet<Invite> Invites { get; private set; } = null!;

    public DbSet<RoomInvite> RoomInvites { get; private set; } = null!;

    public DbSet<RoomTimer> RoomTimers { get; private set; } = null!;

    public DbSet<AvailableRoomPermission> AvailableRoomPermission { get; private set; } = null!;

    public DbSet<Category> Categories { get; private set; } = null!;

    public DbSet<QuestionAnswer> QuestionAnswers { get; private set; } = null!;

    public override int SaveChanges()
    {
        using var saveCookie = new SaveCookie(this, CancellationToken.None);
        saveCookie.NotifyPreProcessors();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        await using var saveCookie = new SaveCookie(this, cancellationToken);
        await saveCookie.NotifyPreProcessorsAsync();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private readonly struct SaveCookie : IDisposable, IAsyncDisposable
    {
        private readonly AppDbContext _db;
        private readonly CancellationToken _cancellationToken;
        private readonly List<Entity>? _addedEntities;
        private readonly List<(Entity Original, Entity Current)>? _modifiedEntities;

        private readonly List<IEntityPreProcessor> _preProcessors;
        private readonly List<IEntityPostProcessor> _postProcessors;

        public SaveCookie(AppDbContext db, CancellationToken cancellationToken)
        {
            _db = db;
            _cancellationToken = cancellationToken;

            _preProcessors = _db.Processors.PreProcessors;
            _postProcessors = _db.Processors.PostProcessors;

            _addedEntities = FilterByState(EntityState.Added).ToList();
            _modifiedEntities = FilterEntryByState(EntityState.Modified)
                .Select(e =>
                {
                    var original = (Entity)e.OriginalValues.ToObject();
                    return (original, e.Entity);
                })
                .ToList();
        }

        public void NotifyPreProcessors()
        {
            if (_preProcessors.Count == 0 || (_addedEntities == null && _modifiedEntities == null))
            {
                return;
            }

            foreach (var preProcessor in _preProcessors)
            {
                if (_addedEntities != null)
                {
                    preProcessor.ProcessAddedAsync(_addedEntities, _cancellationToken)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }

                if (_modifiedEntities != null)
                {
                    preProcessor.ProcessModifiedAsync(_modifiedEntities, _cancellationToken)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }
            }
        }

        public async ValueTask NotifyPreProcessorsAsync()
        {
            if (_preProcessors.Count == 0 || (_addedEntities == null && _modifiedEntities == null))
            {
                return;
            }

            foreach (var preProcessor in _preProcessors)
            {
                if (_addedEntities != null)
                {
                    await preProcessor.ProcessAddedAsync(_addedEntities, _cancellationToken);
                }

                if (_modifiedEntities != null)
                {
                    await preProcessor.ProcessModifiedAsync(_modifiedEntities, _cancellationToken);
                }
            }
        }

        public void Dispose()
        {
            if (_postProcessors.Count == 0 || _addedEntities == null || _modifiedEntities == null)
            {
                return;
            }

            foreach (var processor in _postProcessors)
            {
                processor.ProcessAddedAsync(_addedEntities, _cancellationToken).ConfigureAwait(false).GetAwaiter()
                    .GetResult();
                processor.ProcessModifiedAsync(_modifiedEntities, _cancellationToken).ConfigureAwait(false).GetAwaiter()
                    .GetResult();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_postProcessors.Count == 0 || _addedEntities == null || _modifiedEntities == null)
            {
                return;
            }

            foreach (var processor in _postProcessors)
            {
                await processor.ProcessAddedAsync(_addedEntities, _cancellationToken);
                await processor.ProcessModifiedAsync(_modifiedEntities, _cancellationToken);
            }
        }

        private IEnumerable<Entity> FilterByState(EntityState entityState)
        {
            return FilterEntryByState(entityState).Select(e => e.Entity);
        }

        private IEnumerable<EntityEntry<Entity>> FilterEntryByState(EntityState entityState)
        {
            foreach (var entityEntry in _db.ChangeTracker.Entries<Entity>())
            {
                if (entityEntry.State != entityState)
                {
                    continue;
                }

                yield return entityEntry;
            }
        }
    }
}

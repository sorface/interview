using Interview.Domain.Categories;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events;
using Interview.Domain.Invites;
using Interview.Domain.Questions;
using Interview.Domain.Questions.CodeEditors;
using Interview.Domain.Questions.QuestionAnswers;
using Interview.Domain.Reactions;
using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Rooms.RoomReviews;
using Interview.Domain.Rooms.RoomTimers;
using Interview.Domain.Tags;
using Interview.Domain.Users;
using Interview.Domain.Users.Permissions;
using Interview.Domain.Users.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Internal;
using RoomConfiguration = Interview.Domain.Rooms.RoomConfigurations.RoomConfiguration;

namespace Interview.Domain.Database;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public ISystemClock SystemClock { get; set; } = new SystemClock();

    public LazyPreProcessors Processors { get; set; } = null!;

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

    public DbSet<Category> Categories { get; private set; } = null!;

    public DbSet<QuestionAnswer> QuestionAnswers { get; private set; } = null!;

    public DbSet<QuestionCodeEditor> QuestionCodeEditors { get; private set; } = null!;

    public DbSet<RoomQuestionEvaluation> RoomQuestionEvaluation { get; private set; } = null!;

    public DbSet<RoomReview> RoomReview { get; private set; } = null!;

    public DbSet<QuestionTree> QuestionTree { get; private set; } = null!;

    public DbSet<QuestionSubjectTree> QuestionSubjectTree { get; private set; } = null!;

    public async Task<HashSet<Guid>> GetWithChildCategoriesAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var res = new HashSet<Guid> { categoryId };
        var checkCategories = new List<Guid> { categoryId };
        do
        {
            var tmp = await Categories.AsNoTracking()
                .Where(e => e.ParentId != null && checkCategories.Contains(e.ParentId.Value))
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);
            checkCategories.Clear();
            foreach (var id in tmp)
            {
                if (res.Add(id))
                {
                    checkCategories.Add(id);
                }
            }
        }
        while (checkCategories.Count > 0);

        return res;
    }

    /// <summary>
    /// Runs code within a transaction, taking into account previously created code.
    /// </summary>
    /// <param name="action">Action for transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TRes">Result type.</typeparam>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
    public async Task<TRes> RunTransactionAsync<TRes>(
        Func<CancellationToken, Task<TRes>> action,
        CancellationToken cancellationToken)
    {
        using var transaction = Database.CurrentTransaction is not null
            ? EmptyDisposable.Instance
            : await Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var res = await action(cancellationToken);
            if (transaction is IDbContextTransaction typedTransaction)
            {
                await typedTransaction.CommitAsync(cancellationToken);
            }

            return res;
        }
        catch (Exception)
        {
            if (transaction is IDbContextTransaction typedTransaction)
            {
                await typedTransaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
    }

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

        private readonly List<IEntityPreProcessor> _preProcessors;
        private readonly List<IEntityPostProcessor> _postProcessors;
        private readonly MutableEntityState _entityState;

        public SaveCookie(AppDbContext db, CancellationToken cancellationToken)
        {
            _db = db;
            _cancellationToken = cancellationToken;

            _preProcessors = _db.Processors.PreProcessors;
            _postProcessors = _db.Processors.PostProcessors;

            var (added, modifiedEntities) = GetChangedEntities();
            _entityState = new MutableEntityState { Added = added, ModifiedEntities = modifiedEntities };
        }

        public void NotifyPreProcessors()
        {
            if (_preProcessors.Count == 0 || !_entityState.HasEntries)
            {
                return;
            }

            foreach (var preProcessor in _preProcessors)
            {
                preProcessor.ProcessAddedAsync(_entityState.Added, _cancellationToken)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                preProcessor.ProcessModifiedAsync(_entityState.ModifiedEntities, _cancellationToken)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }

            var (added, modifiedEntities) = GetChangedEntities();
            _entityState.Added = added;
            _entityState.ModifiedEntities = modifiedEntities;
        }

        public async ValueTask NotifyPreProcessorsAsync()
        {
            if (_preProcessors.Count == 0 || !_entityState.HasEntries)
            {
                return;
            }

            foreach (var preProcessor in _preProcessors)
            {
                await preProcessor.ProcessAddedAsync(_entityState.Added, _cancellationToken);
                await preProcessor.ProcessModifiedAsync(_entityState.ModifiedEntities, _cancellationToken);
            }

            var (added, modifiedEntities) = GetChangedEntities();
            _entityState.Added = added;
            _entityState.ModifiedEntities = modifiedEntities;
        }

        public void Dispose()
        {
            if (_postProcessors.Count == 0 || !_entityState.HasEntries)
            {
                return;
            }

            foreach (var processor in _postProcessors)
            {
                processor.ProcessAddedAsync(_entityState.Added, _cancellationToken).ConfigureAwait(false).GetAwaiter()
                    .GetResult();
                processor.ProcessModifiedAsync(_entityState.ModifiedEntities, _cancellationToken).ConfigureAwait(false).GetAwaiter()
                    .GetResult();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_postProcessors.Count == 0 || !_entityState.HasEntries)
            {
                return;
            }

            foreach (var processor in _postProcessors)
            {
                await processor.ProcessAddedAsync(_entityState.Added, _cancellationToken);
                await processor.ProcessModifiedAsync(_entityState.ModifiedEntities, _cancellationToken);
            }
        }

        private (List<Entity> Added, List<(Entity Original, Entity Entity)> ModifiedEntities) GetChangedEntities()
        {
            var addedEntities = FilterByState(EntityState.Added).ToList();
            var modifiedEntities = FilterEntryByState(EntityState.Modified)
                .Select(e =>
                {
                    var original = (Entity)e.OriginalValues.ToObject();
                    return (original, e.Entity);
                })
                .ToList();
            return (addedEntities, modifiedEntities);
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

        private sealed class MutableEntityState
        {
            public required List<Entity> Added { get; set; }

            public required List<(Entity Original, Entity Entity)> ModifiedEntities { get; set; }

            public bool HasEntries => Added.Count > 0 || ModifiedEntities.Count > 0;
        }
    }
}

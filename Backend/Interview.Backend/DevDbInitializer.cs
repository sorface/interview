using Interview.Domain.Categories;
using Interview.Domain.Database;

namespace Interview.Backend;

public class DevDbInitializer
{
    private readonly AppDbContext _appDbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DevDbInitializer> _logger;

    public DevDbInitializer(AppDbContext appDbContext, IWebHostEnvironment environment, IConfiguration configuration, ILogger<DevDbInitializer> logger)
    {
        _appDbContext = appDbContext;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return;
        }

        await AddDevUserAsync(cancellationToken);
        await AddInitialDataAsync(cancellationToken);
    }

    private async Task AddInitialDataAsync(CancellationToken cancellationToken)
    {
        var dbData = _configuration.GetSection(nameof(InitialDbData)).Get<InitialDbData>();
        if (dbData is null)
        {
            _logger.LogInformation("Not found Initial Database Data");
            return;
        }

        var requiredCategories = dbData.Categories.Select(e => e.Id).ToList();
        if (_appDbContext.Categories.Any(e => requiredCategories.Contains(e.Id)))
        {
            _logger.LogInformation("The initial data has already been added and you don't need to add anything.");
            return;
        }

        foreach (var initialCategories in dbData.GetCategoriesForCreate())
        {
            var categories = initialCategories.Select(e => new Category { Name = e.Name, Id = e.Id, ParentId = e.ParentId, });
            await _appDbContext.Categories.AddRangeAsync(categories, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        var questions = dbData.Questions.Select(e => new Question(e.Value) { Id = e.Id, CategoryId = e.CategoryId, });
        await _appDbContext.Questions.AddRangeAsync(questions, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AddDevUserAsync(CancellationToken cancellationToken)
    {
        var testUserId = Guid.Parse("b5a05f34-e44d-11ed-b49f-e8e34e3377ec");
        if (_appDbContext.Users.Any(e => e.Id == testUserId))
        {
            return;
        }

        var addUser = new User("TEST_BACKEND_DEV_USER", "d1731c50-e44d-11ed-905c-d08c09609150")
        {
            Id = testUserId,
            Avatar = null,
            Roles =
            {
                (await _appDbContext.Roles.FindAsync(RoleName.User.Id))!,
            },
        };
        await _appDbContext.Users.AddAsync(addUser, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class InitialDbData
    {
        public required InitialCategory[] Categories { get; set; }

        public required InitialQuestion[] Questions { get; set; }

        public IEnumerable<IEnumerable<Category>> GetCategoriesForCreate()
        {
            var actualCategories = Categories.Select(e => new Category
            {
                Name = e.Name,
                Id = e.Id,
            }).ToList();
            yield return actualCategories;

            var categoryThree = new Stack<(Guid ParentId, IReadOnlyCollection<InitialCategory> Child)>();
            foreach (var initialCategory in Categories)
            {
                if (initialCategory.Children is null || initialCategory.Children.Count == 0)
                {
                    continue;
                }

                categoryThree.Push((initialCategory.Id, initialCategory.Children));
            }

            while (categoryThree.TryPop(out var currentValue))
            {
                yield return currentValue.Child.Select(e => new Category { Id = e.Id, Name = e.Name, ParentId = currentValue.ParentId, });

                foreach (var initialCategory in currentValue.Child)
                {
                    if (initialCategory.Children is null || initialCategory.Children.Count == 0)
                    {
                        continue;
                    }

                    categoryThree.Push((initialCategory.Id, initialCategory.Children));
                }
            }
        }
    }

    private sealed class InitialCategory
    {
        public required Guid Id { get; set; }

        public required string Name { get; set; }

        public List<InitialCategory>? Children { get; set; }
    }

    private sealed class InitialQuestion
    {
        public required Guid Id { get; set; }

        public required string Value { get; set; }

        public Guid? CategoryId { get; set; }
    }
}

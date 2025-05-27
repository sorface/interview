using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Categories;
using Interview.Domain.Categories.Edit;
using Interview.Domain.Categories.Page;

namespace Interview.Test.Integrations;

public class CategoryServiceTest
{
    [Fact]
    public async Task Create()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var initialCount = appDbContext.Categories.Count();
        var service = new CategoryService(appDbContext, new ArchiveService<Category>(appDbContext));

        var request = new CategoryEditRequest
        {
            Name = "Test",
        };
        var result = await service.CreateAsync(request, CancellationToken.None);
        var actualCount = appDbContext.Categories.Count();

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Name.Should().Be("Test");
        initialCount.Should().Be(0);
        actualCount.Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public async Task Create_Empty_Name(string? name)
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var initialCount = appDbContext.Categories.Count();
        var service = new CategoryService(appDbContext, new ArchiveService<Category>(appDbContext));

        var request = new CategoryEditRequest
        {
            Name = name,
        };
        var result = await service.CreateAsync(request, CancellationToken.None);
        var actualCount = appDbContext.Categories.Count();

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Category should not be empty");
        initialCount.Should().Be(0);
        actualCount.Should().Be(0);
    }

    [Fact]
    public async Task Update()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var category = new Category { Name = "Test" };
        appDbContext.Categories.Add(category);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();
        var service = new CategoryService(appDbContext, new ArchiveService<Category>(appDbContext));

        var request = new CategoryEditRequest
        {
            Name = "Test 2",
        };
        var result = await service.UpdateAsync(category.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Name.Should().Be("Test 2");
    }

    [Fact]
    public async Task Update_Child_Category_As_Parent()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var category = new Category
        {
            Name = "Test",
            Parent = new Category
            {
                Name = "Test parent",
            }
        };
        appDbContext.Categories.Add(category);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();
        var service = new CategoryService(appDbContext, new ArchiveService<Category>(appDbContext));

        var request = new CategoryEditRequest
        {
            Name = "Test parent 2",
            ParentId = category.Id
        };
        var result = await service.UpdateAsync(category.ParentId!.Value, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("A child category cannot be specified as a parent category");
    }

    [Fact]
    public async Task Update_Child_Category_As_Parent_2_level()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var category = new Category
        {
            Name = "Test",
            Parent = new Category
            {
                Name = "Test parent 1",
                Parent = new Category
                {
                    Name = "Test parent 2"
                }
            }
        };
        appDbContext.Categories.Add(category);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();
        var service = new CategoryService(appDbContext, new ArchiveService<Category>(appDbContext));

        var request = new CategoryEditRequest
        {
            Name = "Test parent 2",
            ParentId = category.Id
        };
        var result = await service.UpdateAsync(category.Parent!.ParentId!.Value, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("A child category cannot be specified as a parent category");
    }

    [Fact]
    public async Task FindPageA()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var category = new Category
        {
            Name = "Test",
            Parent = new Category
            {
                Name = "Test parent 1",
                Parent = new Category
                {
                    Name = "Test parent 2"
                }
            }
        };
        appDbContext.Categories.Add(category);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();
        var service = new CategoryService(appDbContext, new ArchiveService<Category>(appDbContext));

        var request = new CategoryPageRequest
        {
            Filter = new CategoryPageRequestFilter
            {
                ParentId = null,
                Name = null
            },
            Page = new PageRequest
            {
                PageNumber = 1,
                PageSize = 30,
            }
        };
        var result = await service.FindPageAsync(request, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().HaveCount(3)
            .And.ContainSingle(e => e.Name == "Test")
            .And.ContainSingle(e => e.Name == "Test parent 1")
            .And.ContainSingle(e => e.Name == "Test parent 2");
    }

    [Fact]
    public async Task FindPage_With_EditingCategoryId()
    {
        await using var appDbContext = new TestAppDbContextFactory().Create(new TestSystemClock());
        var category = new Category
        {
            Name = "Test",
            Parent = new Category
            {
                Name = "Test parent 1",
                Parent = new Category
                {
                    Name = "Test parent 2",
                }
            }
        };
        appDbContext.Categories.Add(category);
        await appDbContext.SaveChangesAsync();
        appDbContext.ChangeTracker.Clear();
        var service = new CategoryService(appDbContext, new ArchiveService<Category>(appDbContext));

        var request = new CategoryPageRequest
        {
            Filter = new CategoryPageRequestFilter
            {
                EditingCategoryId = category.ParentId,
                Name = null,
                ParentId = null
            },
            Page = new PageRequest
            {
                PageNumber = 1,
                PageSize = 30,
            }
        };
        var result = await service.FindPageAsync(request, CancellationToken.None);

        result.Should().HaveCount(2)
            .And.ContainSingle(e => e.Name == "Test parent 1")
            .And.ContainSingle(e => e.Name == "Test parent 2");
    }
}

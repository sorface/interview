using CSharpFunctionalExtensions;
using Interview.Domain.Categories.Edit;
using Interview.Domain.Categories.Page;
using Interview.Domain.Database;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Categories;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    private readonly ArchiveService<Category> _archiveService;

    public CategoryService(AppDbContext db, ArchiveService<Category> archiveService)
    {
        _db = db;
        _archiveService = archiveService;
    }

    public async Task<CategoryResponse> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _db.Categories.AsNoTracking()
            .Where(e => e.Id == id)
            .Select(CategoryResponse.Mapper.Expression)
            .FirstOrDefaultAsync(cancellationToken);
        if (response is null)
        {
            throw NotFoundException.Create<Category>(id);
        }

        return response;
    }

    public Task<IPagedList<CategoryResponse>> FindPageAsync(CategoryPageRequest request, CancellationToken cancellationToken)
    {
        return FindPageCoreAsync(false, request, cancellationToken);
    }

    public Task<IPagedList<CategoryResponse>> FindArchivePageAsync(CategoryPageRequest request, CancellationToken cancellationToken)
    {
        return FindPageCoreAsync(true, request, cancellationToken);
    }

    public async Task<Result<ServiceResult<CategoryResponse>, ServiceError>> CreateAsync(CategoryEditRequest request, CancellationToken cancellationToken)
    {
        var error = await EnsureValidAsync(request, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        request.Name = request.Name!.Trim();
        var category = new Category
        {
            Name = request.Name,
            ParentId = request.ParentId,
        };
        await _db.Categories.AddAsync(category, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        var response = CategoryResponse.Mapper.Map(category);
        return ServiceResult.Ok(response);
    }

    public async Task<Result<ServiceResult<CategoryResponse>, ServiceError>> UpdateAsync(Guid id, CategoryEditRequest request, CancellationToken cancellationToken)
    {
        var error = await EnsureValidAsync(request, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        request.Name = request.Name!.Trim();
        var category = await _db.Categories.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceError.NotFound($"Not found category by id '{id}'");
        }

        category.Name = request.Name.Trim();
        category.ParentId = request.ParentId;
        await _db.SaveChangesAsync(cancellationToken);
        var response = CategoryResponse.Mapper.Map(category);
        return ServiceResult.Ok(response);
    }

    public async Task<CategoryResponse> ArchiveAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _archiveService.ArchiveAsync(id, cancellationToken);
        return CategoryResponse.Mapper.Map(result);
    }

    public async Task<CategoryResponse> UnarchiveAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _archiveService.UnarchiveAsync(id, cancellationToken);
        return CategoryResponse.Mapper.Map(result);
    }

    private Task<IPagedList<CategoryResponse>> FindPageCoreAsync(bool archive, CategoryPageRequest request, CancellationToken cancellationToken)
    {
        var filter = BuildSpecification(archive, request);
        return _db.Categories
            .AsNoTracking()
            .Where(filter)
            .OrderBy(e => e.Name)
            .Select(CategoryResponse.Mapper.Expression)
            .ToPagedListAsync(request.Page.PageNumber, request.Page.PageSize, cancellationToken);

        static ASpec<Category> BuildSpecification(bool archive, CategoryPageRequest request)
        {
            ASpec<Category> res = new Spec<Category>(e => e.IsArchived == archive);
            if (request.Filter is null)
            {
                return res;
            }

            if (request.Filter.ParentId is not null)
            {
                res &= new Spec<Category>(e => e.ParentId == request.Filter.ParentId);
            }
            else if (request.Filter.ShowOnlyWithoutParent)
            {
                res &= new Spec<Category>(e => e.ParentId == null);
            }

            var name = request.Filter.Name?.Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                res &= new Spec<Category>(e => e.Name.Contains(name));
            }

            return res;
        }
    }

    private Task<ServiceError?> EnsureValidAsync(CategoryEditRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Task.FromResult<ServiceError?>(ServiceError.Error("Category should not be empty"));
        }

        return Category.ValidateParentAsync(_db, request.ParentId, cancellationToken);
    }
}

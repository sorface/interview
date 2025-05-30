using CSharpFunctionalExtensions;
using Interview.Domain.Repository;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using Interview.Domain.Tags.Records.Response;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Tags;

public class TagService(ITagRepository tagRepository) : ITagService
{
    public Task<IPagedList<TagItem>> FindTagsPageAsync(
        string? value, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        value = value?.Trim();
        var mapper =
            new Mapper<Tag, TagItem>(
                question => new TagItem
                {
                    Id = question.Id,
                    Value = question.Value,
                    HexValue = question.HexColor,
                });

        value = value?.ToLower();
        var specification = !string.IsNullOrWhiteSpace(value) ?
#pragma warning disable CA1862
            new Spec<Tag>(tag => tag.Value.ToLower().Contains(value)) :
#pragma warning restore CA1862
            Spec<Tag>.Any;
        return tagRepository.GetPageAsync(specification, mapper, pageNumber, pageSize, cancellationToken);
    }

    public async Task<Result<ServiceResult<TagItem>, ServiceError>> CreateTagAsync(TagEditRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
        {
            return ServiceError.Error("Tag should not be empty");
        }

        var hexValue = request.HexValue ?? string.Empty;
        if (!Tag.IsValidColor(hexValue))
        {
            return ServiceError.Error("Tag should contain valid hex value.");
        }

        request.Value = request.Value.Trim();
        var hasTag = await tagRepository.HasAsync(new Spec<Tag>(e => e.Value == request.Value), cancellationToken);
        if (hasTag)
        {
            return ServiceError.Error($"Already exists tag '{request.Value}'");
        }

        var tag = new Tag
        {
            Value = request.Value,
            HexColor = hexValue,
        };
        await tagRepository.CreateAsync(tag, cancellationToken);
        return ServiceResult.Created(new TagItem
        {
            Id = tag.Id,
            Value = tag.Value,
            HexValue = tag.HexColor,
        });
    }

    public async Task<Result<ServiceResult<TagItem>, ServiceError>> UpdateTagAsync(Guid id, TagEditRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
        {
            return ServiceError.Error("Tag should not be empty");
        }

        if (!Tag.IsValidColor(request.HexValue))
        {
            return ServiceError.Error("Tag should contain valid hex value.");
        }

        request.Value = request.Value.Trim();
        var hasTag = await tagRepository.HasAsync(new Spec<Tag>(e => e.Value == request.Value && e.Id != id), cancellationToken);
        if (hasTag)
        {
            return ServiceError.Error($"Already exists tag '{request.Value}'");
        }

        var tag = await tagRepository.FindByIdAsync(id, cancellationToken);
        if (tag is null)
        {
            return ServiceError.NotFound($"Not found tag by id '{id}'");
        }

        tag.Value = request.Value;
        tag.HexColor = request.HexValue ?? string.Empty;
        await tagRepository.UpdateAsync(tag, cancellationToken);
        return ServiceResult.Ok(new TagItem
        {
            Id = tag.Id,
            Value = tag.Value,
            HexValue = tag.HexColor,
        });
    }
}

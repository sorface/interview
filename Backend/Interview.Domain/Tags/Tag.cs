using System.Globalization;
using CSharpFunctionalExtensions;
using Interview.Domain.ServiceResults.Errors;
using Entity = Interview.Domain.Repository.Entity;

namespace Interview.Domain.Tags;

public class Tag : Entity
{
    public string Value { get; internal set; } = string.Empty;

    public string HexColor { get; internal set; } = string.Empty;

    public static bool IsValidColor(string? color) => int.TryParse(color, NumberStyles.HexNumber, null, out _);

    public static async Task<List<Tag>> EnsureValidTagsAsync(ITagRepository tagRepository, IReadOnlySet<Guid> tagsForCheck, CancellationToken cancellationToken)
    {
        var requestTags = tagsForCheck.ToHashSet();
        var tags = await tagRepository.FindByIdsAsync(requestTags, cancellationToken);
        requestTags.ExceptWith(tags.Select(e => e.Id));
        var notFoundTags = string.Join(",", requestTags);

        if (!string.IsNullOrWhiteSpace(notFoundTags))
        {
            throw new NotFoundException($"Not found tags: [{notFoundTags}]");
        }

        return tags;
    }
}

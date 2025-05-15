using Interview.Domain.Database;
using Interview.Domain.Roadmaps.RoadmapPage;
using Interview.Domain.Roadmaps.UpsertRoadmap;
using Interview.Domain.ServiceResults.Success;
using Interview.Domain.Tags.Records.Response;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Roadmaps;

public class RoadmapService(AppDbContext db)
{
    public async Task<ServiceResult<Guid>> UpsertAsync(UpsertRoadmapRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpsertRoadmapRequestValidator();
        var result = validator.Validate(request);
        if (!result.IsValid)
        {
            throw new UserException(result.Errors);
        }

        Roadmap? roadmap = null;
        if (request.Id is not null)
        {
            roadmap = await db.Roadmap.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        }

        if (roadmap is null)
        {
            roadmap = await CreateRoadmapAsync(request, cancellationToken);
            return ServiceResult.Created(roadmap.Id);
        }

        await UpdateRoadmapAsync(roadmap, request, cancellationToken);
        return ServiceResult.Ok(roadmap.Id);
    }

    public async Task<IPagedList<RoadmapPageResponse>> FindPageAsync(FilteredRequest<RoadmapPageRequestFilter> request, CancellationToken cancellationToken)
    {
        var spec = BuildSpecification(request);
        var tmpRes = await db.Roadmap
            .AsNoTracking()
            .Include(e => e.Tags)
            .Include(e => e.Milestones).ThenInclude(e => e.Items)
            .Where(spec)
            .Select(e => new
            {
                Id = e.Id,
                Name = e.Name,
                Order = e.Order,
                Tags = e.Tags.Select(t => new TagItem
                {
                    Id = t.Id,
                    Value = t.Value,
                    HexValue = t.HexColor,
                }).ToList(),
                Items = e.Milestones.Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Order,
                    t.ParentRoadmapMilestoneId,
                    Items = t.Items.Select(tt => new
                    {
                        tt.Id,
                        tt.Order,
                        tt.QuestionTreeId,
                    }).OrderBy(tt => tt.Order).ToList(),
                }).OrderBy(t => t.Order).ToList(),
            })
            .OrderBy(e => e.Order)
            .ToPagedListAsync(request.Page, cancellationToken);
        var questionTreeIds = tmpRes.SelectMany(e =>
            e.Items.SelectMany(t =>
                t.Items.Select(tt => tt.QuestionTreeId)))
            .ToHashSet();
        var roomIdList = await db.Rooms
            .Where(e => e.QuestionTreeId != null && questionTreeIds.Contains(e.QuestionTreeId.Value))
            .Select(e => new { QuestionTreeId = e.QuestionTreeId!.Value, RoomId = e.Id })
            .ToListAsync(cancellationToken);
        var roomIdMap = roomIdList.ToLookup(e => e.QuestionTreeId);

        return tmpRes.ConvertAll(e => new RoadmapPageResponse
        {
            Id = e.Id,
            Name = e.Name,
            Order = e.Order,
            Tags = e.Tags,
            Items = e.Items.SelectMany(t =>
            {
                var items = new List<RoadmapPageItemResponse>(t.Items.Count + 1);
                items.Add(new RoadmapPageItemResponse
                {
                    Id = t.Id,
                    Type = EVRoadmapItemType.Milestone,
                    Name = t.Name,
                    QuestionTreeId = null,
                    RoomId = null,
                    Order = t.Order,
                });
                items.AddRange(t.Items.Select(item => new RoadmapPageItemResponse
                {
                    Id = item.Id,
                    Type = EVRoadmapItemType.QuestionTree,
                    Name = null,
                    QuestionTreeId = item.QuestionTreeId,
                    RoomId = roomIdMap[item.QuestionTreeId].FirstOrDefault()?.RoomId,
                    Order = item.Order,
                }));

                // TODO: add vertical split
                return items;
            }).ToList(),
        });

        static ASpec<Roadmap> BuildSpecification(FilteredRequest<RoadmapPageRequestFilter> request)
        {
            if (request.Filter is null)
            {
                return Spec<Roadmap>.Any;
            }

            var spec = Spec<Roadmap>.Any;
            if (request.Filter.Name is not null)
            {
                var filterName = request.Filter.Name?.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(filterName))
                {
                    spec = new Spec<Roadmap>(e => e.Name.ToLower().Contains(filterName));
                }
            }

            if (request.Filter.Tags is not null)
            {
                spec &= new Spec<Roadmap>(e => e.Tags.Any(t => request.Filter.Tags.Contains(t.Id)));
            }

            return spec;
        }
    }

    private Task<Roadmap> CreateRoadmapAsync(UpsertRoadmapRequest request, CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }

    private Task UpdateRoadmapAsync(Roadmap roadmap, UpsertRoadmapRequest request, CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }
}

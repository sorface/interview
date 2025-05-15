using Interview.Domain.Database;
using Interview.Domain.Roadmaps.RoadmapById;
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

    public async Task<RoadmapResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tmpRes = await db.Roadmap
            .AsNoTracking()
            .Include(e => e.Tags)
            .Include(e => e.Milestones).ThenInclude(e => e.Items)
            .Where(e => e.Id == id)
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
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (tmpRes is null)
        {
            throw NotFoundException.Create<Roadmap>(id);
        }

        var questionTreeIds = tmpRes.Items.SelectMany(t => t.Items.Select(tt => tt.QuestionTreeId)).ToHashSet();
        var roomIdList = await db.Rooms
            .Where(e => e.QuestionTreeId != null && questionTreeIds.Contains(e.QuestionTreeId.Value))
            .Select(e => new { QuestionTreeId = e.QuestionTreeId!.Value, RoomId = e.Id })
            .ToListAsync(cancellationToken);
        var roomIdMap = roomIdList.ToLookup(e => e.QuestionTreeId);

        var items = new List<RoadmapItemResponse>(tmpRes.Items.Count + 1 + tmpRes.Items.Select(e => e.Items.Count).Sum());
        for (var index = 0; index < tmpRes.Items.Count; index++)
        {
            var item = tmpRes.Items[index];
            if (item.ParentRoadmapMilestoneId is null && index > 0)
            {
                items.Add(new RoadmapItemResponse
                {
                    Id = null,
                    Type = EVRoadmapItemType.VerticalSplit,
                    Name = null,
                    QuestionTreeId = null,
                    RoomId = null,
                    Order = -1,
                });
            }

            items.Add(new RoadmapItemResponse
            {
                Id = item.Id,
                Type = EVRoadmapItemType.Milestone,
                Name = item.Name,
                QuestionTreeId = null,
                RoomId = null,
                Order = item.Order,
            });
            items.AddRange(item.Items.Select(e => new RoadmapItemResponse
            {
                Id = e.Id,
                Type = EVRoadmapItemType.QuestionTree,
                Name = null,
                QuestionTreeId = e.QuestionTreeId,
                RoomId = roomIdMap[e.QuestionTreeId].FirstOrDefault()?.RoomId,
                Order = e.Order,
            }));
        }

        return new RoadmapResponse
        {
            Id = tmpRes.Id,
            Name = tmpRes.Name,
            Order = tmpRes.Order,
            Tags = tmpRes.Tags,
            Items = items,
        };
    }

    public Task<IPagedList<RoadmapPageResponse>> FindPageAsync(FilteredRequest<RoadmapPageRequestFilter> request, CancellationToken cancellationToken)
    {
        var spec = BuildSpecification(request);
        return db.Roadmap
            .AsNoTracking()
            .Include(e => e.Tags)
            .Where(spec)
            .OrderBy(e => e.Order)
            .Select(e => new RoadmapPageResponse
            {
                Id = e.Id,
                Name = e.Name,
                Tags = e.Tags.Select(t => new TagItem
                {
                    Id = t.Id,
                    Value = t.Value,
                    HexValue = t.HexColor,
                }).ToList(),
            })
            .ToPagedListAsync(request.Page, cancellationToken);

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

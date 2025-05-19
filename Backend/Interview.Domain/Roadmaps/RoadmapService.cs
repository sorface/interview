using Interview.Domain.Database;
using Interview.Domain.Questions;
using Interview.Domain.Roadmaps.RoadmapById;
using Interview.Domain.Roadmaps.RoadmapPage;
using Interview.Domain.Roadmaps.UpsertRoadmap;
using Interview.Domain.ServiceResults.Success;
using Interview.Domain.Tags;
using Interview.Domain.Tags.Records.Response;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Roadmaps;

public class RoadmapService(AppDbContext db) : IRoadmapService
{
    public async Task<ServiceResult<Guid>> UpsertAsync(UpsertRoadmapRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpsertRoadmapRequestValidator();
        var result = validator.Validate(request);
        if (!result.IsValid)
        {
            throw new UserException(result.Errors);
        }

        var tags = await EnsureDbCheckValidateAsync(db, request, result.Tree, cancellationToken);

        Roadmap? roadmap = null;
        if (request.Id is not null)
        {
            var id = request.Id.Value;
            roadmap = await db.Roadmap
                .Include(e => e.Tags)
                .Include(e => e.Milestones).ThenInclude(e => e.Items)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (roadmap is null)
            {
                throw NotFoundException.Create<Roadmap>(id);
            }
        }

        if (roadmap is null)
        {
            roadmap = await db.RunTransactionAsync(ct => CreateRoadmapAsync(result.Tree, tags, request, ct), cancellationToken);
            return ServiceResult.Created(roadmap.Id);
        }

        await db.RunTransactionAsync(async ct =>
        {
            await UpdateRoadmapAsync(result.Tree, roadmap, tags, request, ct);
            return DBNull.Value;
        },
        cancellationToken);
        return ServiceResult.Ok(roadmap.Id);

        static async Task<List<Tag>> EnsureDbCheckValidateAsync(AppDbContext db,
                                                                UpsertRoadmapRequest request,
                                                                UpsertRoadmapRequestValidator.RoadmapTree tree,
                                                                CancellationToken cancellationToken)
        {
            var questionTreeIds = tree.RootMilestones
                .SelectMany(e => e.QuestionTrees)
                .Where(e => e.QuestionTreeId is not null)
                .Select(e => e.QuestionTreeId!.Value)
                .ToHashSet();
            var dbQuestionTrees = await db.QuestionTree.Where(e => questionTreeIds.Contains(e.Id))
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);

            questionTreeIds.ExceptWith(dbQuestionTrees);
            if (questionTreeIds.Count > 0)
            {
                throw NotFoundException.Create<QuestionTree>(questionTreeIds);
            }

            if (db.Roadmap.Any(e => e.Order == request.Order))
            {
                throw new UserException("Roadmap order should be unique");
            }

            List<Tag>? tags = null;
            if (request.Tags.Count > 0)
            {
                tags = await db.Tag.Where(e => request.Tags.Contains(e.Id)).ToListAsync(cancellationToken);
                var notFoundTags = request.Tags.Except(tags.Select(e => e.Id)).ToList();
                if (notFoundTags.Count > 0)
                {
                    throw NotFoundException.Create<Tag>(notFoundTags);
                }
            }

            return tags ?? [];
        }
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

    private async Task<Roadmap> CreateRoadmapAsync(
        UpsertRoadmapRequestValidator.RoadmapTree resultTree,
        List<Tag> tags,
        UpsertRoadmapRequest request,
        CancellationToken cancellationToken)
    {
        var roadmap = new Roadmap { Name = request.Name, Order = request.Order, Tags = tags };
        await db.Roadmap.AddAsync(roadmap, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var rootMilestones = new Stack<(Guid? Parent, UpsertRoadmapRequestValidator.RoadmapMilestoneNode Current)>(
            resultTree.RootMilestones.Select(e => ((Guid?)null, e)));
        while (rootMilestones.TryPop(out var milestone))
        {
            var roadmapMilestone = new RoadmapMilestone
            {
                Name = milestone.Current.Milestone.Name ?? string.Empty,
                Order = milestone.Current.Milestone.Order,
                RoadmapId = roadmap.Id,
                ParentRoadmapMilestoneId = milestone.Parent,
                Items = [],
            };
            await db.RoadmapMilestone.AddAsync(roadmapMilestone, cancellationToken);

            foreach (var e in milestone.Current.QuestionTrees)
            {
                var roadmapMilestoneItem = new RoadmapMilestoneItem
                {
                    RoadmapMilestoneId = roadmapMilestone.Id,
                    QuestionTreeId = e.QuestionTreeId!.Value,
                    Order = e.Order,
                };
                roadmapMilestone.Items.Add(roadmapMilestoneItem);
            }

            await db.SaveChangesAsync(cancellationToken);

            foreach (var roadmapMilestoneNode in milestone.Current.Children)
            {
                rootMilestones.Push((roadmapMilestone.Id, roadmapMilestoneNode));
            }
        }

        return roadmap;
    }

    private async Task UpdateRoadmapAsync(
        UpsertRoadmapRequestValidator.RoadmapTree resultTree,
        Roadmap roadmap,
        List<Tag> tags,
        UpsertRoadmapRequest request,
        CancellationToken cancellationToken)
    {
        var milestonesQuery = request.Items
            .Where(e => e is { Type: EVRoadmapItemType.Milestone, Id: not null })
            .ToList();
        var requiredRoadmapMilestones = milestonesQuery
            .Select(e => e.Id!.Value)
            .ToHashSet();
        if (requiredRoadmapMilestones.Count > 0)
        {
            var existsRoadmapMilestones = await db.RoadmapMilestone
                .Where(e => requiredRoadmapMilestones.Contains(e.Id))
                .Select(e => new { e.Id, e.RoadmapId })
                .ToListAsync(cancellationToken);
            var nonRoadmapMilestones = existsRoadmapMilestones.Where(e => e.RoadmapId != roadmap.Id).ToList();
            if (nonRoadmapMilestones.Count > 0)
            {
                throw new UserException($"These milestones [{string.Join(", ", nonRoadmapMilestones)}] are not related to the roadmap");
            }

            var notFoundMilestones = requiredRoadmapMilestones.Except(existsRoadmapMilestones.Select(e => e.Id)).ToList();
            if (notFoundMilestones.Count > 0)
            {
                throw NotFoundException.Create<RoadmapMilestone>(notFoundMilestones);
            }
        }

        roadmap.Tags.Clear();
        roadmap.Tags.AddRange(tags);
        roadmap.Order = request.Order;
        roadmap.Name = request.Name;

        // remove milestones
        roadmap.Milestones.RemoveAll(e => !requiredRoadmapMilestones.Contains(e.Id));
        var existsMilestoneMap = roadmap.Milestones.ToDictionary(e => e.Id, e => e);

        var rootMilestones = new Stack<(Guid? Parent, UpsertRoadmapRequestValidator.RoadmapMilestoneNode Current)>(
            resultTree.RootMilestones.Select(e => ((Guid?)null, e)));
        while (rootMilestones.TryPop(out var milestone))
        {
            RoadmapMilestone roadmapMilestone;
            if (milestone.Current.Milestone.Id is not null && existsMilestoneMap.TryGetValue(milestone.Current.Milestone.Id.Value, out var milestoneTree))
            {
                // update
                roadmapMilestone = milestoneTree;
                roadmapMilestone.Name = milestoneTree.Name;
                roadmapMilestone.Order = milestoneTree.Order;
                roadmapMilestone.ParentRoadmapMilestoneId = milestone.Parent;
                roadmapMilestone.Items.Clear();
            }
            else
            {
                // add
                roadmapMilestone = new RoadmapMilestone
                {
                    Name = milestone.Current.Milestone.Name ?? string.Empty,
                    Order = milestone.Current.Milestone.Order,
                    RoadmapId = roadmap.Id,
                    ParentRoadmapMilestoneId = milestone.Parent,
                };
                roadmap.Milestones.Add(roadmapMilestone);
                await db.SaveChangesAsync(cancellationToken);
            }

            roadmapMilestone.Items.AddRange(milestone.Current.QuestionTrees.Select(e => new RoadmapMilestoneItem
            {
                RoadmapMilestoneId = roadmapMilestone.Id,
                QuestionTreeId = e.QuestionTreeId!.Value,
                Order = e.Order,
            }));

            foreach (var roadmapMilestoneNode in milestone.Current.Children)
            {
                rootMilestones.Push((roadmapMilestone.Id, roadmapMilestoneNode));
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

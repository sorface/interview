namespace Interview.Domain.Roadmaps.UpsertRoadmap;

public class UpsertRoadmapRequestValidator
{
    public RoadmapValidationResult Validate(UpsertRoadmapRequest request)
    {
        var result = new RoadmapValidationResult();

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 2)
        {
            result.Errors.Add(Errors.NameTooShort);
        }

        if (ReferenceEquals(request.Items, null) || request.Items.Count == 0)
        {
            result.Errors.Add(Errors.ItemsRequired);
            return result;
        }

        var items = request.Items;

        if (items[0].Type != EVRoadmapItemType.Milestone)
        {
            result.Errors.Add(Errors.FirstMustBeMilestone);
        }

        var previousType = default(EVRoadmapItemType?);
        var lastWasVerticalSplit = false;
        var hasRootMilestone = false;

        RoadmapMilestoneNode? currentMilestoneNode = null;
        var indexMap = new Dictionary<UpsertRoadmapItemRequest, int>(items.Count);
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            indexMap[item] = i;
            var nextItem = i + 1 < items.Count ? items[i + 1] : null;

            ValidateItem(item, result);

            switch (item.Type)
            {
                case EVRoadmapItemType.Milestone:
                    {
                        var hasNextQuestionTree = request.Items.Count > i + 1 && request.Items[i + 1].Type == EVRoadmapItemType.QuestionTree;
                        if (!hasNextQuestionTree)
                        {
                            result.Errors.Add(Errors.MilestoneMustHaveFollowingQuestionTree(i));
                        }

                        if (!hasRootMilestone)
                        {
                            if (item.Order < 0)
                            {
                                result.Errors.Add(Errors.RootMilestoneMustHaveUniqueOrder(i));
                            }

                            currentMilestoneNode = new RoadmapMilestoneNode { Milestone = item };
                            result.Tree.RootMilestones.Add(currentMilestoneNode);
                            hasRootMilestone = true;
                        }
                        else
                        {
                            if (previousType == EVRoadmapItemType.VerticalSplit)
                            {
                                if (item.Order < 0)
                                {
                                    result.Errors.Add(Errors.NewRootMilestoneMustHaveUniqueOrder(i));
                                }

                                var newNode = new RoadmapMilestoneNode { Milestone = item };
                                result.Tree.RootMilestones.Add(newNode);
                                currentMilestoneNode = newNode;
                            }
                            else
                            {
                                if (item.Order != -1)
                                {
                                    result.Errors.Add(Errors.InvalidOrderForChildMilestone(i));
                                }

                                var childNode = new RoadmapMilestoneNode { Milestone = item };
                                currentMilestoneNode?.Children.Add(childNode);
                                currentMilestoneNode = childNode;
                            }
                        }

                        lastWasVerticalSplit = false;
                        break;
                    }

                case EVRoadmapItemType.QuestionTree:
                    {
                        if (currentMilestoneNode is null)
                        {
                            result.Errors.Add(Errors.QuestionTreeMissingParent(i));
                        }

                        if (item.Order < 0)
                        {
                            result.Errors.Add(Errors.InvalidOrderForQuestionTree(i));
                        }

                        if (currentMilestoneNode is not null)
                        {
                            var duplicate = currentMilestoneNode.QuestionTrees.Any(q => q.Order == item.Order);

                            if (duplicate)
                            {
                                result.Errors.Add(Errors.DuplicateOrderInQuestionTree(i));
                            }

                            currentMilestoneNode.QuestionTrees.Add(item);
                            lastWasVerticalSplit = false;
                        }
                        else
                        {
                            result.Errors.Add(Errors.QuestionTreeMissingParent(i));
                        }

                        break;
                    }

                case EVRoadmapItemType.VerticalSplit:
                    {
                        if (previousType != EVRoadmapItemType.QuestionTree)
                        {
                            result.Errors.Add(Errors.VerticalSplitWithoutQuestion(i));
                        }

                        if (lastWasVerticalSplit)
                        {
                            result.Errors.Add(Errors.ConsecutiveVerticalSplit(i));
                        }

                        if (nextItem is null || nextItem.Type != EVRoadmapItemType.Milestone)
                        {
                            result.Errors.Add(Errors.VerticalSplitWithoutMilestone(i));
                        }

                        lastWasVerticalSplit = true;
                        currentMilestoneNode = null;
                        break;
                    }
            }

            previousType = item.Type;
        }

        var visited = new HashSet<Guid>();
        var visiting = new HashSet<Guid>();

        foreach (var root in result.Tree.RootMilestones)
        {
            if (HasCycle(root, visiting, visited, result.Errors))
            {
                break;
            }
        }

        var orders = new HashSet<int>();
        foreach (var roadmapMilestoneNode in result.Tree.RootMilestones)
        {
            if (!orders.Add(roadmapMilestoneNode.Milestone.Order))
            {
                var idx = indexMap[roadmapMilestoneNode.Milestone];
                result.Errors.Add(Errors.NewRootMilestoneMustHaveUniqueOrder(idx));
            }
        }

        return result;
    }

    private static void ValidateItem(UpsertRoadmapItemRequest item, RoadmapValidationResult result)
    {
        switch (item.Type)
        {
            case EVRoadmapItemType.VerticalSplit:
                {
                    if (item.Name is not null || item.QuestionTreeId is not null || item.Order != -1)
                    {
                        result.Errors.Add(Errors.VerticalSplitMustNotHaveValues);
                    }

                    break;
                }

            case EVRoadmapItemType.Milestone:
                {
                    if (string.IsNullOrWhiteSpace(item.Name) || item.Name.Length < 2)
                    {
                        result.Errors.Add(Errors.MilestoneMustHaveNameAtLeast2Characters);
                    }

                    if (item.QuestionTreeId is not null)
                    {
                        result.Errors.Add(Errors.MilestoneMustNotHaveQuestionTreeId);
                    }

                    break;
                }

            case EVRoadmapItemType.QuestionTree:
                {
                    if (item.QuestionTreeId is null)
                    {
                        result.Errors.Add(Errors.QuestionTreeMustHaveQuestionTreeId);
                    }

                    if (item.Name is not null)
                    {
                        result.Errors.Add(Errors.QuestionTreeMustNotHaveName);
                    }

                    break;
                }
        }
    }

    private static bool HasCycle(RoadmapMilestoneNode node, HashSet<Guid> visiting, HashSet<Guid> visited, List<string> errors)
    {
        if (node.Milestone.Id is null)
        {
            return false;
        }

        var id = node.Milestone.Id.Value;

        if (visiting.Contains(id))
        {
            errors.Add(Errors.CycleError(id));
            return true;
        }

        if (visited.Contains(id))
        {
            return false;
        }

        visiting.Add(id);

        foreach (var child in node.Children)
        {
            if (HasCycle(child, visiting, visited, errors))
            {
                return true;
            }
        }

        visiting.Remove(id);
        visited.Add(id);

        return false;
    }

    public static class Errors
    {
        public const string NameTooShort = "Roadmap name must be at least 2 characters long.";
        public const string ItemsRequired = "Items must contain at least one element.";
        public const string FirstMustBeMilestone = "The first item must be of type Milestone.";
        public const string MilestoneMustHaveNameAtLeast2Characters = "Milestone must have a name of at least 2 characters.";
        public const string VerticalSplitMustNotHaveValues = "VerticalSplit must have Name = null, QuestionTreeId = null, and Order = -1.";
        public const string MilestoneMustNotHaveQuestionTreeId = "Milestone must not have a QuestionTreeId.";
        public const string QuestionTreeMustHaveQuestionTreeId = "QuestionTree must have a QuestionTreeId.";
        public const string QuestionTreeMustNotHaveName = "QuestionTree must not have a Name.";

        public static string MilestoneMustHaveFollowingQuestionTree(int index)
            => string.Format("Milestone must be followed by a QuestionTree. Error at index {0}.", index);

        public static string ConsecutiveVerticalSplit(int index) =>
            string.Format("Two consecutive VerticalSplit elements are not allowed. Error at index {0}.", index);

        public static string VerticalSplitWithoutQuestion(int index) =>
            string.Format("VerticalSplit must follow a QuestionTree. Error at index {0}.", index);

        public static string VerticalSplitWithoutMilestone(int index) =>
            string.Format("VerticalSplit must be followed by a Milestone. Error at index {0}.", index);

        public static string QuestionTreeMissingParent(int index) =>
            string.Format("QuestionTree must have a parent Milestone. Error at index {0}.", index);

        public static string DuplicateOrderInQuestionTree(int index) =>
            string.Format("Duplicate Order in QuestionTree within the same Milestone. Error at index {0}.", index);

        public static string CycleError(Guid id) =>
            string.Format("Cycle detected in tree at ID: {0}", id);

        public static string InvalidOrderForQuestionTree(int index) =>
            string.Format("Order of QuestionTree must not be -1. Error at index {0}.", index);

        public static string InvalidOrderForChildMilestone(int index) =>
            string.Format("Child Milestone must have Order -1. Error at index {0}.", index);

        public static string NewRootMilestoneMustHaveUniqueOrder(int index) =>
            $"New root Milestone must have a unique Order >= 0. Error at index {index}.";

        public static string RootMilestoneMustHaveUniqueOrder(int index) =>
            $"Root Milestone must have a unique Order >= 0. Error at index {index}.";
    }

    public class RoadmapValidationResult
    {
        public bool IsValid => Errors.Count == 0;

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public List<string> Errors { get; } = new();

        public RoadmapTree Tree { get; set; } = new();
    }

    public class RoadmapTree
    {
        public List<RoadmapMilestoneNode> RootMilestones { get; set; } = new();
    }

    public class RoadmapMilestoneNode
    {
        public UpsertRoadmapItemRequest Milestone { get; set; } = null!;

        public List<UpsertRoadmapItemRequest> QuestionTrees { get; set; } = new();

        public List<RoadmapMilestoneNode> Children { get; set; } = new();
    }
}

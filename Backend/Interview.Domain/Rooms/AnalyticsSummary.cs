namespace Interview.Domain.Rooms;

public class AnalyticsSummary
{
    public required List<AnalyticsSummaryQuestion> Questions { get; init; }
}

#pragma warning disable SA1402
public class AnalyticsSummaryQuestion
#pragma warning restore SA1402
{
    public required Guid Id { get; init; }

    public required string Value { get; init; }

    public required List<AnalyticsSummaryViewer> Viewers { get; init; }

    public required List<AnalyticsSummaryExpert> Experts { get; init; }
}

#pragma warning disable SA1402
public class AnalyticsSummaryViewer
#pragma warning restore SA1402
{
    public required List<Analytics.AnalyticsReactionSummary> ReactionsSummary { get; init; }
}

#pragma warning disable SA1402
public class AnalyticsSummaryExpert
#pragma warning restore SA1402
{
    public required string Nickname { get; init; }

    public required List<Analytics.AnalyticsReactionSummary> ReactionsSummary { get; init; }
}

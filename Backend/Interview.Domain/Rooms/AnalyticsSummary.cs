namespace Interview.Domain.Rooms;

public class AnalyticsSummary
{
    public required List<AnalyticsSummaryQuestion> Questions { get; init; }
}

public class AnalyticsSummaryQuestion
{
    public required Guid Id { get; init; }

    public required string Value { get; init; }

    public required List<AnalyticsSummaryViewer> Viewers { get; init; }

    public required List<AnalyticsSummaryExpert> Experts { get; init; }
}

public class AnalyticsSummaryViewer
{
    public required List<Analytics.AnalyticsReactionSummary> ReactionsSummary { get; init; }
}

public class AnalyticsSummaryExpert
{
    public required string Nickname { get; init; }

    public required List<Analytics.AnalyticsReactionSummary> ReactionsSummary { get; init; }
}

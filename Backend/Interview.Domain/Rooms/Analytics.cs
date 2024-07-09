namespace Interview.Domain.Rooms;

public class Analytics
{
    public List<AnalyticsQuestion>? Questions { get; set; }

    public class AnalyticsQuestion
    {
        public Guid Id { get; set; }

        public string Value { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public List<AnalyticsUser>? Users { get; set; }
    }

    public class AnalyticsReaction
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class AnalyticsReactionSummary
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class AnalyticsUser
    {
        public Guid Id { get; set; }

        public string Nickname { get; set; } = string.Empty;

        public string Avatar { get; set; } = string.Empty;

        public string ParticipantType { get; set; } = string.Empty;
    }
}

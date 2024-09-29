using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Rooms;

public class Analytics
{
    public required List<AnalyticsQuestion>? Questions { get; set; }

    public required bool Completed { get; set; }

    public required double? AverageMark { get; set; }

    public required List<AnalyticsUserAverageMark> UserReview { get; set; }

    public class AnalyticsQuestion
    {
        public required Guid Id { get; set; }

        public required string Value { get; set; } = string.Empty;

        public required string Status { get; set; } = string.Empty;

        public required List<AnalyticsUser>? Users { get; set; }

        public required double? AverageMark { get; set; }
    }

    public sealed class AnalyticsUserQuestionEvaluation
    {
        public required string? Review { get; set; }

        public required int? Mark { get; set; }
    }

    public class AnalyticsReactionSummary
    {
        public required Guid Id { get; set; }

        public required string Type { get; set; } = string.Empty;

        public required int Count { get; set; }
    }

    public class AnalyticsUser
    {
        public required Guid Id { get; set; }

        public required AnalyticsUserQuestionEvaluation? Evaluation { get; set; }
    }

    public class AnalyticsUserAverageMark
    {
        public required Guid UserId { get; set; }

        public required double? AverageMark { get; set; }

        public required EVRoomParticipantType ParticipantType { get; set; }

        public required string Comment { get; set; } = string.Empty;

        public required string Nickname { get; set; } = string.Empty;

        public required string? Avatar { get; set; } = string.Empty;
    }
}

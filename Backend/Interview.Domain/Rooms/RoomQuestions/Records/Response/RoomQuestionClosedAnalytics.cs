using Interview.Domain.Rooms.RoomQuestionEvaluations;

namespace Interview.Domain.Rooms.RoomQuestions.Records.Response;

public class RoomQuestionClosedAnalytics
{
    public required List<ClosedQuestionAnalytics> Questions { get; set; }
    public required double? OverallAverageMark { get; set; }
    public required int TotalClosedQuestions { get; set; }
    public required int TotalEvaluations { get; set; }
    public required DateTime LastClosedQuestionDate { get; set; }

    public class ClosedQuestionAnalytics
    {
        public required Guid QuestionId { get; set; }
        public required string QuestionValue { get; set; }
        public required DateTime ClosedDate { get; set; }
        public required double? AverageMark { get; set; }
        public required int TotalEvaluations { get; set; }
        public required List<EvaluationDistribution> MarkDistribution { get; set; }
        public required List<EvaluatorSummary> TopEvaluators { get; set; }
    }

    public class EvaluationDistribution
    {
        public required int Mark { get; set; }
        public required int Count { get; set; }
        public required double Percentage { get; set; }
    }

    public class EvaluatorSummary
    {
        public required Guid UserId { get; set; }
        public required string Nickname { get; set; }
        public required string? Avatar { get; set; }
        public required EVRoomParticipantType ParticipantType { get; set; }
        public required double AverageMark { get; set; }
        public required int TotalEvaluations { get; set; }
    }
} 
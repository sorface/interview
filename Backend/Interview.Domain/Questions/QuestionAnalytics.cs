using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Questions;

public class QuestionAnalytics
{
    public required double? AverageMark { get; set; }
    
    public required bool Completed { get; set; }
    
    public required List<AnalyticsUserEvaluation> Evaluations { get; set; }

    public class AnalyticsUserEvaluation
    {
        public required Guid UserId { get; set; }
        
        public required string Nickname { get; set; } = string.Empty;
        
        public required string? Avatar { get; set; }
        
        public required EVRoomParticipantType ParticipantType { get; set; }
        
        public required int? Mark { get; set; }
        
        public required string? Review { get; set; }
    }
} 
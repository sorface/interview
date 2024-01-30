using Interview.Domain.Users;

namespace Interview.Domain.RoomParticipants
{
    public class RoomMembershipChecker : IService, IRoomMembershipChecker
    {
        private readonly ICurrentUserAccessor _currentUserAccessor;
        private readonly IRoomParticipantRepository _participantRepository;

        public RoomMembershipChecker(ICurrentUserAccessor currentUserAccessor, IRoomParticipantRepository participantRepository)
        {
            _currentUserAccessor = currentUserAccessor;
            _participantRepository = participantRepository;
        }

        public async Task EnsureCurrentUserMemberOfRoom(Guid roomId, CancellationToken cancellationToken)
        {
            var userId = _currentUserAccessor.GetUserIdOrThrow();
            var hasParticipant = await _participantRepository.IsExistsByRoomIdAndUserIdAsync(roomId, userId, cancellationToken);
            if (!hasParticipant)
            {
                throw new AccessDeniedException("You are not a member of the room.");
            }
        }
    }
}

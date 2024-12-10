using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Events.Service.FindPage;

public sealed class AppEventItemParticipantTypeMapper() : Mapper<AppEvent, AppEventItemParticipantType>(e => new AppEventItemParticipantType
{
    Id = e.Id,
    Type = e.Type,
    Roles = Enumerable.Select(e.Roles!, e => e.Name.EnumValue)
        .ToList(),
    ParticipantTypes = e.ParticipantTypes ?? new List<SERoomParticipantType>(),
    Stateful = e.Stateful,
});

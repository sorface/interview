using FluentAssertions;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;

namespace Interview.Test.Units.Events.Serializers
{
    public class JsonRoomEventSerializerTest
    {
        public static IEnumerable<object?[]> SerializeAsStringData
        {
            get
            {
                yield return new object?[] { "{}", null };

                yield return new object?[]
                {
                    """{"Id":"55ad40ec-c89d-11ed-ac80-463da0479b2d","RoomId":"81ad40ec-c89d-11ed-ac80-463da0479b2d","Type":"Test","Stateful":false,"CreatedAt":"2005-02-01T00:00:00"}""",
                    new RoomEvent(Guid.Parse("55ad40ec-c89d-11ed-ac80-463da0479b2d"), Guid.Parse("81ad40ec-c89d-11ed-ac80-463da0479b2d"), "Test", null, false, new DateTime(2005, 02, 01))
                };

                yield return new object?[]
                {
                    """{"Id":"55ad40ec-c89d-11ed-ac80-463da0479b2d","RoomId":"81ad40ec-c89d-11ed-ac80-463da0479b2d","Type":"Test","Stateful":false,"CreatedAt":"2005-02-01T00:00:00","Value":"Hello world"}""",
                    new RoomEvent(Guid.Parse("55ad40ec-c89d-11ed-ac80-463da0479b2d"), Guid.Parse("81ad40ec-c89d-11ed-ac80-463da0479b2d"), "Test", "Hello world", false, new DateTime(2005, 02, 01))
                };

                yield return new object?[]
                {
                    """{"Id":"55ad40ec-c89d-11ed-ac80-463da0479b2d","RoomId":"81ad40ec-c89d-11ed-ac80-463da0479b2d","Type":"Test","Stateful":false,"CreatedAt":"2005-02-01T00:00:00"}""",
                    new RoomEvent<RoomEventUserPayload>(Guid.Parse("55ad40ec-c89d-11ed-ac80-463da0479b2d"), Guid.Parse("81ad40ec-c89d-11ed-ac80-463da0479b2d"), "Test", null, false, new DateTime(2005, 02, 01))
                };

                yield return new object?[]
                {
                    """{"Id":"55ad40ec-c89d-11ed-ac80-463da0479b2d","RoomId":"81ad40ec-c89d-11ed-ac80-463da0479b2d","Type":"Test","Stateful":false,"CreatedAt":"2005-02-01T00:00:00","Value":{"UserId":"81ad40ec-c89d-11ed-ac80-9acce9101761"}}""",
                    new RoomEvent<RoomEventUserPayload>(Guid.Parse("55ad40ec-c89d-11ed-ac80-463da0479b2d"), Guid.Parse("81ad40ec-c89d-11ed-ac80-463da0479b2d"), "Test", new RoomEventUserPayload(Guid.Parse("81ad40ec-c89d-11ed-ac80-9acce9101761")), false, new DateTime(2005, 02, 01))
                };
            }
        }

        [Theory]
        [MemberData(nameof(SerializeAsStringData))]
        public void SerializeAsString(string expectedResult, IRoomEvent? @event)
        {
            var serializer = new JsonRoomEventSerializer();

            var result = serializer.SerializeAsString(@event);

            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}

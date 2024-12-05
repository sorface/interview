using System.Diagnostics.CodeAnalysis;
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
                    """{"Id":"55ad40ec-c89d-11ed-ac80-463da0479b2d","RoomId":"81ad40ec-c89d-11ed-ac80-463da0479b2d","Type":"Test","Stateful":false,"CreatedAt":"2005-02-01T00:00:00","CreatedById":"CD5F5144-D489-4D1E-BC6D-15FEC736BB7F"}""",
                    new RoomEvent
                    {
                        Id = Guid.Parse("55ad40ec-c89d-11ed-ac80-463da0479b2d"),
                        RoomId = Guid.Parse("81ad40ec-c89d-11ed-ac80-463da0479b2d"),
                        Type = "Test",
                        Value = null,
                        CreatedAt = new DateTime(2005, 02, 01),
                        CreatedById = Guid.Parse("CD5F5144-D489-4D1E-BC6D-15FEC736BB7F"),
                    }
                };

                yield return new object?[]
                {
                    """{"Id":"55ad40ec-c89d-11ed-ac80-463da0479b2d","RoomId":"81ad40ec-c89d-11ed-ac80-463da0479b2d","Type":"Test","Stateful":false,"CreatedAt":"2005-02-01T00:00:00","CreatedById":"9E2BB88F-A45D-4BCA-900B-B27CC29FA06D","Value":"Hello world"}""",
                    new RoomEvent
                    {
                        Id = Guid.Parse("55ad40ec-c89d-11ed-ac80-463da0479b2d"),
                        RoomId = Guid.Parse("81ad40ec-c89d-11ed-ac80-463da0479b2d"),
                        Type = "Test",
                        Value = "Hello world",
                        CreatedAt = new DateTime(2005, 02, 01),
                        CreatedById = Guid.Parse("9E2BB88F-A45D-4BCA-900B-B27CC29FA06D"),
                    }
                };

                yield return new object?[]
                {
                    """{"Id":"55ad40ec-c89d-11ed-ac80-463da0479b2d","RoomId":"81ad40ec-c89d-11ed-ac80-463da0479b2d","Type":"Test","Stateful":false,"CreatedAt":"2005-02-01T00:00:00","CreatedById":"233B89BA-3244-409F-9FC3-581179562391"}""",
                    new TestRoomEvent(Guid.Parse("55ad40ec-c89d-11ed-ac80-463da0479b2d"), Guid.Parse("81ad40ec-c89d-11ed-ac80-463da0479b2d"), "Test", null, false, new DateTime(2005, 02, 01), Guid.Parse("233B89BA-3244-409F-9FC3-581179562391"))
                };

                yield return new object?[]
                {
                    """{"Id":"55ad40ec-c89d-11ed-ac80-463da0479b2d","RoomId":"81ad40ec-c89d-11ed-ac80-463da0479b2d","Type":"Test","Stateful":false,"CreatedAt":"2005-02-01T00:00:00","CreatedById":"663DCF8B-241B-4671-AA58-2AB05D327388","Value":{"UserId":"81ad40ec-c89d-11ed-ac80-9acce9101761"}}""",
                    new TestRoomEvent(Guid.Parse("55ad40ec-c89d-11ed-ac80-463da0479b2d"), Guid.Parse("81ad40ec-c89d-11ed-ac80-463da0479b2d"), "Test", new RoomEventUserPayload(Guid.Parse("81ad40ec-c89d-11ed-ac80-9acce9101761")), false, new DateTime(2005, 02, 01), Guid.Parse("663DCF8B-241B-4671-AA58-2AB05D327388"))
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

        public sealed class TestRoomEvent : RoomEvent<RoomEventUserPayload>
        {
            [SetsRequiredMembers]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public TestRoomEvent(Guid id, Guid roomId, string type, RoomEventUserPayload? value, bool stateful, DateTime createdAt, Guid createdById)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            {
                Id = id;
                RoomId = roomId;
                Type = type;
                Value = value;
                Stateful = stateful;
                CreatedAt = createdAt;
                CreatedById = createdById;
            }
        }
    }
}

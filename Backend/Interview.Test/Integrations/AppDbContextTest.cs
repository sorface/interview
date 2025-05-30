using FluentAssertions;
using Interview.Domain.Questions;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomQuestions;

namespace Interview.Test.Integrations;

public class AppDbContextTest
{
    [Fact(DisplayName = "AppDbContext should update the update date and the entity creation date when saving")]
    public async Task DbContext_Should_Update_Create_And_Update_Dates()
    {
        var clock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(clock);

        var room = new Room("Test room", SERoomAccessType.Public, SERoomType.Standard)
        {
            Questions =
            [
                new()
                {
                    Question = new Question("Value 1"),
                    State = RoomQuestionState.Active,
                    RoomId = default,
                    QuestionId = default,
                    Room = null,
                    Order = 0,
                }
            ]
        };
        await appDbContext.AddAsync(room);
        await appDbContext.SaveChangesAsync();

        room.Id.Should().NotBe(Guid.Empty);
        room.CreateDate.Should().NotBe(null);
        room.UpdateDate.Should().NotBe(null);

        room.Questions[0].Id.Should().NotBe(Guid.Empty);
        room.Questions[0].CreateDate.Should().NotBe(null);
        room.Questions[0].UpdateDate.Should().NotBe(null);

        room.Questions[0].Question!.Id.Should().NotBe(Guid.Empty);
        room.Questions[0].Question!.CreateDate.Should().NotBe(null);
        room.Questions[0].Question!.UpdateDate.Should().NotBe(null);
    }
}

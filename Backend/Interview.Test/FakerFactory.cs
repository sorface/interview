using Bogus;
using Interview.Domain.Questions;
using Interview.Domain.Rooms;

namespace Interview.Test
{
    public class FakerFactory
    {
        public Faker<Room> Room()
        {
            return new Faker<Room>()
                .CustomInstantiator(e => new Room(string.Empty, SERoomAccessType.Public, SERoomType.Standard))
                .RuleFor(e => e.Name, f => f.Random.Word())
                .RuleFor(e => e.Id, f => Guid.NewGuid());
        }

        public Faker<Question> Question()
        {
            return new Faker<Question>()
                .CustomInstantiator(e => new Question(string.Empty))
                .RuleFor(e => e.Value, e => e.Random.Word());
        }
    }
}

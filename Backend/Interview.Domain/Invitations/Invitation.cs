using Interview.Domain.Repository;

namespace Interview.Domain.Invitations;

public class Invitation : Entity
{
    public Invitation()
    {
    }

    public string Hash { get; set; }
}

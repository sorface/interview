using Interview.Domain.Repository;

namespace Interview.Domain.Invitations;

public class Invitation : Entity
{
    private Invitation()
    {
    }

    public string Hash { get; internal set; }
}

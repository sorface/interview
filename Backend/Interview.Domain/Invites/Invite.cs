using Interview.Domain.Repository;

namespace Interview.Domain.Invites;

public class Invite(int usesMax) : Entity
{
    private Invite()
        : this(5)
    {
    }

    public int UsesCurrent { get; set; } = 0;

    public int UsesMax { get; set; } = usesMax;
}

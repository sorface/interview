using Interview.Domain.Repository;

namespace Interview.Domain.Invites;

public class Invite : Entity
{
    public Invite(int usesMax)
    {
        UsesMax = usesMax;
        UsesCurrent = 0;
    }

    private Invite()
        : this(5)
    {
    }

    public int UsesCurrent { get; set; }

    public int UsesMax { get; set; }
}

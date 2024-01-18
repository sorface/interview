namespace Interview.Domain.Invitations.Records.Response;

public class InvitationItem
{
    public Guid Id { get; set; }

    public string? Hash { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public Guid? OwnerId { get; set; }
}

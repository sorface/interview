using Interview.Domain.Invitations;
using Interview.Domain.Invitations.Records.Response;
using Interview.Domain.Repository;

namespace Interview.Domain.Invites.Services;

public class InviteService : IInviteService
{
    private static readonly Mapper<Invite, InvitationItem> _MAPPERINVITATIONITEM = new(
        invitation => new InvitationItem
        {
            Id = invitation.Id,
            UsesCurrent = invitation.UsesCurrent,
            UsesMax = invitation.UsesMax,

            CreateDate = invitation.CreateDate,
            UpdateDate = invitation.UpdateDate,
            OwnerId = invitation.CreatedById,
        });

    private readonly IInviteRepository _inviteRepository;

    public InviteService(IInviteRepository inviteRepository)
    {
        _inviteRepository = inviteRepository;
    }

    public async Task<InvitationItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _inviteRepository.FindByIdDetailedAsync(id, _MAPPERINVITATIONITEM, cancellationToken);
    }
}

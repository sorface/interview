using Interview.Domain.Invites.Records.Response;
using Interview.Domain.Repository;

namespace Interview.Domain.Invites.Services;

public class InviteService(IInviteRepository inviteRepository) : IInviteService
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

    public async Task<InvitationItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await inviteRepository.FindByIdDetailedAsync(id, _MAPPERINVITATIONITEM, cancellationToken);
    }
}

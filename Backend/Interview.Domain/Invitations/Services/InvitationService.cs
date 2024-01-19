using Interview.Domain.Invitations.Records.Response;
using Interview.Domain.Invitations.Utils;
using Interview.Domain.Repository;
using NSpecifications;

namespace Interview.Domain.Invitations.Services;

public class InvitationService : IInvitationService
{
    private static readonly Mapper<Invitation, InvitationItem> MAPPERINVITATIONITEM = new(
        invitation => new InvitationItem
        {
            Id = invitation.Id,
            Hash = invitation.Hash,
            CreateDate = invitation.CreateDate,
            UpdateDate = invitation.UpdateDate,
            OwnerId = invitation.CreatedById,
        });

    private readonly IInvitationRepository _invitationRepository;

    public InvitationService(IInvitationRepository invitationRepository)
    {
        _invitationRepository = invitationRepository;
    }

    public async Task<InvitationItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _invitationRepository.FindByIdDetailedAsync(id, MAPPERINVITATIONITEM, cancellationToken);
    }

    public async Task<InvitationItem?> FindByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        var spec = new Spec<Invitation>(invitation => invitation.Hash == hash);

        return await _invitationRepository.FindFirstOrDefaultAsync(spec, MAPPERINVITATIONITEM, cancellationToken);
    }

    public async Task<InvitationItem?> Create(CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.CreateAsync(TokenUtils.GenerateToken(10), cancellationToken);

        return MAPPERINVITATIONITEM.Map(invitation!);
    }

    public async Task<InvitationItem?> RegenerationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.RegenerationAsync(id, TokenUtils.GenerateToken(10), cancellationToken);

        return MAPPERINVITATIONITEM.Map(invitation!);
    }
}

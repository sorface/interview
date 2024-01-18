using Interview.Domain.Invitations.Records.Response;
using Interview.Domain.Repository;
using NSpecifications;

namespace Interview.Domain.Invitations.Services;

public class InvitationService : IInvitationService
{
    private readonly IInvitationRepository _invitationRepository;

    private static readonly Mapper<Invitation, InvitationItem> MapperInvitationItem = new(
        invitation => new InvitationItem
        {
            Id = invitation.Id,
            Hash = invitation.Hash,
            CreateDate = invitation.CreateDate,
            UpdateDate = invitation.UpdateDate,
            OwnerId = invitation.CreatedById,
        });

    public InvitationService(IInvitationRepository invitationRepository)
    {
        _invitationRepository = invitationRepository;
    }

    public async Task<InvitationItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _invitationRepository.FindByIdDetailedAsync(id, MapperInvitationItem, cancellationToken);
    }

    public async Task<InvitationItem?> FindByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        var spec = new Spec<Invitation>(invitation => invitation.Hash == hash);

        return await _invitationRepository.FindFirstOrDefaultAsync(spec, MapperInvitationItem, cancellationToken);
    }
}

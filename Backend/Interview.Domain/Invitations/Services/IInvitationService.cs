using Interview.Domain.Invitations.Records.Response;

namespace Interview.Domain.Invitations;

public interface IInvitationService : IService
{
    public Task<InvitationItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<InvitationItem?> FindByHashAsync(string hash, CancellationToken cancellationToken = default);

    public Task<InvitationItem?> Create(CancellationToken cancellationToken = default);
}

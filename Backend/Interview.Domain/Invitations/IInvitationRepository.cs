using Interview.Domain.Invitations.Records.Response;
using Interview.Domain.Repository;

namespace Interview.Domain.Invitations;

public interface IInvitationRepository : IRepository<Invitation>
{
    public Task<Invitation?> CreateAsync(string hash, CancellationToken cancellationToken = default);

    public Task<InvitationItem?> RegenerationAsync(
        Guid id,
        string hash,
        IMapper<Invitation, InvitationItem> mapper,
        CancellationToken cancellationToken = default);
}

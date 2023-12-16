using System.Security.Claims;
using AspNet.Security.OAuth.Twitch;
using CSharpFunctionalExtensions;
using Interview.Domain;

namespace Interview.Backend.Auth
{
    public class UserClaimService
    {
        /// <summary>
        /// Parse claims to user object of type <see cref="UserClaim"/>.
        /// </summary>
        /// <param name="claims">List with claim is a statement about an entity by an Issuer.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns><see cref="Task{TResult}"/> with the result of user data or an error message.</returns>
        public Task<UserClaim?> ParseClaimsAsync(
            IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default)
        {
            var claimList = claims.ToList();

            var externalId = claimList.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier);
            var nickname = claimList.FirstOrDefault(e => e.Type == ClaimTypes.Name);

            var avatar = claimList
                .FirstOrDefault(e => e.Type == TwitchAuthenticationConstants.Claims.ProfileImageUrl);

            if (externalId is null || nickname is null || avatar is null)
            {
                throw new NotFoundException($"Not found users fields");
            }

            var id = claimList.FirstOrDefault(e => e.Type == UserClaimConstants.UserId);

            if (id == null)
            {
                throw new NotFoundException(ExceptionMessage.UserNotFound());
            }

            if (!Guid.TryParse(id.Value, out var typedId))
            {
                throw new NotFoundException("User id is invalid");
            }

            return Task.FromResult<UserClaim?>(new UserClaim
            {
                Identity = typedId,
                Nickname = nickname.Value,
                Avatar = avatar.Value,
                Roles = claimList.Where(claim => claim.Type == ClaimTypes.Role)
                        .Select(claim => claim.Value)
                        .ToList(),
                TwitchIdentity = externalId.Value,
            });
        }
    }
}

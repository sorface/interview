using System.Security.Claims;

namespace Interview.Backend.Auth.Sorface;

public static class SorfaceClaimTypes
{
    private const string ClaimType2005Namespace = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";

    public static class Claims
    {
        public static class Profile
        {
            public const string Id = ClaimType2005Namespace + "/profile-id";

            public const string Username = ClaimType2005Namespace + "/profile-username";

            public const string Email = ClaimType2005Namespace + "/profile-emailaddress";

            public const string Avatar = ClaimType2005Namespace + "/profile-avatar";

            public const string FirstName = ClaimType2005Namespace + "/profile-firstname";

            public const string LastName = ClaimType2005Namespace + "/profile-lastname";

            public const string MiddleName = ClaimType2005Namespace + "/profile-middleName";

            public const string Authorities = ClaimType2005Namespace + "/profile-authorities";
        }
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Interview.Backend.Auth.Dev
{
    public class DevelopmentAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public Func<HttpContext, ClaimsPrincipal, Task> OnTokenValidated { get; set; } = (context, principal) => Task.CompletedTask;
    }
}

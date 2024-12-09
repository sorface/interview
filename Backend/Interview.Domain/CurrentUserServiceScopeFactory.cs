using Interview.Domain.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Interview.Domain
{
    public sealed class CurrentUserServiceScopeFactory(IServiceProvider rootServiceProvider, IServiceScopeFactory root) : IServiceScopeFactory
    {
        public IServiceScope CreateScope()
        {
            var scope = root.CreateScope();

            // Unpleasant hack, because when creating a new Scope a new ICurrentUserAccessor will be created, there will be no information about the original user there,
            // so before creating the scope, you need to set the user if it has already been created.
            var currentUserAccessor = rootServiceProvider.GetService<ICurrentUserAccessor>();
            if (currentUserAccessor?.UserDetailed is not null)
            {
                var editableCurrentUserAccessor = scope.ServiceProvider.GetRequiredService<IEditableCurrentUserAccessor>();
                editableCurrentUserAccessor.SetUser(currentUserAccessor.UserDetailed);
            }

            return scope;
        }
    }
}

using Interview.Domain.Users;
using Interview.Infrastructure.Chat;
using Microsoft.EntityFrameworkCore;

namespace Interview.DependencyInjection;

public sealed class DependencyInjectionAppServiceOption
{
    public required Action<DbContextOptionsBuilder> DbConfigurator { get; init; }

    public required Action<EventStorageOptionBuilder> EventStorageConfigurator { get; init; }

    public required TwitchTokenProviderOption TwitchTokenProviderOption { get; init; }

    public required AdminUsers AdminUsers { get; init; }
}

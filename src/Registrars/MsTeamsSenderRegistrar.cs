using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.MsTeams.Sender.Abstract;
using Soenneker.Utils.HttpClientCache.Registrar;

namespace Soenneker.MsTeams.Sender.Registrars;

/// <summary>
/// A utility that sends Adaptive Card messages to Microsoft Teams via configured webhooks, handling channel routing, logging, and error responses including rate-limiting.
/// </summary>
public static class MsTeamsSenderRegistrar
{
    /// <summary>
    /// Adds <see cref="IMsTeamsSender"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddMsTeamsSenderAsSingleton(this IServiceCollection services)
    {
        services.AddHttpClientCacheAsSingleton().TryAddSingleton<IMsTeamsSender, MsTeamsSender>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IMsTeamsSender"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddMsTeamsSenderAsScoped(this IServiceCollection services)
    {
        services.AddHttpClientCacheAsSingleton().TryAddScoped<IMsTeamsSender, MsTeamsSender>();

        return services;
    }
}
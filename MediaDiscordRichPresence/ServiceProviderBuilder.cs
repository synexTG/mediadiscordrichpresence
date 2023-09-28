using Microsoft.Extensions.DependencyInjection;
using Plex.Api.Factories;
using Plex.Library.Factories;
using Plex.ServerApi.Api;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.Clients;
using Plex.ServerApi;

internal class ServiceProviderBuilder
{
    internal static ServiceProvider build()
    {
        var services = new ServiceCollection();
        var apiOptions = new ClientOptions
        {
            Product = "MediaDiscordRichPresence",
            DeviceName = "DESKTOP-BM",
            ClientId = "1337",
            Platform = "Web",
            Version = "v1"
        };
        services.AddLogging();
        services.AddSingleton(apiOptions);
        services.AddTransient<IPlexServerClient, PlexServerClient>();
        services.AddTransient<IPlexAccountClient, PlexAccountClient>();
        services.AddTransient<IPlexLibraryClient, PlexLibraryClient>();
        services.AddTransient<IApiService, ApiService>();
        services.AddTransient<IPlexFactory, PlexFactory>();
        services.AddTransient<IPlexRequestsHttpClient, PlexRequestsHttpClient>();

        return services.BuildServiceProvider();
    }
}
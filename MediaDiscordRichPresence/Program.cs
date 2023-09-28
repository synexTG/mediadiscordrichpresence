using DiscordRPC.Logging;
using DiscordRPC;
using MediaDiscordRichPresence;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Plex.Api.Factories;
using Plex.Library.Factories;
using Plex.ServerApi.Api;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.Clients;
using Plex.ServerApi;

Console.WriteLine("Starting rich presence application");
Console.WriteLine("Getting config...");
Config config = new Config();
try
{
    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json"));
} catch(Exception e)
{
    Console.WriteLine("Config could not be loaded: " + e.ToString());
    Environment.Exit(1);
}
Console.WriteLine("Config loaded succesfully");
Console.WriteLine("Initialize plex api wrapper");

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

var sp = services.BuildServiceProvider();

Console.WriteLine("Initialize providers");
PlexProvider plex = new PlexProvider(config.Plex.PlexUrl, config.Plex.PlexProfileName, config.Plex.PlexAuthToken, sp);
EmbyProvider emby = new EmbyProvider(config.Emby.EmbyUrl, config.Emby.EmbyApiKey, config.Emby.EmbyProfileName, config.Emby.EmbyEpgHourOffset);
Console.WriteLine("Initialize discord rich presence client");
async Task InitializeAsync()
{
    var client = new DiscordRpcClient("1156551061608353872");

    client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

    client.Initialize();

    var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(3));
    while (await periodicTimer.WaitForNextTickAsync())
    {
        if (plex.IsCurrentlyPlaying())
        {
            plex.SetRichPresence(client);
        }
        else if (emby.IsCurrentlyPlaying())
        {
            emby.SetRichPresence(client);
        } else
        {
            client.ClearPresence();
        }
    }
}
await InitializeAsync();
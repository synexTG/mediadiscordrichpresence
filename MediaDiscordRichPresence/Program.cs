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
    if (config is null) throw new Exception("Config is null: Check if Config.json exists and is filled out correctly!");
} catch(Exception e)
{
    Console.WriteLine("Config could not be loaded: " + e.ToString());
    Environment.Exit(1);
}
Console.WriteLine("Config loaded succesfully");

Console.WriteLine("Initialize plex api wrapper");
var sp = ServiceProviderBuilder.build();

Console.WriteLine("Initialize providers");
PlexProvider plex = new(config, sp);
EmbyProvider emby = new(config);

Console.WriteLine("Initialize discord rich presence client");
async Task InitializeAsync()
{
    var client = new DiscordRpcClient(config.Discord.ApplicationId)
    {
        Logger = new ConsoleLogger() { Level = LogLevel.Warning }
    };

    client.Initialize();

    var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(config.RichPresence.RefreshIntervalInSeconds));
    while (await periodicTimer.WaitForNextTickAsync())
    {
        try { 
            switch (config.RichPresence.PriorityMode)
            {
                case 0:
                    if (plex.IsCurrentlyPlaying() && config.Plex.Enabled) {
                        plex.SetRichPresence(client);
                        break;
                    }
                    if (emby.IsCurrentlyPlaying() && config.Emby.Enabled)
                    {
                        emby.SetRichPresence(client);
                        break;
                    }
                    break;
                case 1:
                    if (emby.IsCurrentlyPlaying() && config.Emby.Enabled)
                    {
                        emby.SetRichPresence(client);
                        break;
                    }
                    if (plex.IsCurrentlyPlaying() && config.Plex.Enabled)
                    {
                        plex.SetRichPresence(client);
                        break;
                    }
                    break;
            }
        } catch (Exception ex)
        {
            client.ClearPresence();
            Console.WriteLine("An error occurred on setting the rich presence, will retry in the next interval");
            Console.WriteLine(ex.ToString());
        }
    }
}
await InitializeAsync();
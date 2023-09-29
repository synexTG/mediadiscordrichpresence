using DiscordRPC.Logging;
using DiscordRPC;
using MediaDiscordRichPresence;
using Newtonsoft.Json;

Console.WriteLine("Starting rich presence application");
Console.WriteLine("Getting config...");
Config config = new Config();
try
{
    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json"));
    if (config is null) throw new Exception("Config is null: Check if Config.json exists and is filled out correctly!");
}
catch (Exception e)
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
        try
        {
            if (config.RichPresence.RefreshConfigOnEveryCheck)
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json"));
                plex.Config = config;
                emby.Config = config;
            }

            switch (config.RichPresence.PriorityMode)
            {
                case 0:
                    if (config.Plex.Enabled && plex.IsCurrentlyPlaying())
                    {
                        plex.SetRichPresence(client);
                        break;
                    }
                    if (config.Emby.Enabled && emby.IsCurrentlyPlaying())
                    {
                        emby.SetRichPresence(client);
                        break;
                    }
                    client.ClearPresence();
                    break;
                case 1:
                    if (config.Emby.Enabled && emby.IsCurrentlyPlaying())
                    {
                        emby.SetRichPresence(client);
                        break;
                    }
                    if (config.Plex.Enabled && plex.IsCurrentlyPlaying())
                    {
                        plex.SetRichPresence(client);
                        break;
                    }
                    client.ClearPresence();
                    break;
            }
        }
        catch (Exception ex)
        {
            client.ClearPresence();
            Console.WriteLine("An error occurred on setting the rich presence, will retry in the next interval");
            Console.WriteLine(ex.ToString());
        }
    }
}
await InitializeAsync();
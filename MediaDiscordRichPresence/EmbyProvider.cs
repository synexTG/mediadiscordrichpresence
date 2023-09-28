using DiscordRPC;
using MediaDiscordRichPresence.EmbyModels;
using MediaDiscordRichPresence.Models;
using RestSharp;

namespace MediaDiscordRichPresence;
public class EmbyProvider : IProvider
{
    private ActivityObject CurrentActivityObject { get; set; } = new ActivityObject();
    public Config Config { get; set; }
    private long SavedDurationLeft { get; set; } = 0;
    public EmbyProvider(Config pConfig) 
    {
        Config = pConfig;
    }

    public bool IsCurrentlyPlaying()
    {
        foreach(EmbySessions.Class1 c in GetSessions())
        {
            if(c.PlayState.CanSeek && c.UserName == Config.Emby.ProfileName)
            {
                return true;
            }
        }
        return false;
    }

    public void SetRichPresence(DiscordRpcClient client)
    {
        CurrentActivityObject = GetActivityObject();

        //Setting Tooltip for the large Image
        string largeImageText = Config.RichPresence.WatchingUnknown;
        switch(GetActivityType()) {
            case ActivityType.LiveTV:
                largeImageText = Config.RichPresence.WatchingTV;
                break;
            case ActivityType.Movie:
                largeImageText = Config.RichPresence.WatchingMovie;
                break;
            case ActivityType.Show:
                largeImageText = Config.RichPresence.WatchingShow;
                break;
            default: 
                break;
        }

        //Setting TimeStamp
        Timestamps timestamps = Timestamps.Now;
        if(Config.RichPresence.ShowTimeLeftIfPossible)
        {
            timestamps = CurrentActivityObject.DurationLeft != 0 ? Timestamps.FromTimeSpan(TimeSpan.FromMilliseconds(CurrentActivityObject.DurationLeft)) : Timestamps.Now;
        }

        RichPresence currentRichPresence = client.CurrentPresence;
        RichPresence updatedRichPresence = new RichPresence()
        {
            Timestamps = timestamps,
            Details = "Emby: " + CurrentActivityObject.Title,
            State = CurrentActivityObject.Description,
            Assets = new Assets()
            {
                LargeImageKey = CurrentActivityObject.Logo,
                LargeImageText = largeImageText,
                SmallImageKey = CurrentActivityObject.IsPaused ? Config.ImageTemplateLinks.Paused : Config.ImageTemplateLinks.Playing,
                SmallImageText = CurrentActivityObject.IsPaused ? Config.RichPresence.Paused : Config.RichPresence.Playing
            }
        };

        if (currentRichPresence is null)
        {
            if (CurrentActivityObject.IsPaused) updatedRichPresence.Timestamps = null;
            client.SetPresence(updatedRichPresence);
        }

        if (currentRichPresence is not null && (currentRichPresence.Details != updatedRichPresence.Details ||
            currentRichPresence.State != updatedRichPresence.State ||
            currentRichPresence.Assets.LargeImageText != updatedRichPresence.Assets.LargeImageText ||
            currentRichPresence.Assets.SmallImageText != updatedRichPresence.Assets.SmallImageText ||
            (Config.RichPresence.ShowTimeLeftIfPossible && ((CurrentActivityObject.DurationLeft - (SavedDurationLeft - 3000)) > 10000 ||
            (CurrentActivityObject.DurationLeft - (SavedDurationLeft - 3000)) < -10000))))
        {
            if (CurrentActivityObject.IsPaused && Config.RichPresence.ShowTimeLeftIfPossible) updatedRichPresence.Timestamps = null;
            client.SetPresence(updatedRichPresence);
        }
        SavedDurationLeft = CurrentActivityObject.DurationLeft;
    }

    private ActivityType GetActivityType()
    {
        foreach (EmbySessions.Class1 c in GetSessions())
        {
            if (c.PlayState.CanSeek && c.UserName == Config.Emby.ProfileName)
            {
                if(c.NowPlayingItem.Type == "TvChannel") return ActivityType.LiveTV;
            }
        }
        return ActivityType.None;
    }

    private ActivityObject GetActivityObject()
    {
        foreach (EmbySessions.Class1 c in GetSessions())
        {
            if (c.PlayState.CanSeek && c.UserName == Config.Emby.ProfileName)
            {
                return new ActivityObject()
                {
                    Description = "Program: " + c.NowPlayingItem.CurrentProgram.Name,
                    Logo = Config.Emby.Url + "/emby/Items/" + c.NowPlayingItem.CurrentProgram.ParentId + "/Images/Primary?tag=" + c.NowPlayingItem.CurrentProgram.ChannelPrimaryImageTag + "&quality=9",
                    Title = c.NowPlayingItem.CurrentProgram.ChannelName,
                    IsPaused = c.PlayState.IsPaused,
                    DurationLeft = (long) (c.NowPlayingItem.CurrentProgram.EndDate.AddHours(Config.Emby.EpgHourOffset) - DateTime.Now).TotalMilliseconds
                };
            }
        }
        throw new Exception("Something went wrong on getting the current activity of emby");
    }

    private List<EmbySessions.Class1> GetSessions()
    {
        var options = new RestClientOptions(Config.Emby.Url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("/emby/Sessions?api_key=" + Config.Emby.ApiKey, Method.Get);
        RestResponse response = client.Execute(request);
        if (response.IsSuccessful)
        {
            List<EmbySessions.Class1> sessions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EmbySessions.Class1>>(response.Content);
            if (sessions is null) throw new Exception("Sessions could not be retrieved!");
            return sessions;
        }
        throw new Exception("Sessions could not be retrieved!");
    }
}

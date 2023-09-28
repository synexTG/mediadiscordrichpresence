using DiscordRPC;
using MediaDiscordRichPresence.EmbyModels;
using RestSharp;

namespace MediaDiscordRichPresence;
public class EmbyProvider : IProvider
{
    public ActivityObject activityObject { get; set; } = new ActivityObject();
    public string Url { get; set; }
    public string ApiKey { get; set; }
    public string ProfileName { get; set; }
    public int EpgOffset { get; set; }
    public EmbyProvider(string pUrl, string pApiKey, string pProfileName, int pEpgOffset) 
    {
        //Init connection
        Url = pUrl;
        ApiKey = pApiKey;
        ProfileName = pProfileName;
        EpgOffset = pEpgOffset;
        Console.WriteLine("Emby connection successfully initialized.");
    }

    public bool IsCurrentlyPlaying()
    {
        var options = new RestClientOptions(Url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("/emby/Sessions?api_key=" + ApiKey, Method.Get);
        RestResponse response = client.Execute(request);
        if(response.IsSuccessful)
        {
            List<Sessions.Class1> sessions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Sessions.Class1>>(response.Content);
            foreach(Sessions.Class1 c in sessions)
            {
                if(c.PlayState.CanSeek && c.UserName == ProfileName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SetRichPresence(DiscordRpcClient client)
    {
        //GetNewValues
        ActivityObject newActivityObject = GetActivityObject();
        if (newActivityObject is null)
        {
            if (client.CurrentPresence is not null) client.ClearPresence();
            return;
        }
        if (newActivityObject.Title == activityObject.Title && newActivityObject.Logo == activityObject.Logo && newActivityObject.Description == activityObject.Description && newActivityObject.IsPaused == activityObject.IsPaused)
        {
            return;
        }
        activityObject = newActivityObject;

        string largeImageText = "Unknown";
        if (IsLiveTV())
        {
            largeImageText = "Watching TV";
        }

        client.SetPresence(new RichPresence()
        {
            Timestamps = activityObject.DurationLeft != 0 ? Timestamps.FromTimeSpan(TimeSpan.FromMilliseconds(activityObject.DurationLeft)) : Timestamps.Now,
            Details = "Emby: " + activityObject.Title,
            State = activityObject.Description,
            Assets = new Assets()
            {
                LargeImageKey = activityObject.Logo,
                LargeImageText = largeImageText,
                SmallImageKey = activityObject.IsPaused ? "https://i.imgur.com/VomKC7b.png" : "https://i.imgur.com/cK2Tn8l.png",
                SmallImageText = activityObject.IsPaused ? "Paused" : "Playing"
            }
        });
    }

    private bool IsLiveTV()
    {
        var options = new RestClientOptions(Url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("/emby/Sessions?api_key=" + ApiKey, Method.Get);
        RestResponse response = client.Execute(request);
        if (response.IsSuccessful)
        {
            List<Sessions.Class1> sessions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Sessions.Class1>>(response.Content);
            foreach (Sessions.Class1 c in sessions)
            {
                if (c.PlayState.CanSeek && c.UserName == ProfileName)
                {
                    return c.NowPlayingItem.Type == "TvChannel";
                }
            }
        }
        return false;
    }

    private ActivityObject GetActivityObject()
    {
        var options = new RestClientOptions(Url)
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("/emby/Sessions?api_key=" + ApiKey, Method.Get);
        RestResponse response = client.Execute(request);
        if (response.IsSuccessful)
        {
            List<Sessions.Class1> sessions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Sessions.Class1>>(response.Content);
            foreach (Sessions.Class1 c in sessions)
            {
                if (c.PlayState.CanSeek && c.UserName == ProfileName)
                {
                    return new ActivityObject()
                    {
                        Description = "Program: " + c.NowPlayingItem.CurrentProgram.Name,
                        Logo = Url + "/emby/Items/" + c.NowPlayingItem.CurrentProgram.ParentId + "/Images/Primary?tag=" + c.NowPlayingItem.CurrentProgram.ChannelPrimaryImageTag + "&quality=9",
                        Title = c.NowPlayingItem.CurrentProgram.ChannelName,
                        IsPaused = c.PlayState.IsPaused,
                        DurationLeft = (long) (c.NowPlayingItem.CurrentProgram.EndDate.AddHours(EpgOffset) - DateTime.Now).TotalMilliseconds
                    };
                }
            }
        }

        return new ActivityObject()
        {
            Description = "",
            Logo = "",
            Title = "",
            IsPaused = false
        };
    }
}

using DiscordRPC;
using MediaDiscordRichPresence.EmbyModels;
using MediaDiscordRichPresence.Models;
using Plex.ServerApi.PlexModels.Media;
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
                if (c.NowPlayingItem.Path is not null && Config.Emby.HiddenLibraries.Contains(GetLibraryNameByPath(c.NowPlayingItem.Path))) return false;
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
            updatedRichPresence.Assets.LargeImageKey = GetCorrectImageUrl(updatedRichPresence.Assets.LargeImageKey);
            client.SetPresence(updatedRichPresence);
        }

        if (currentRichPresence is not null && (currentRichPresence.Details != updatedRichPresence.Details ||
            currentRichPresence.State != updatedRichPresence.State ||
            currentRichPresence.Assets.LargeImageText != updatedRichPresence.Assets.LargeImageText ||
            currentRichPresence.Assets.SmallImageText != updatedRichPresence.Assets.SmallImageText ||
            (Config.RichPresence.ShowTimeLeftIfPossible && ((CurrentActivityObject.DurationLeft - (SavedDurationLeft - (Config.RichPresence.RefreshIntervalInSeconds * 1000))) > 10000 ||
            (CurrentActivityObject.DurationLeft - (SavedDurationLeft - (Config.RichPresence.RefreshIntervalInSeconds*1000))) < -10000))))
        {
            if (CurrentActivityObject.IsPaused && Config.RichPresence.ShowTimeLeftIfPossible) updatedRichPresence.Timestamps = null;
            updatedRichPresence.Assets.LargeImageKey = GetCorrectImageUrl(updatedRichPresence.Assets.LargeImageKey);
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
                if (c.NowPlayingItem.Type == "Episode") return ActivityType.Show;
                if (c.NowPlayingItem.Type == "Movie") return ActivityType.Movie;
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
                switch(GetActivityType())
                {
                    case ActivityType.LiveTV:
                        return new ActivityObject()
                        {
                            Description = c.NowPlayingItem.CurrentProgram is null ? "" : "Program: " + c.NowPlayingItem.CurrentProgram.Name,
                            Logo = c.NowPlayingItem.CurrentProgram is not null && c.NowPlayingItem.CurrentProgram.ChannelPrimaryImageTag is not null ? Config.Emby.Url + "/emby/Items/" + c.NowPlayingItem.CurrentProgram.ParentId + "/Images/Primary?tag=" + c.NowPlayingItem.CurrentProgram.ChannelPrimaryImageTag + "&quality=9": Config.ImageTemplateLinks.Emby,
                            Title = c.NowPlayingItem.Name,
                            IsPaused = c.PlayState.IsPaused,
                            DurationLeft = c.NowPlayingItem.CurrentProgram is null ? 0 : (long)(c.NowPlayingItem.CurrentProgram.EndDate.AddHours(Config.Emby.EpgHourOffset) - DateTime.Now).TotalMilliseconds
                        };
                    case ActivityType.Movie:
                        TimeSpan movieDuration = TimeSpan.FromMilliseconds(c.NowPlayingItem.RunTimeTicks/10000);

                        string movieDurationStr = "";
                        if (movieDuration.Hours > 0) movieDurationStr += movieDuration.Hours + "h";
                        if (movieDuration.Minutes > 0) movieDurationStr += movieDuration.Minutes + "m";
                        if (movieDuration.Seconds > 0) movieDurationStr += movieDuration.Seconds + "s";

                        string genreStr = "";
                        foreach (string genre in c.NowPlayingItem.Genres)
                        {
                            if (genreStr != "") genreStr += ", ";
                            genreStr += genre;
                        }

                        return new ActivityObject()
                        {
                            Description = movieDurationStr + " · Genre: " + genreStr,
                            Logo = Config.Emby.Url + "/emby/Items/" + c.NowPlayingItem.Id + "/Images/Primary?maxWidth=200&tag=" + c.NowPlayingItem.ImageTags.Primary + "&quality=90",
                            Title = c.NowPlayingItem.Name + " (" + c.NowPlayingItem.ProductionYear.ToString() + ")",
                            IsPaused = c.PlayState.IsPaused,
                            DurationLeft = (c.NowPlayingItem.RunTimeTicks / 10000) - (c.PlayState.PositionTicks/10000)
                        };
                    case ActivityType.Show:

                        TimeSpan episodeDuration = TimeSpan.FromMilliseconds(c.NowPlayingItem.RunTimeTicks / 10000);

                        string activityObjectDescription = "";
                        if (episodeDuration.Days > 0) activityObjectDescription += episodeDuration.Days + "d";
                        if (episodeDuration.Hours > 0) activityObjectDescription += episodeDuration.Hours + "h";
                        if (episodeDuration.Minutes > 0) activityObjectDescription += episodeDuration.Minutes + "m";
                        if (episodeDuration.Seconds > 0) activityObjectDescription += episodeDuration.Seconds + "s";

                        activityObjectDescription += " · S";
                        if (c.NowPlayingItem.ParentIndexNumber.ToString().Length == 1)
                        {
                            activityObjectDescription += "0" + c.NowPlayingItem.ParentIndexNumber.ToString();
                        }
                        else
                        {
                            activityObjectDescription += c.NowPlayingItem.ParentIndexNumber.ToString();
                        }
                        activityObjectDescription += "E";

                        if (c.NowPlayingItem.IndexNumber.ToString().Length == 1)
                        {
                            activityObjectDescription += "0" + c.NowPlayingItem.IndexNumber.ToString();
                        }
                        else
                        {
                            activityObjectDescription += c.NowPlayingItem.IndexNumber.ToString();
                        }
                        activityObjectDescription += " · " + c.NowPlayingItem.Name;


                        return new ActivityObject()
                        {
                            Description = activityObjectDescription,
                            Logo = Config.Emby.Url + "/emby/Items/" + c.NowPlayingItem.SeriesId + "/Images/Primary?maxWidth=200&tag=" + c.NowPlayingItem.SeriesPrimaryImageTag + "&quality=90",
                            Title = c.NowPlayingItem.SeriesName,
                            IsPaused = c.PlayState.IsPaused,
                            DurationLeft = (c.NowPlayingItem.RunTimeTicks/10000) - (c.PlayState.PositionTicks/10000)
                        };
                }
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

    public string GetCorrectImageUrl(string pUrl)
    {
        if (Config.Images.UseProviderImageLinks) return pUrl.StartsWith("https://") ? pUrl : Config.ImageTemplateLinks.Emby;
        if (Config.Images.UseImgur) return ImgurUploader.UploadImage(pUrl, Config.Images.ImgurClientId, Config, "emby");
        return Config.ImageTemplateLinks.Emby;
    }

    public string GetLibraryNameByPath(string pPath)
    {
        var options = new RestClientOptions("https://emby.synex.dev:2017")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("/emby/Library/SelectableMediaFolders?api_key=" + Config.Emby.ApiKey, Method.Get);
        RestResponse response = client.Execute(request);
        List<EmbyLibraryMediaFolders.Class1> folders = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EmbyLibraryMediaFolders.Class1>>(response.Content);
        foreach(EmbyLibraryMediaFolders.Class1 folder in folders)
        {
            foreach(EmbyLibraryMediaFolders.Subfolder subfolder in folder.SubFolders)
            {
                if (pPath.Contains(subfolder.Path)) return folder.Name;
            }
        }
        return "Unknown";
    }
}

using DiscordRPC;
using Microsoft.Extensions.DependencyInjection;
using Plex.Api.Factories;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Server.Sessions;
using Plex.ServerApi.PlexModels.Media;
using MediaDiscordRichPresence.Models;
using RestSharp;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Data;

namespace MediaDiscordRichPresence;
public class PlexProvider : IProvider
{
    private ActivityObject CurrentActivityObject { get; set; } = new ActivityObject();
    private IPlexServerClient plexServerClient;
    private long SavedDurationLeft { get; set; } = 0;
    public Config Config { get; set; }
    public PlexProvider(Config pConfig, ServiceProvider pServiceProvider)
    {
        Config = pConfig;
        if(Config.Plex.Enabled) plexServerClient = pServiceProvider.GetService<IPlexServerClient>();
    }

    public bool IsCurrentlyPlaying()
    {
        var sessions = plexServerClient.GetSessionsAsync(Config.Plex.AuthToken, Config.Plex.Url).Result;
        if (sessions.Metadata is null) return false;

        foreach (SessionMetadata session in sessions.Metadata)
        {
            if (session.User.Title.Equals(Config.Plex.ProfileName))
            {
                if (Config.Plex.HiddenLibraries.Contains(session.LibrarySectionTitle)) return false;
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
        switch (GetActivityType())
        {
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
        if (Config.RichPresence.ShowTimeLeftIfPossible)
        {
            timestamps = CurrentActivityObject.DurationLeft != 0 ? Timestamps.FromTimeSpan(TimeSpan.FromMilliseconds(CurrentActivityObject.DurationLeft)) : Timestamps.Now;
        }

        RichPresence currentRichPresence = client.CurrentPresence;
        RichPresence updatedRichPresence = new RichPresence()
        {
            Timestamps = timestamps,
            Details = "Plex: " + CurrentActivityObject.Title,
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
            (CurrentActivityObject.DurationLeft - (SavedDurationLeft - (Config.RichPresence.RefreshIntervalInSeconds * 1000))) < -10000))))
        {
            if (CurrentActivityObject.IsPaused && Config.RichPresence.ShowTimeLeftIfPossible) updatedRichPresence.Timestamps = null;
            updatedRichPresence.Assets.LargeImageKey = GetCorrectImageUrl(updatedRichPresence.Assets.LargeImageKey);
            client.SetPresence(updatedRichPresence);
        }
        SavedDurationLeft = CurrentActivityObject.DurationLeft;
    }

    private ActivityType GetActivityType()
    {
        var sessions = plexServerClient.GetSessionsAsync(Config.Plex.AuthToken, Config.Plex.Url).Result;
        foreach (SessionMetadata session in sessions.Metadata)
        {
            if (session.User.Title.Equals(Config.Plex.ProfileName))
            {
                if (session.Type is null) return ActivityType.None;
                if (session.Key is not null && session.Key.Contains("livetv")) return ActivityType.LiveTV;
                switch (session.Type)
                {
                    case "movie":
                        return ActivityType.Movie;
                    case "episode":
                        return ActivityType.Show;
                }
            }
        }

        return ActivityType.None;
    }

    private ActivityObject GetActivityObject()
    {
        SessionMetadata selectedSession = null;
        var sessions = plexServerClient.GetSessionsAsync(Config.Plex.AuthToken, Config.Plex.Url).Result;

        foreach (SessionMetadata session in sessions.Metadata)
        {
            if (session.User.Title.Equals(Config.Plex.ProfileName))
            {
                selectedSession = session;
                break;
            }
        }

        if(selectedSession is null) throw new Exception("Something went wrong on getting the current activity of plex: No session found");

        switch (GetActivityType())
        {
            case ActivityType.LiveTV:

                try
                {
                    foreach (PlexLiveTv.DvrContainer.Dvr dvrContainer in GetDvrInfo().MediaContainer.Dvr)
                    {
                        foreach (PlexLiveTv.XmlTvGridContainer.Metadata program in GetXmlInfo(dvrContainer.epgIdentifier).MediaContainer.Metadata)
                        {
                            if (program.guid.Equals(selectedSession.Guid) && program.onAir)
                            {
                                foreach (PlexLiveTv.XmlTvGridContainer.Medium media in program.Media)
                                {
                                    if (media.onAir)
                                    {
                                        return new ActivityObject()
                                        {
                                            Description = "Program: " + selectedSession.Title,
                                            Logo = GetCorrectImageUrl(media.channelThumb),
                                            Title = media.channelCallSign,
                                            IsPaused = selectedSession.Player.State == "paused",
                                            DurationLeft = (long)(DateTimeOffset.FromUnixTimeSeconds(media.endsAt).UtcDateTime.AddHours(Config.Plex.EpgHourOffset) - DateTime.Now).TotalMilliseconds //EndDatum!!
                                        };
                                    }
                                }
                            }
                        }
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine("Plex Live TV Channel information could not be loaded: " + ex.ToString());
                }

                //Check available dvrs - https://plex.synex.dev:2018/livetv/dvrs - header X-Plex-Token
                //Check foreach dvr the xmltv - https://plex.synex.dev:2018/EPGIdentifier of dvr/grid?type=1
                //Check each xmltv for each dvr for program identifier - guid: tv.plex.xmltv://movie/Frauentausch
                //Check each found program if it is running in this moment - beginsAt endsAt // Unix timestamps 
                //If yes take Channel Name, Logo and Duration left / channelCallSign - channelThumb - endsAt 

                return new ActivityObject()
                {
                    Description = "Program: " + selectedSession.Title,
                    Logo = Config.ImageTemplateLinks.Plex,
                    Title = "",
                    IsPaused = selectedSession.Player.State == "paused",
                    DurationLeft = 0
                };
            case ActivityType.Show:

                TimeSpan episodeDuration = TimeSpan.FromMilliseconds(selectedSession.Duration);

                string activityObjectDescription = "";
                if (episodeDuration.Days > 0) activityObjectDescription += episodeDuration.Days + "d";
                if (episodeDuration.Hours > 0) activityObjectDescription += episodeDuration.Hours + "h";
                if (episodeDuration.Minutes > 0) activityObjectDescription += episodeDuration.Minutes + "m";
                if (episodeDuration.Seconds > 0) activityObjectDescription += episodeDuration.Seconds + "s";

                activityObjectDescription += " · S";
                if (selectedSession.ParentIndex.ToString().Length == 1)
                {
                    activityObjectDescription += "0" + selectedSession.ParentIndex.ToString();
                }
                else
                {
                    activityObjectDescription += selectedSession.ParentIndex.ToString();
                }
                activityObjectDescription += "E";

                if (selectedSession.Index.ToString().Length == 1)
                {
                    activityObjectDescription += "0" + selectedSession.Index.ToString();
                }
                else
                {
                    activityObjectDescription += selectedSession.Index.ToString();
                }
                activityObjectDescription += " · " + selectedSession.Title;


                return new ActivityObject()
                {
                    Description = activityObjectDescription,
                    Logo = Config.Plex.Url + selectedSession.GrandparentThumb + "?X-Plex-Token=" + Config.Plex.AuthToken,
                    Title = selectedSession.GrandparentTitle,
                    IsPaused = selectedSession.Player.State == "paused",
                    DurationLeft = selectedSession.Duration - selectedSession.ViewOffset
                };
            case ActivityType.Movie:
                TimeSpan movieDuration = TimeSpan.FromMilliseconds(selectedSession.Duration);

                string movieDurationStr = "";
                if (movieDuration.Hours > 0) movieDurationStr += movieDuration.Hours + "h";
                if (movieDuration.Minutes > 0) movieDurationStr += movieDuration.Minutes + "m";
                if (movieDuration.Seconds > 0) movieDurationStr += movieDuration.Seconds + "s";

                string genreStr = "";
                foreach (Genre genre in selectedSession.Genres)
                {
                    if (genreStr != "") genreStr += ", ";
                    genreStr += genre.Tag;
                }

                return new ActivityObject()
                {
                    Description = movieDurationStr + " · Genre: " + genreStr,
                    Logo = Config.Plex.Url + selectedSession.Thumb + "?X-Plex-Token=" + Config.Plex.AuthToken,
                    Title = selectedSession.Title + " (" + selectedSession.Year.ToString() + ")",
                    IsPaused = selectedSession.Player.State == "paused",
                    DurationLeft = selectedSession.Duration - selectedSession.ViewOffset
                };
        }
        throw new Exception("Something went wrong on getting the current activity of plex");
    }

    public string GetCorrectImageUrl(string pUrl)
    {
        if (Config.Images.UseProviderImageLinks) return pUrl.StartsWith("https://") ? pUrl : Config.ImageTemplateLinks.Plex;
        if (Config.Images.UseImgur) return ImgurUploader.UploadImage(pUrl, Config.Images.ImgurClientId, Config, "plex");
        return Config.ImageTemplateLinks.Plex;
    }

    public PlexLiveTv.DvrContainer.Rootobject GetDvrInfo()
    {
        var options = new RestClientOptions(Config.Plex.Url)
        {
            MaxTimeout = -1
        };
        var client = new RestClient(options);
        var request = new RestRequest("/livetv/dvrs", Method.Get);
        request.AddHeader("Accept", "application/json");
        request.AddHeader("X-Plex-Token", Config.Plex.AuthToken);
        RestResponse response = client.Execute(request);
        if (!response.IsSuccessful) throw new Exception("Could not get dvr info for plex live tv activity");

        return Newtonsoft.Json.JsonConvert.DeserializeObject<PlexLiveTv.DvrContainer.Rootobject>(response.Content);
    }

    public PlexLiveTv.XmlTvGridContainer.Rootobject GetXmlInfo(string pEpgIdentifier)
    {
        var options = new RestClientOptions(Config.Plex.Url + "/" + pEpgIdentifier)
        {
            MaxTimeout = -1
        };
        var client = new RestClient(options);
        var request = new RestRequest("/grid?type=1", Method.Get);
        request.AddHeader("Accept", "application/json");
        request.AddHeader("X-Plex-Token", Config.Plex.AuthToken);
        RestResponse response = client.Execute(request);
        if (!response.IsSuccessful) throw new Exception("Could not get xmltv info for plex live tv activity");

        return Newtonsoft.Json.JsonConvert.DeserializeObject<PlexLiveTv.XmlTvGridContainer.Rootobject>(response.Content);
    }
}

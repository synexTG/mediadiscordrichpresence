using DiscordRPC;
using Microsoft.Extensions.DependencyInjection;
using Plex.Api.Factories;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Server.Sessions;
using Plex.ServerApi.PlexModels.Media;

namespace MediaDiscordRichPresence;
public class PlexProvider : IProvider
{
    public ActivityObject activityObject { get; set; } = new ActivityObject();
    public IPlexFactory plexFactory;
    public IPlexServerClient plexServerClient;

    public string ProfileName { get; set; }
    public string AuthToken { get; set; }
    public string Url { get; set; }
    public PlexProvider(string pUrl, string pProfileName,string pAuthToken, ServiceProvider sServiceProvider)
    {
        //Init connection
        plexFactory = sServiceProvider.GetService<IPlexFactory>();
        plexServerClient = sServiceProvider.GetService<IPlexServerClient>();

        var sessions = plexServerClient.GetSessionsAsync(pAuthToken, pUrl).Result;

        ProfileName = pProfileName;
        AuthToken = pAuthToken;
        Url = pUrl;

        Console.WriteLine("Plex connection successfully initialized.");
    }

    public bool IsCurrentlyPlaying()
    {
        var sessions = plexServerClient.GetSessionsAsync(AuthToken, Url).Result;
        if (sessions.Metadata is null) return false;

        foreach (SessionMetadata session in sessions.Metadata)
        {
            if (session.User.Title.Equals(ProfileName))
            {
                return true;
            }
        }
        return false;
    }

    public void SetRichPresence(DiscordRpcClient client)
    {
        //GetNewValues
        ActivityObject newActivityObject = GetActivityObject();
        if(newActivityObject is null)
        {
            if(client.CurrentPresence is not null) client.ClearPresence();
            return;
        }
        if (newActivityObject.Title == activityObject.Title && newActivityObject.Logo == activityObject.Logo && newActivityObject.Description == activityObject.Description && newActivityObject.IsPaused == activityObject.IsPaused)
        {
            return;
        }
        activityObject = newActivityObject;

        string largeImageText = "Unknown";
        if (IsMovie())
        {
            largeImageText = "Watching a movie";
        }
        if (IsShow())
        {
            largeImageText = "Watching a show";
        }

        client.SetPresence(new RichPresence()
        {
            Timestamps = activityObject.DurationLeft != 0 ? Timestamps.FromTimeSpan(TimeSpan.FromMilliseconds(activityObject.DurationLeft)) : Timestamps.Now,
            Details = "Plex: " + activityObject.Title,
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

    private bool IsShow()
    {
        var sessions = plexServerClient.GetSessionsAsync(AuthToken, Url).Result;
        foreach (SessionMetadata session in sessions.Metadata)
        {
            if (session.User.Title.Equals(ProfileName))
            {
                if (session.Type == "episode") return true;
                break;
            }
        }
        return false;
    }

    private bool IsMovie()
    {
        var sessions = plexServerClient.GetSessionsAsync(AuthToken, Url).Result;
        foreach (SessionMetadata session in sessions.Metadata)
        {
            if (session.User.Title.Equals(ProfileName))
            {
                if (session.Type == "movie") return true;
                break;
            }
        }
        return false;
    }

    private ActivityObject GetActivityObject()
    {
        SessionMetadata selectedSession = null;
        var sessions = plexServerClient.GetSessionsAsync(AuthToken, Url).Result;
        if (sessions.Metadata is null) return null;

        foreach (SessionMetadata session in sessions.Metadata)
        {
            if (session.User.Title.Equals(ProfileName))
            {
                selectedSession = session;
                break;
            }
        }
        if (selectedSession is null) return null;

        if (IsShow())
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(selectedSession.Duration);
            string index = "S";
            if (selectedSession.ParentIndex.ToString().Length == 1)
            {
                index += "0" + selectedSession.ParentIndex.ToString();
            }
            else
            {
                index += selectedSession.ParentIndex.ToString();
            }
            index += "E";

            if (selectedSession.Index.ToString().Length == 1)
            {
                index += "0" + selectedSession.Index.ToString();
            }
            else
            {
                index += selectedSession.Index.ToString();
            }

            string durationStr = "";
            if(duration.Hours > 0)
            {
                durationStr += duration.Hours + "h";
            }
            if(duration.Minutes > 0)
            {
                durationStr += duration.Minutes + "m";
            }
            if (duration.Seconds > 0)
            {
                durationStr += duration.Seconds + "s";
            }

            return new ActivityObject()
            {
                Description = durationStr + " · " + index + " · " + selectedSession.Title,
                Logo = Url + selectedSession.GrandparentThumb + "?X-Plex-Token=" + AuthToken,
                Title = selectedSession.GrandparentTitle,
                IsPaused = selectedSession.Player.State == "paused",
                DurationLeft = selectedSession.Duration-selectedSession.ViewOffset
            };
        } else if(IsMovie())
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(selectedSession.Duration);
            
            string durationStr = "";
            if (duration.Hours > 0)
            {
                durationStr += duration.Hours + "h";
            }
            if (duration.Minutes > 0)
            {
                durationStr += duration.Minutes + "m";
            }
            if (duration.Seconds > 0)
            {
                durationStr += duration.Seconds + "s";
            }

            string genreStr = "";
            foreach(Genre genre in selectedSession.Genres) {
                if (genreStr != "") genreStr += ", ";
                genreStr += genre.Tag;
            }

            return new ActivityObject()
            {
                Description = durationStr + " · Genre: " + genreStr,
                Logo = Url + selectedSession.Thumb + "?X-Plex-Token=" + AuthToken,
                Title = selectedSession.Title + " (" + selectedSession.Year.ToString() + ")",
                IsPaused = selectedSession.Player.State == "paused",
                DurationLeft = selectedSession.Duration - selectedSession.ViewOffset
            };
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

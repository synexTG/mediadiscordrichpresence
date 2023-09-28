﻿using DiscordRPC;
using Microsoft.Extensions.DependencyInjection;
using Plex.Api.Factories;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Server.Sessions;
using Plex.ServerApi.PlexModels.Media;
using MediaDiscordRichPresence.Models;

namespace MediaDiscordRichPresence;
public class PlexProvider : IProvider
{
    private ActivityObject CurrentActivityObject { get; set; } = new ActivityObject();
    private IPlexServerClient plexServerClient;
    private Config Config { get; set; }
    public PlexProvider(Config pConfig, ServiceProvider pServiceProvider)
    {
        plexServerClient = pServiceProvider.GetService<IPlexServerClient>();

        Config = pConfig;
    }

    public bool IsCurrentlyPlaying()
    {
        var sessions = plexServerClient.GetSessionsAsync(Config.Plex.AuthToken, Config.Plex.Url).Result;
        if (sessions.Metadata is null) return false;

        foreach (SessionMetadata session in sessions.Metadata)
        {
            if (session.User.Title.Equals(Config.Plex.ProfileName))
            {
                return true;
            }
        }
        return false;
    }

    public void SetRichPresence(DiscordRpcClient client)
    {
        ActivityObject ActivityObject = GetActivityObject();

        if (ActivityObject.Title == CurrentActivityObject.Title 
            && ActivityObject.Logo == CurrentActivityObject.Logo 
            && ActivityObject.Description == CurrentActivityObject.Description 
            && ActivityObject.IsPaused == CurrentActivityObject.IsPaused) return;

        CurrentActivityObject = ActivityObject;

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

        client.SetPresence(new RichPresence()
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
        });
    }

    private ActivityType GetActivityType()
    {
        var sessions = plexServerClient.GetSessionsAsync(Config.Plex.AuthToken, Config.Plex.Url).Result;
        foreach (SessionMetadata session in sessions.Metadata)
        {
            if (session.User.Title.Equals(Config.Plex.ProfileName))
            {
                if (session.Type is null) return ActivityType.None;
                switch(session.Type)
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

        switch (GetActivityType())
        {
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
                activityObjectDescription += selectedSession.Title;


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
}
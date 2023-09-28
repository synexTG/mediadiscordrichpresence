namespace MediaDiscordRichPresence;
public class Config
{
    public sDiscord Discord { get; set; }
    public sRichPresence RichPresence { get; set; }
    public sPlex Plex { get; set; }
    public sEmby Emby { get; set; }
    public sImgages Images { get; set; }
    public sImageTemplateLinks ImageTemplateLinks { get; set; }
}
public class sDiscord
{
    public string ApplicationId { get; set; }
}

public class sRichPresence
{
    public bool RefreshConfigOnEveryCheck { get; set; }
    public bool ShowTimeLeftIfPossible { get; set; }
    public int RefreshIntervalInSeconds { get; set; }
    public int PriorityMode { get; set; }
    public string WatchingTV { get; set; }
    public string WatchingMovie { get; set; }
    public string WatchingShow { get; set; }
    public string WatchingUnknown { get; set; }
    public string Paused { get; set; }
    public string Playing { get; set; }
    
}

public class sPlex
{
    public bool Enabled { get; set; }
    public string Url { get; set; }
    public string ProfileName { get; set; }
    public string AuthToken { get; set; }
    public List<string> HiddenLibraries { get; set; }
}

public class sEmby
{
    public bool Enabled { get; set; }
    public string Url { get; set; }
    public string ProfileName { get; set; }
    public int EpgHourOffset { get; set; }
    public string ApiKey { get; set; }
    public List<string> HiddenLibraries { get; set; }
}

public class sImgages
{
    public bool UseProviderImageLinks { get; set; }
    public bool UseProviderImageLinksAsFallback { get; set; }
    public bool UseImgur { get; set; }
    public string ImgurClientId { get; set; }
}

public class sImageTemplateLinks
{
    public string Playing { get; set; }
    public string Paused { get; set; }
    public string Plex { get; set; }
    public string Emby { get; set; }
}

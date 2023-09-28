namespace MediaDiscordRichPresence;
public class Config
{
    public sPlex Plex { get; set; }
    public sEmby Emby { get; set; }
    public sImgages Images { get; set; }
    public sImageTemplateLinks ImageTemplateLinks { get; set; }
    public sRichPresence RichPresence { get; set; }
}

public class sPlex
{
    public bool Enabled { get; set; }
    public string PlexUrl { get; set; }
    public string PlexProfileName { get; set; }
    public string PlexAuthToken { get; set; }
}

public class sEmby
{
    public bool Enabled { get; set; }
    public string EmbyUrl { get; set; }
    public string EmbyProfileName { get; set; }
    public int EmbyEpgHourOffset { get; set; }
    public string EmbyApiKey { get; set; }
}

public class sImgages
{
    public bool UseProviderImageLinks { get; set; }
    public bool UseImgur { get; set; }
    public string ImgurApiKey { get; set; }
}

public class sImageTemplateLinks
{
    public string Playing { get; set; }
    public string Paused { get; set; }
    public string Plex { get; set; }
    public string Emby { get; set; }
}
public class sRichPresence
{
    public bool ShowTimeElapsed { get; set; }
}

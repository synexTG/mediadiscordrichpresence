namespace MediaDiscordRichPresence;
public class ActivityObject
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Logo { get; set; } = "";
    public bool IsPaused { get; set; } = false;
    public long DurationLeft { get; set; } = 0;
}

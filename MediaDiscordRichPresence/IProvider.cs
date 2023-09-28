using DiscordRPC;

namespace MediaDiscordRichPresence;
public interface IProvider
{
    public void SetRichPresence(DiscordRpcClient client);
}

using Newtonsoft.Json;

namespace MediaDiscordRichPresence;
public class ImageStore
{

    public static Dictionary<string, string> GetImages()
    {
        if(!File.Exists("ImageDataStore.json")) File.WriteAllText("ImageDataStore.json", Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string> { }));
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("ImageDataStore.json"));
    }

    public static string AddImage(string pProviderUrl, string pImgurUrl)
    {
        Dictionary<string, string> Images = GetImages();
        Images.Add(pProviderUrl, pImgurUrl);
        File.WriteAllText("ImageDataStore.json", Newtonsoft.Json.JsonConvert.SerializeObject(Images, Formatting.Indented));
        return pImgurUrl;
    }

}

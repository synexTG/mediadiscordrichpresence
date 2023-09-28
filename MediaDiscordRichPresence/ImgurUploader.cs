using MediaDiscordRichPresence.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaDiscordRichPresence;
public class ImgurUploader
{
    public static ImgurUploadResponse.Rootobject UploadImage(string pImageUrl, string pImgurClientId)
    {
        using var httClient = new HttpClient();
        var imageBytes = httClient.GetByteArrayAsync(pImageUrl).Result;
        string base64 = Convert.ToBase64String(imageBytes);

        var options = new RestClientOptions("https://api.imgur.com")
        {
            MaxTimeout = -1,
        };
        var client = new RestClient(options);
        var request = new RestRequest("/3/upload", Method.Post);
        request.AddHeader("Authorization", "Client-ID " + pImgurClientId);
        request.AlwaysMultipartFormData = true;
        request.AddParameter("image", base64);
        request.AddParameter("type", "base64");
        RestResponse response = client.Execute(request);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<ImgurUploadResponse.Rootobject>(response.Content);
    }
}

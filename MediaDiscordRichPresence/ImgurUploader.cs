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
    public static string UploadImage(string pImageUrl, string pImgurClientId, Config pConfig, string pProvider)
    {
        try
        {
            if(pImageUrl is null)
            {
                if (pProvider == "plex") return pConfig.ImageTemplateLinks.Plex;
                if (pProvider == "emby") return pConfig.ImageTemplateLinks.Emby;
            }
            Dictionary<string, string> Images = ImageStore.GetImages();
            if (Images.ContainsKey(pImageUrl!))
            {
                string imgurLink;
                if(Images.TryGetValue(pImageUrl!, out imgurLink!)) return imgurLink;
                if (pProvider == "plex") return pConfig.ImageTemplateLinks.Plex;
                if (pProvider == "emby") return pConfig.ImageTemplateLinks.Emby;
            }

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
            if (!response.IsSuccessful)
            {
                if (pConfig.Images.UseProviderImageLinksAsFallback && pImageUrl.StartsWith("https://")) return pImageUrl;
                if (pProvider == "plex") return pConfig.ImageTemplateLinks.Plex;
                if (pProvider == "emby") return pConfig.ImageTemplateLinks.Emby;
            }
            return ImageStore.AddImage(pImageUrl, Newtonsoft.Json.JsonConvert.DeserializeObject<ImgurUploadResponse.Rootobject>(response.Content).data.link);
        } catch(Exception ex)
        {
            Console.WriteLine("Image could not be uploaded: " + ex.ToString());
            if (pProvider == "plex") return pConfig.ImageTemplateLinks.Plex;
            if (pProvider == "emby") return pConfig.ImageTemplateLinks.Emby;
            return "";
        }
    }
}

namespace Telegram.Bot.Models
{
#pragma warning disable

    public class Panel
    {
        public int Id { get; set; }
        public string Url { get; set; }

        public string SharedSecret { get; set; }

        public string AdminSecret { get; set; }

        public User Owner { get; set; }      
    }

    public static class PanelExtension
    {
        public static string BuildUserUrl(this Panel panel)
        {
            return String.Format("{0}/{1}/{2}/api/v1/user", panel.Url, panel.SharedSecret, panel.AdminSecret);
        }

        public static string DocumentUrl(this Panel panel, string uuid)
        {
            return String.Format("{0}/{1}/{2}/all.txt?name=new_link_Ferra_New-unknown-new&asn=unknown&mode=new&base64=True",
                panel.Url, panel.SharedSecret, uuid);
        }

        public static async Task<string> DownloadDocument(this Panel panel, string uuid, HttpClient client)
        {
            using var message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(panel.DocumentUrl(uuid));
            var response = await client.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            }
            var base64 = await response.Content.ReadAsStringAsync();
            return base64;
        }
    }


    

}


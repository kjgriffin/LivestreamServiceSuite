using System.Net.Http;
using System.Threading.Tasks;

namespace Xenon.Helpers
{
    public class WebHelpers
    {
        public static readonly HttpClient httpClient = new HttpClient();

        public static async Task<string> DownloadText(string url)
        {
            var resp = await httpClient.GetAsync(url);
            var text = await resp.Content.ReadAsStringAsync();

            return text;
        }

    }
}

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using LSBHymnTool.MeterSearch;

using System.Drawing;
using System.Net;
using System.Text.Json;

namespace LSBHymnTool
{
    public class LSBAPIClient
    {
        static HttpClient _client = new HttpClient();

        private static void AddAuth(HttpRequestMessage msg)
        {
            Dictionary<string, string> headerDict = new Dictionary<string, string>
            {
                ["accept"] = "application/vnd.api+json",
                ["accept-language"] = "en-CA,en;q=0.9",
                ["authorization"] = "Token token=\"ywn7ksAdPDded1qMCVQP\", email=\"kristopher.j.griffin@gmail.com\"",
                ["x-owner-id"] = "35467524ebdd44f0952af09a2c9c15e8",
                ["x-requested-with"] = "XMLHttpRequest",
                ["x-tenant-id"] = "35467524ebdd44f0952af09a2c9c15e8",
                ["cookie"] = "_ga=GA1.1.1604655920.1692979956; __zlcmid=1HWm4aLGmqe2exN; _ga_2TKF1J6MHQ=GS1.1.1692979956.1.1.1692980160.0.0.0",
            };
            foreach (var item in headerDict)
            {
                msg.Headers.Add(item.Key, item.Value);
            }
        }

        public void ExtractHymnFromNumber()
        {

        }


        internal static async Task<(bool ok, string value)> SearchByMeter(string meter)
        {
            return await GeneralSearch($"meter:\"{meter}\"");
        }


        internal static async Task<(bool ok, string value)> GeneralSearch(string querry)
        {
            const string SearchURL = "https://app.lutheranservicebuilder.com/api/v1/search?query=%20";
            try
            {
                HttpRequestMessage req = new HttpRequestMessage();
                req.RequestUri = new Uri(SearchURL + Uri.EscapeDataString(querry));
                req.Method = HttpMethod.Get;
                AddAuth(req);
                var res = await _client.SendAsync(req);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    return (true, await res.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
            }
            return (false, "");
        }

        internal static async Task<(bool ok, string value)> GetHymnInfo(string hymnID)
        {
            const string URL = "https://app.lutheranservicebuilder.com/api/v1/packages?contentName=Info&packageId=hymn%2F";
            try
            {
                HttpRequestMessage req = new HttpRequestMessage();
                req.RequestUri = new Uri(URL + Uri.EscapeDataString(hymnID));
                req.Method = HttpMethod.Get;
                AddAuth(req);
                var res = await _client.SendAsync(req);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    return (true, await res.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
            }
            return (false, "");
        }

        internal static async Task<(bool ok, string value)> GetHymnText(string hymnID)
        {
            string URL = $"https://app.lutheranservicebuilder.com/api/v1/packages.html?preloadForCopy=1&packageId=hymn%2F{hymnID}&contentName=Text";
            try
            {
                HttpRequestMessage req = new HttpRequestMessage();
                req.RequestUri = new Uri(URL);
                req.Method = HttpMethod.Get;
                AddAuth(req);
                var res = await _client.SendAsync(req);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    return (true, await res.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
            }
            return (false, "");
        }
        internal static async Task<(bool ok, string value)> GetHymnMelody(string hymnID)
        {
            string URL = $"https://app.lutheranservicebuilder.com/api/v1/packages.html?preloadForCopy=1&packageId=hymn%2F{hymnID}&contentName=Melody";
            try
            {
                HttpRequestMessage req = new HttpRequestMessage();
                req.RequestUri = new Uri(URL);
                req.Method = HttpMethod.Get;
                AddAuth(req);
                var res = await _client.SendAsync(req);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    return (true, await res.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
            }
            return (false, "");
        }
        internal static async Task<(bool ok, Stream stream)> GetLSBImage(string url)
        {
            try
            {
                HttpRequestMessage req = new HttpRequestMessage();
                req.RequestUri = new Uri(url);
                req.Method = HttpMethod.Get;
                AddAuth(req);
                var res = await _client.SendAsync(req);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    return (true, await res.Content.ReadAsStreamAsync());
                }
            }
            catch (Exception ex)
            {
            }
            return (false, null);
        }



    }

    public class LSBAPI
    {
        public static async Task<string> GetLSBHymnID(string searchnumber)
        {
            var res = await LSBAPIClient.GeneralSearch(searchnumber);
            if (res.ok)
            {
                var results = JsonSerializer.Deserialize<GeneralSearch.LSBSearchResults>(res.value);
                // should be a hymn match
                var hymn = results.data.FirstOrDefault(r => r.attributes.section == "Results for Hymns");
                return hymn.id.Remove(0, "hymn/".Length);
            }
            return "";
        }
        public static async Task<HymnInfoSearch.HymnInfoAttributes> GetLSBHymnMetaInfo(string hymnid)
        {
            var res = await LSBAPIClient.GetHymnInfo(hymnid);
            if (res.ok)
            {
                var result = JsonSerializer.Deserialize<HymnInfoSearch.HymnInfoSearchResult>(res.value);
                return result.data.attributes;
            }
            return null;
        }
        public static async Task<List<LSBMeterSearchAttributes>> GetLSBAltTunesByMeter(string meter)
        {
            var res = await LSBAPIClient.SearchByMeter(meter);
            if (res.ok)
            {
                var result = JsonSerializer.Deserialize<MeterSearch.LSBMeterSearch>(res.value);
                return result.data.Where(x => x.attributes.section == "Results for Hymns").Select(x => x.attributes).ToList();
            }
            return null;
        }
        public static async Task<TextHymn> GetLSBHymnText(string hymnid)
        {
            var res = await LSBAPIClient.GetHymnText(hymnid);
            if (res.ok)
            {
                // parse the html?
                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(res.value);
                var verses = document.QuerySelectorAll("p.numbered-stanza").Where(x => x.LocalName == "p");
                TextHymn hymn = new TextHymn()
                {
                    Name = hymnid,
                    Number = hymnid,
                    Verses = new List<TextVerse>(),
                };
                foreach (var child in verses)
                {
                    var number = child.QuerySelectorAll(".stanza-number")?.FirstOrDefault()?.TextContent;
                    TextVerse verse = new TextVerse();
                    verse.StanzaId = number;
                    if (child.ClassList.Contains("numbered-stanza"))
                    {
                        var words = child.TextContent.Replace("/t", "");
                        var trimmed = words.Remove(0, number.Length).Trim();
                        verse.Words = trimmed;
                    }
                    if (child.ClassList.Contains("doxological-numbered-stanza"))
                    {
                        // TODO:
                        verse.Words = child.TextContent;
                    }
                    hymn.Verses.Add(verse);
                }
                return hymn;
            }
            return null;
        }
        public static async Task<List<string>> GetLSBHymnImageURLs(string hymnid)
        {
            var res = await LSBAPIClient.GetHymnMelody(hymnid);
            if (res.ok)
            {
                // parse the html?
                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(res.value);
                var images = document.QuerySelectorAll("img");

                List<string> urls = new List<string>();

                foreach (var img in images)
                {
                    urls.Add(img.GetAttribute("src") ?? "");
                }

                return urls;
            }
            return null;
        }

        public static async Task<Bitmap> GetLSBImageFromURL(string url)
        {
            var res = await LSBAPIClient.GetLSBImage(url);
            if (res.ok)
            {
                res.stream.Seek(0, SeekOrigin.Begin);
                var bitmap = new Bitmap(res.stream);
                return bitmap;
            }
            return null;

        }


    }





}
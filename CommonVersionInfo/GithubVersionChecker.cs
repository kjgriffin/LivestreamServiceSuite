using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommonVersionInfo
{
    public static class GithubVersionChecker
    {



        public static async Task<BuildVersion> GetCurentReleaseVersion(HttpClient client)
        {
            if (client == null)
            {
                client = new HttpClient();
            }

            var resp = await client.GetAsync("https://raw.githubusercontent.com/kjgriffin/LivestreamServiceSuite/master/ReleaseVersions.json");
            var json = await resp.Content.ReadAsStringAsync();
            var versionHistory = JsonSerializer.Deserialize<ReleaseVersionHistory>(json);

            return BuildVersion.Parse(versionHistory.CurentVersion);
        }

    }
}

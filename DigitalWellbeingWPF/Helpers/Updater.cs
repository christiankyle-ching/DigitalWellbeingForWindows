using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DigitalWellbeingWPF.Helpers
{
    public static class Updater
    {
        public static readonly string appGithubLink = "https://github.com/christiankyle-ching/DigitalWellbeingForWindows";
        public static readonly string appReleasesLink = "https://github.com/christiankyle-ching/DigitalWellbeingForWindows/releases";
        public static readonly string appWebsiteLink = "https://christiankyleching.vercel.app/works.html?scrollTo=digital-wellbeing-windows";

        static readonly HttpClient client = new HttpClient();
        static readonly string appGithubLink_ReleasesAPIURL =
                    "https://api.github.com/repos/christiankyle-ching/DigitalWellbeingForWindows/releases?per_page=1";

        public static async Task<string> GetLatestVersion()
        {
            try
            {
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

                string responseBody = await client.GetStringAsync(appGithubLink_ReleasesAPIURL);

                Regex rx = new Regex(@"""tag_name"":""([a-z0-9.]*)""");
                Match match = rx.Match(responseBody);

                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            return "";
        }

        public static int ParseVersion(string strVersion)
        {
            int version = -1;

            if (strVersion == string.Empty) return version;

            strVersion = Regex.Replace(strVersion, "[^0-9.]", ""); // Remove all non-numbers and period

            if (int.TryParse(strVersion, out version))
            {
                return version;
            }

            return version;
        }

        public static bool IsUpdateAvailable(int curVersion, int latestVersion)
        {
            return latestVersion > curVersion;
        }
    }
}

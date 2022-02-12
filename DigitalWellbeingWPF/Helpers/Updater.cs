using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DigitalWellbeingWPF.Helpers
{
    public static class Updater
    {
        public static readonly string appGithubLink = "https://github.com/christiankyle-ching/DigitalWellbeingForWindows";
        public static readonly string appReleasesLink = "https://github.com/christiankyle-ching/DigitalWellbeingForWindows/releases/latest";
        public static readonly string appWebsiteLink = "https://christiankyleching.vercel.app/works.html?scrollTo=digital-wellbeing-windows";

        static readonly HttpClient client = new HttpClient();
        static readonly string appGithubLink_ReleasesAPIURL =
                    "https://api.github.com/repos/christiankyle-ching/DigitalWellbeingForWindows/releases/latest";

        // Versions should be:
        // 1.0.1        -> OK
        // 1.0.1.0      -> OK
        // 1.0.1.0.1    -> NOT OK
        static readonly int VERSION_SEGMENT_LENGTH = 4;

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

        public static bool IsUpdateAvailable(int[] curVersion, int[] latestVersion)
        {
            // Check new version from major(left) to minor(right) rev
            for (int i = 0; i < VERSION_SEGMENT_LENGTH; i++)
            {
                if (latestVersion[i] == curVersion[i])
                {
                    continue;
                }
                else if (latestVersion[i] > curVersion[i])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public static int[] VersionStringToIntArray(string version)
        {
            List<int> segments = new List<int>();
            string[] tmp = version.Split('.');

            // Get all version segments first in int
            // It is guaranteed that every split has a number because
            // A version like "3..2" is not a given
            foreach (string seg in tmp)
            {
                segments.Add(int.Parse(seg));
            }

            // Fill empty slots with -1
            // Make them all 4 in length
            for (int i = 0; i < VERSION_SEGMENT_LENGTH - segments.Count; i++)
            {
                segments.Add(-1);
            }

            return segments.ToArray();
        }

        public static string VersionArrayToString(int[] version)
        {
            string strVersion = "";

            for (int i = 0; i < version.Length; i++)
            {
                // Append version number and dot if greater than 0, else append empty
                strVersion += version[i] >= 0 ? $"{version[i]}." : "";
            }

            // Remove end string
            return strVersion.Remove(strVersion.Length - 1, 1);
        }

        public static string FormatVersionString(string version)
        {
            // Removes 'v' prefix
            return version.Replace("v", "");
        }

        public static async Task<string> CheckForUpdates()
        {
            try
            {
                string strCurrent = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string strLatest = await GetLatestVersion();

                Console.WriteLine($"Current Version: {strCurrent}");
                Console.WriteLine($"Latest Version: {strLatest}");

                int[] curVersion = VersionStringToIntArray(FormatVersionString(strCurrent));
                int[] latestVersion = VersionStringToIntArray(FormatVersionString(strLatest));

                return IsUpdateAvailable(curVersion, latestVersion) ? strLatest : "";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLine($"Updater: Failed to check for updates. {ex}");
            }

            return "";
        }
    }
}

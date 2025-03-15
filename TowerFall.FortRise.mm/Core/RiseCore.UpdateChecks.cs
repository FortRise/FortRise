using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FortRise;

public partial class RiseCore
{
    public static class UpdateChecks
    {
        private const string API_REPO_REF_LINK = "https://api.github.com/repos/Terria-K/FortRise/git/refs/tags";
        private static string version;
        public static string UpdateMessage = "Checking for an update...";
        public struct UpdateRef
        {
            [JsonPropertyName("ref")]
            public string Ref { get; set; }
        }

        public static async Task<Result<UpdateRef, string>> CheckUpdate()
        {
            try 
            {
                using var webClient = new WebClient();
                webClient.Headers.Add(HttpRequestHeader.Accept, "application/vnd.github+json");
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "FortRise/" + FortRiseVersion.ToString());
                var json = await webClient.DownloadStringTaskAsync(new Uri(API_REPO_REF_LINK));

                var updateRefs = JsonSerializer.Deserialize<List<UpdateRef>>(json);
                updateRefs.Reverse();

                return updateRefs[0];
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return "Could not get an update to FortRise.";
            }
        }

        public static void UpdateConfirm(string updateVersion)
        {
            version = updateVersion;
        }

        public static void OpenUpdateURL()
        {
            string url;
            if (version == null)
            {
                url = $"https://github.com/Terria-K/FortRise/releases";
            }
            else 
            {
                url = $"https://github.com/Terria-K/FortRise/releases/tag/{version}";
            }

            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }   
    }
}
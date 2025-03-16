using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FortRise;

public partial class RiseCore
{
    public static class UpdateChecks
    {
        private const string API_REPO_REF_LINK = "https://api.github.com/repos/FortRise/FortRise/git/refs/tags";
        private static string version;
        public static string UpdateMessage = "CHECKING FOR AN UPDATE...";
        public static bool FortRiseUpdateAvailable;
        public static HashSet<ModuleMetadata> HasUpdates = new HashSet<ModuleMetadata>();
        public static Dictionary<ModuleMetadata, string> Tags = new Dictionary<ModuleMetadata, string>();
        public struct UpdateRef
        {
            [JsonPropertyName("ref")]
            public string Ref { get; set; }

            [JsonIgnore]
            public Version Version { get; set; }
        }

        public static async Task<Result<bool, string>> CheckModUpdate(ModuleMetadata metadata)
        {
            if (metadata.Update == null || metadata.Update.GH == null || string.IsNullOrEmpty(metadata.Update.GH.Repository))
            {
                return false;
            }
            var urlRepository = $"https://api.github.com/repos/{metadata.Update.GH.Repository}/git/refs/tags";

            try
            {
                using var webClient = new WebClient();
                webClient.Headers.Add(HttpRequestHeader.Accept, "application/vnd.github+json");
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "FortRise/" + FortRiseVersion.ToString());
                var json = await webClient.DownloadStringTaskAsync(new Uri(urlRepository));

                var updateRefs = JsonSerializer.Deserialize<UpdateRef[]>(json);

                if (!string.IsNullOrEmpty(metadata.Update.GH.TagRegex))
                {
                    var regex = new Regex(metadata.Update.GH.TagRegex);
                    var list = updateRefs
                        .Where(x => regex.IsMatch(TrimUrl(x.Ref)))
                        .Select(x => {
                            x.Version = new Version(regex.Match(TrimUrl(x.Ref)).Groups[1].Value);
                            return x;
                        })
                        .ToArray();

                    updateRefs = list;
                }

                Array.Reverse(updateRefs);
                var r = updateRefs[0];

                Tags.Add(metadata, TrimUrl(r.Ref));

                if (r.Version == null)
                {
                    if (!System.Version.TryParse(TrimUrl(r.Ref), out Version version))
                    {
                        return "ERROR PARSING THE VERSION NUMBER.";
                    }
                    r.Version = version;
                }


                if (r.Version > metadata.Version)
                {
                    HasUpdates.Add(metadata);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return $"Could not get an update to {metadata.Name}.";
            }
        }

        public static async Task<Result<UpdateRef, string>> CheckFortRiseUpdate()
        {
            try 
            {
                using var webClient = new WebClient();
                webClient.Headers.Add(HttpRequestHeader.Accept, "application/vnd.github+json");
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "FortRise/" + FortRiseVersion.ToString());
                var json = await webClient.DownloadStringTaskAsync(new Uri(API_REPO_REF_LINK));

                var updateRefs = JsonSerializer.Deserialize<UpdateRef[]>(json);
                Array.Reverse(updateRefs);

                return updateRefs[0];
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return "Could not get an update to FortRise.";
            }
        }

        public static void UpdateFortRiseConfirm(Version version)
        {
            if (version > FortRiseVersion)
            {
                FortRiseUpdateAvailable = true;
            }
        }

        public static void OpenFortRiseUpdateURL()
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
            OpenURL(url);
        }

        public static void OpenGithubURL(string repo)
        {
            OpenURL($"https://github.com/{repo}");
        }

        public static void OpenGithubModUpdateURL(ModuleMetadata metadata, string repo)
        {
            var releaseTag = Tags[metadata];
            OpenURL($"https://github.com/{repo}/releases/tag/{releaseTag}");
        }

        private static void OpenURL(string url)
        {
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

        private static string TrimUrl(string url)
        {
            int index = url.LastIndexOf('/');
            return url.Substring(index + 1);
        }
    }
}
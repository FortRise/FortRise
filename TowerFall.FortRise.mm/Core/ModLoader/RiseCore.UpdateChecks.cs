using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
        internal static string UpdateMessage = "CHECKING FOR AN UPDATE...";
        public static bool FortRiseUpdateAvailable;
        public static HashSet<ModuleMetadata> HasUpdates = new HashSet<ModuleMetadata>();
        public static Dictionary<ModuleMetadata, string> Tags = new Dictionary<ModuleMetadata, string>();


        public static bool IsUpdateAvailable(FortModule module)
        {
            return IsUpdateAvailable(module.Meta);
        }

        public static bool IsUpdateAvailable(ModuleMetadata metadata)
        {
            return HasUpdates.Contains(metadata);
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
                string json;
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/vnd.github+json");
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, "FortRise/" + FortRiseVersion.ToString());
                    json = await webClient.DownloadStringTaskAsync(new Uri(urlRepository));
                }

                var updateRefs = JsonSerializer.Deserialize<UpdateRef[]>(json);

                if (!string.IsNullOrEmpty(metadata.Update.GH.TagRegex))
                {
                    var regex = new Regex(metadata.Update.GH.TagRegex);
                    var list = updateRefs
                        .Where(x => regex.IsMatch(TrimUrl(x.Ref)))
                        .Select(x => {
                            x.Version = new SemanticVersion(regex.Match(TrimUrl(x.Ref)).Groups[1].Value);
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
                    if (!SemanticVersion.TryParse(TrimUrl(r.Ref), out SemanticVersion version))
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

        public static async Task<Result<bool, string>> DownloadUpdate(ModuleMetadata metadata)
        {
            var tagName = Tags[metadata];

            var urlTaggedRelease = $"https://api.github.com/repos/{metadata.Update.GH.Repository}/releases/tags/{tagName}";
            try 
            {
                string fortriseAgent = "FortRise/" + FortRiseVersion.ToString();
                string json;
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/vnd.github+json");
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, fortriseAgent);
                    json = await webClient.DownloadStringTaskAsync(new Uri(urlTaggedRelease));
                }

                var updateRelease = JsonSerializer.Deserialize<UpdateRelease>(json);
                var firstAsset = updateRelease.Assets[0];

                var downloadURI = new Uri(firstAsset.BrowserDownloadUrl);
                byte[] data;
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/zip");
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, fortriseAgent);
                    data = await webClient.DownloadDataTaskAsync(downloadURI);
                }

                if (!ValidateReleaseByte(data))
                {
                    return "First release does not have a valid mod metadata.";
                }

                using (var fs = File.Create(Path.Combine("ModUpdater", firstAsset.Name)))
                {
                    fs.Write(data, 0, data.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return "Could not get a download file from this repository.";
            }
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

        private static bool ValidateReleaseByte(byte[] data)
        {
            try 
            {
                using var ms = new MemoryStream(data);
                using var zip = new ZipArchive(ms);
                var entry = zip.GetEntry("meta.json");

                if (entry == null)
                {
                    return false;
                }

                using var jsonMs = entry.ExtractStream();
                if (!ModuleMetadata.ParseMetadata(null, jsonMs).Check(out _, out string err))
                {
                    Logger.Error(err);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        internal static async Task<Result<UpdateRef, string>> CheckFortRiseUpdate()
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

        internal static void UpdateFortRiseConfirm(SemanticVersion version)
        {
            if (version > FortRiseVersion)
            {
                FortRiseUpdateAvailable = true;
            }
        }

        internal static void OpenFortRiseUpdateURL()
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

        public struct UpdateRef
        {
            [JsonPropertyName("ref")]
            public string Ref { get; set; }

            [JsonIgnore]
            [JsonConverter(typeof(SemanticVersionConverter))]
            public SemanticVersion Version { get; set; }
        }

        public struct UpdateRelease
        {
            [JsonPropertyName("tag_name")]
            public string TagName { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("assets")]
            public UpdateReleaseAssets[] Assets { get; set; }
        }

        public struct UpdateReleaseAssets 
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }
        }
    }
}
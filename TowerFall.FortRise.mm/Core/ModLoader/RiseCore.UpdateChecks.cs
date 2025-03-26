using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
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
        private static readonly Lock syncRoot = new Lock();


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
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "FortRise/" + FortRiseVersion.ToString());
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
                    json = await client.GetStringAsync(urlRepository);
                }

                var updateRefs = JsonSerializer.Deserialize<UpdateRef[]>(json);

                if (!string.IsNullOrEmpty(metadata.Update.GH.TagRegex))
                {
                    var regex = new Regex(metadata.Update.GH.TagRegex);
                    var list = updateRefs
                        .Where(x => regex.IsMatch(TrimUrl(x.Ref)))
                        .Select(x => {
                            x.Version = new SemanticVersion(regex.Match(new string(TrimUrl(x.Ref))).Groups[1].Value);
                            return x;
                        })
                        .ToArray();

                    updateRefs = list;
                }

                Array.Reverse(updateRefs);
                var r = updateRefs[0];

                lock (syncRoot)
                {
                    Tags.Add(metadata, new string(TrimUrl(r.Ref)));
                }

                if (r.Version == SemanticVersion.Empty)
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
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "FortRise/" + FortRiseVersion.ToString());
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
                    json = await client.GetStringAsync(urlTaggedRelease);
                }

                var updateRelease = JsonSerializer.Deserialize<UpdateRelease>(json);
                var firstAsset = updateRelease.Assets[0];

                byte[] data;
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "FortRise/" + FortRiseVersion.ToString());
                    client.DefaultRequestHeaders.Add("Accept", "application/zip");
                    data = await client.GetByteArrayAsync(firstAsset.BrowserDownloadUrl);
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
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "FortRise/" + FortRiseVersion.ToString());
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
                var json = await client.GetStringAsync(API_REPO_REF_LINK);

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

        private static ReadOnlySpan<char> TrimUrl(ReadOnlySpan<char> url)
        {
            int index = url.LastIndexOf('/');
            return url.Slice(index + 1);
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
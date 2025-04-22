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
using System.Threading.Tasks;

namespace FortRise;

public partial class RiseCore
{
    public static partial class UpdateChecks
    {
        private const string API_REPO_REF_LINK = "https://api.github.com/repos/FortRise/FortRise/git/refs/tags";
        private static string version;
        internal static string UpdateMessage = "CHECKING FOR AN UPDATE...";
        public static bool FortRiseUpdateAvailable;
        public static HashSet<ModuleMetadata> HasUpdates = new HashSet<ModuleMetadata>();


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
            if (metadata.Update is null)
            {
                return false;
            }

            Result<bool, string> results = false;

            if (metadata.Update.GH != null && !string.IsNullOrEmpty(metadata.Update.GH.Repository))
            {
                results = await Github.CheckModUpdate(metadata);
            }

            if (metadata.Update.GB != null && metadata.Update.GB.ID != null)
            {
                results = await GameBanana.CheckModUpdate(metadata);
            }
            return results;

        }

        public static async Task<Result<bool, string>> DownloadUpdate(ModuleMetadata metadata)
        {
            if (GameBanana.MetadataGBUpdates.Contains(metadata))
            {
                return await GameBanana.DownloadUpdate(metadata);
            }
            if (Github.MetadataGHUpdates.Contains(metadata))
            {
                return await Github.DownloadUpdate(metadata);
            }

            return false;
        }

        public static void OpenGithubURL(string repo)
        {
            OpenURL($"https://github.com/{repo}");
        }


        internal static bool ValidateReleaseByte(byte[] data)
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

        internal static async Task<Result<Github.UpdateRef, string>> CheckFortRiseUpdate()
        {
            try 
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "FortRise/" + FortRiseVersion.ToString());
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
                var json = await client.GetStringAsync(API_REPO_REF_LINK);

                var updateRefs = JsonSerializer.Deserialize<Github.UpdateRef[]>(json);
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

        internal static void OpenURL(string url)
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

        internal static ReadOnlySpan<char> TrimUrl(ReadOnlySpan<char> url)
        {
            int index = url.LastIndexOf('/');
            return url.Slice(index + 1);
        }

        public static class GameBanana
        {
            public static HashSet<ModuleMetadata> MetadataGBUpdates = new HashSet<ModuleMetadata>();
            public static async Task<Result<bool, string>> CheckModUpdate(ModuleMetadata metadata)
            {
                var urlMod = $"https://api.gamebanana.com/Core/Item/Data?itemtype=Mod&itemid={metadata.Update.GB.ID}&fields=Updates().aGetLatestUpdates()";
                try
                {
                    string json;
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "FortRise/" + FortRiseVersion.ToString());
                        json = await client.GetStringAsync(urlMod);
                    }

                    var updateInfos = JsonSerializer.Deserialize<UpdateInfo[][]>(json);

                    if (updateInfos.Length == 0)
                    {
                        return false;
                    }

                    UpdateInfo info = updateInfos[0][0];
                    if (!SemanticVersion.TryParse(info.Version, out SemanticVersion version))
                    {
                        return "ERROR, INVALID VERSION PARSING PATTERN";
                    }

                    if (version > metadata.Version)
                    {
                        HasUpdates.Add(metadata);
                        MetadataGBUpdates.Add(metadata);
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
                var urlTaggedRelease = $"https://api.gamebanana.com/Core/Item/Data?itemtype=Mod&itemid={metadata.Update.GB.ID}&fields=Files().aFiles()";
                try 
                {
                    string fortriseAgent = "FortRise/" + FortRiseVersion.ToString();
                    string json;
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "FortRise/" + FortRiseVersion.ToString());
                        json = await client.GetStringAsync(urlTaggedRelease);
                    }

                    var updateRelease = JsonSerializer.Deserialize<Dictionary<string, DownloadInfo>[]>(json);
                    var firstAsset = updateRelease[0].FirstOrDefault();
                    if (firstAsset.Key == null)
                    {
                        return false;
                    }

                    byte[] data;
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "FortRise/" + FortRiseVersion.ToString());
                        client.DefaultRequestHeaders.Add("Accept", "application/zip");
                        data = await client.GetByteArrayAsync(firstAsset.Value.DownloadURL);
                    }

                    if (!UpdateChecks.ValidateReleaseByte(data))
                    {
                        return "First release does not have a valid mod metadata.";
                    }

                    using (var fs = File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModUpdater", $"{metadata.Name}.zip")))
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
            
            public struct UpdateInfo 
            {
                [JsonPropertyName("_sVersion")]
                public string Version { get; set; }
            }

            public struct DownloadInfo 
            {
                [JsonPropertyName("_sDownloadUrl")]
                public string DownloadURL { get; set; }
            }
        }
    }
}
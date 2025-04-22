using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace FortRise;

public partial class RiseCore
{
    public static partial class UpdateChecks
    {
        internal static JsonSerializerOptions UpdaterInfoOptions = new();
        private const string API_REPO_REF_LINK = "https://api.github.com/repos/FortRise/FortRise/git/refs/tags";
        private static string version;
        internal static string UpdateMessage = "CHECKING FOR AN UPDATE...";
        public static bool FortRiseUpdateAvailable;
        public static HashSet<ModuleMetadata> HasUpdates = new HashSet<ModuleMetadata>();

        static UpdateChecks()
        {
            UpdaterInfoOptions.Converters.Add(new SemanticVersionConverter());
        }


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

        internal static void AddToUpdaterList(ModuleMetadata metadata, string updatePath, SemanticVersion updateVersion)
        {
            var updaterListFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModUpdater", "updater.json");
            List<UpdaterInfo> updaterInfo;
            if (!File.Exists(updaterListFile))
            {
                updaterInfo = new List<UpdaterInfo>();
            }
            else 
            {
                string jsonInput = File.ReadAllText(updaterListFile);
                updaterInfo = JsonSerializer.Deserialize<List<UpdaterInfo>>(jsonInput, UpdaterInfoOptions);
            }

            string path;

            if (metadata.IsZipped)
            {
                path = metadata.PathZip;
            }
            else 
            {
                path = metadata.PathDirectory;
            }

            updaterInfo.Add(new UpdaterInfo(metadata.Name, metadata.Version, updateVersion, path, updatePath, metadata.IsZipped));

            string json = JsonSerializer.Serialize(updaterInfo, UpdaterInfoOptions);
            File.WriteAllText(updaterListFile, json);
        }

        public static void OpenGithubURL(string repo)
        {
            OpenURL($"https://github.com/{repo}");
        }

        internal static bool ValidateReleaseByte(byte[] data, out SemanticVersion version)
        {
            try 
            {
                using var ms = new MemoryStream(data);
                using var zip = new ZipArchive(ms);
                var entry = zip.GetEntry("meta.json");

                version = default;
                if (entry == null)
                {
                    return false;
                }

                using var jsonMs = entry.ExtractStream();
                if (!ModuleMetadata.ParseMetadata(null, jsonMs).Check(out ModuleMetadata m, out string err))
                {
                    Logger.Error(err);
                    return false;
                }

                version = m.Version;

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                version = default;
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
            return url[(index + 1)..];
        }

        public struct UpdaterInfo(string modName, SemanticVersion version, SemanticVersion updateVersion, string modPath, string updateModPath, bool isZipped)
        {
            public string ModName { get; set; } = modName;
            public SemanticVersion Version { get; set; } = version;
            public SemanticVersion UpdateVersion { get; set; } = updateVersion;
            public string ModPath { get; set; } = modPath;
            public string UpdateModPath { get; set; } = updateModPath;
            public bool IsZipped { get; set; } = isZipped;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FortRise;

public partial class RiseCore
{
    public static partial class UpdateChecks
    {
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
                        var j = await client.GetStringAsync(urlMod);
                        json = j;
                    }

                    var updateInfos = JsonSerializer.Deserialize<UpdateInfo[][]>(json);

                    if (updateInfos.Length == 0)
                    {
                        return false;
                    }

                    if (updateInfos[0].Length == 0)
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
                        Logger.Warning($"Update {metadata.Name} {metadata.Version} -> {metadata.Name} {version}");
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

                    var downloadedChecksum = Convert.FromHexString(firstAsset.Value.MD5Checksum);

                    var hashedBytes = MD5.HashData(data);

                    if (!downloadedChecksum.SequenceEqual(hashedBytes))
                    {
                        return "Checksum is not valid";
                    }

                    if (!ValidateReleaseByte(data, out SemanticVersion version))
                    {
                        return "Latest Update file does not have a valid mod metadata.";
                    }

                    string updatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModUpdater", $"{metadata.Name}.zip");

                    using (var fs = File.Create(updatePath))
                    {
                        fs.Write(data, 0, data.Length);
                    }

                    AddToUpdaterList(metadata, updatePath, version);
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
                [JsonPropertyName("_sMd5Checksum")]
                public string MD5Checksum { get; set; }
            }
        }
    }
}
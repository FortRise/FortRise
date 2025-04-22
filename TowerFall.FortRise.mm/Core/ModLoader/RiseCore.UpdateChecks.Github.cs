using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FortRise;

public partial class RiseCore
{
    public static partial class UpdateChecks
    {
        public static class Github 
        {
            public static Dictionary<ModuleMetadata, string> Tags = new Dictionary<ModuleMetadata, string>();
            public static HashSet<ModuleMetadata> MetadataGHUpdates = new HashSet<ModuleMetadata>();
            private static readonly Lock syncRoot = new Lock();

            public static async Task<Result<bool, string>> CheckModUpdate(ModuleMetadata metadata)
            {
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
                            .Where(x => regex.IsMatch(UpdateChecks.TrimUrl(x.Ref)))
                            .Select(x => {
                                x.Version = new SemanticVersion(regex.Match(new string(UpdateChecks.TrimUrl(x.Ref))).Groups[1].Value);
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
                        MetadataGHUpdates.Add(metadata);
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

                    if (!UpdateChecks.ValidateReleaseByte(data, out SemanticVersion version))
                    {
                        return "First release does not have a valid mod metadata.";
                    }

                    string updatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModUpdater", firstAsset.Name);

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

            public static void OpenGithubModUpdateURL(ModuleMetadata metadata, string repo)
            {
                var releaseTag = Tags[metadata];
                UpdateChecks.OpenURL($"https://github.com/{repo}/releases/tag/{releaseTag}");
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
}
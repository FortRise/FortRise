using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using FortLauncher;

namespace FortRise.GameBanana;

internal sealed class GameBanana(string url)
{
    private string url = url;
    private bool opened = false;

    public void Parse(bool autoAccept)
    {
        if (!opened)
        {
            Console.WriteLine("You are using an experimental way to download mod, but it is currently disabled");
            opened = false;
            return;
        }

        url = url.Replace("fortrise:", "");
        string[] splits = url.Split(',');

        if (splits.Length <= 0)
        {
            return;
        }

        string modType;
        string modID;

        string urlToArchive = splits[0];
        if (splits.Length <= 1)
        {
            return;
        }

        if (splits.Length <= 2)
        {
            return;
        }

        modType = splits[1].Trim();
        modID = splits[2].Trim();

        if (IsModFileExists(modID, urlToArchive, out string? filename) || filename is null)
        {
            Console.WriteLine($"File: '{filename}' does already exist on Mods folder.");
            return;
        }

        if (!autoAccept)
        {
            Console.WriteLine($"Do you want to install '{filename}'? [Y/n]");
            string? line = Console.ReadLine();

            if (line is null)
            {
                return;
            }

            if (!line.StartsWith("y", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
        }

        using var fs = File.Open(
            Path.Combine("Mods", filename), 
            FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
        
        long position = 0;

        using (HttpClient client = new CompressedHttpClient())
        {
            HttpResponseMessage response = client.GetAsync(
                urlToArchive, HttpCompletionOption.ResponseHeadersRead).Result;

            using (Stream input = response.Content.ReadAsStream())
            {
                long length = 0;
                if (response.Content.Headers.TryGetValues("Content-Length", out var headers))
                {
                    length = long.Parse(headers.First());
                }

                if (length == 0)
                {
                    HttpResponseMessage head = client.Send(
                        new HttpRequestMessage(HttpMethod.Head, urlToArchive));

                    if (head.Content.Headers.TryGetValues("Content-Length", out headers))
                    {
                        length = long.Parse(headers.First());
                    }
                }

                DateTime timeLast = DateTime.Now;

                byte[] buffer = new byte[4096];
                int read = 0;
                int rread = 0;
                int speed = 0;
                int count = 0;

                do 
                {
                    count = buffer.Length;
                    read = input.Read(buffer, 0, count);
                    fs.Write(buffer, 0, read);
                    position += read;
                    rread += read;

                    Console.WriteLine(position);

                    TimeSpan td = DateTime.Now - timeLast;
                    if (td.TotalMilliseconds > 100)
                    {
                        speed = (int)(rread / 1024D / td.TotalSeconds);
                        rread = 0;
                        timeLast = DateTime.Now;
                    }

                    if (length > 0)
                    {
                        int progress = (int)Math.Floor(
                            100D * Math.Min(1D, position / (double)length));
                        Console.WriteLine($"Downloading: {progress}% @ {speed} KiB/s {position}/{length} B");
                    }
                    else 
                    {
                        Console.WriteLine(
                            $"Downloading {Math.Floor(position / 1000D)}% @ {speed} KiB/s {position}/{length} B");
                    }
                    
                }
                while (read > 0);
            }
        }

        Console.WriteLine($"Downloaded {position} bytes.");
        // TODO: Check the file with Checksums
    }

    private static string GetDownloadMirror(int id)
    {
        return $"https://gamebanana.com/dl/{id}";
    }

    private static bool IsModFileExists(string modID, string url, out string? file)
    {
        //TODO: have the Dictionary structured
        string json;

        try 
        {
            string itemUrl = $"https://api.gamebanana.com/Core/Item/Data?itemtype=Mod&itemid={modID}&fields=name%2CFiles().aFiles()";

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", "FortRise/" + FortRiseConstants.Version);
            json = client.GetStringAsync(itemUrl).Result;
        }
        catch (Exception)
        {
            file = null;
            return false;
        }

        int index = url.LastIndexOf('/') + 1;

        string downloadID = url[index..];


        var list = JsonSerializer.Deserialize<List<JsonElement>>(json)!;
        string name = list[0]!.ToString()!;
        Console.WriteLine($"Validating {name} if it can be installed.");

        foreach (var enu in list[1].EnumerateObject())
        {
            if (enu.Name == downloadID)
            {
                foreach (var updObj in enu.Value.EnumerateObject())
                {
                    Console.WriteLine(updObj.Name);
                    if (updObj.Name == "_sFile")
                    {
                        file = updObj.Value.GetString()!;
                        var f = Path.Combine("Mods", file);

                        if (File.Exists(f))
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }
        }

        file = null;
        return false;
    }
}


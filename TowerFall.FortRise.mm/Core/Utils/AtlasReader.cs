using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;
using Monocle;

namespace FortRise;

public static class AtlasReader 
{
    internal static Dictionary<string, Type> InternalReaders = new();
    public static IReadOnlyDictionary<string, Type> Readers => InternalReaders;

    public static void Initialize() 
    {
        Register<XmlAtlasReader>(".xml");
        Register<JsonAtlasReader>(".json");
    }

    public static void Register<Reader>(string ext) 
    where Reader : IAtlasReader
    {
        if (InternalReaders.ContainsKey(ext))
        {
            Logger.Warning($"[Atlas Reader] A reader for '{ext}' has already been existed!");
            return;
        }

        InternalReaders.Add(ext, typeof(Reader));
    }

    public static patch_Atlas Read(Stream stream, string ext) 
    {
        if (InternalReaders.TryGetValue(ext, out var reader)) 
        {
            IAtlasReader instance = (IAtlasReader)Activator.CreateInstance(reader);
            return instance.Read(stream);
        }
        Logger.Error($"[Atlas Reader] An reader for {ext} does not existed! Falling back to xml");
        return new XmlAtlasReader().Read(stream);
    }

    public static patch_Atlas Read(string path) 
    {
        var ext = Path.GetExtension(path);
        using var stream = File.OpenRead(path);
        if (InternalReaders.TryGetValue(ext, out var reader)) 
        {
            IAtlasReader instance = (IAtlasReader)Activator.CreateInstance(reader);
            return instance.Read(stream);
        }
        Logger.Error($"[Atlas Reader] An reader for {ext} does not existed! Falling back to xml");
        return new XmlAtlasReader().Read(stream);
    }
}

public class XmlAtlasReader : IAtlasReader
{
    public patch_Atlas Read(Stream stream)
    {
        XmlNodeList elementsByTagName = patch_Calc.LoadXML(stream)["TextureAtlas"].GetElementsByTagName("SubTexture");
        var atlas = new patch_Atlas();
        atlas.SetSubTextures(new Dictionary<string, Subtexture>(elementsByTagName.Count));
        foreach (XmlElement item in elementsByTagName)
        {
            XmlAttributeCollection attributes = item.Attributes;
            atlas.SubTextures.Add(
                attributes["name"].Value, 
                new Subtexture(atlas, 
                Convert.ToInt32(attributes["x"].Value), 
                Convert.ToInt32(attributes["y"].Value), 
                Convert.ToInt32(attributes["width"].Value), 
                Convert.ToInt32(attributes["height"].Value))
            );
        }
        return atlas;
    }
}

public class JsonAtlasReader : IAtlasReader
{
    public patch_Atlas Read(Stream stream)
    {
        var atlas = new patch_Atlas();
        atlas.SetSubTextures(new Dictionary<string, Subtexture>(20));

        var json = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, JsonElement>>>>(stream);
        var frames = json["frames"];
        foreach (var frame in frames) 
        {
            var val = frame.Value;
            atlas.SubTextures.Add(
                frame.Key,
                new Subtexture(atlas, val["x"].GetInt32(), val["y"].GetInt32(), val["width"].GetInt32(), val["height"].GetInt32())
            );
        }
        return atlas;
    }
}

public interface IAtlasReader 
{
    patch_Atlas Read(Stream stream);
}
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise.ContentPatcher;

internal sealed class LevelContext : IFortRiseContentPatcherAPI.ILevelContext
{
    private const int TileSize = 10;

    private readonly CurrentLevel info;

    public LevelContext(CurrentLevel info)
    {
        this.info = info;
        Level = info.Xml;
    }

    public XmlElement Level { get; }
    public LevelSystem LevelSystem => info.LevelSystem;
    public TowerTheme Theme => info.Theme;
    public string? LevelPath => info.LevelPath;
    public string? Filename => info.Filename;
    public Modes Mode => info.Mode;
    public string ModeName => info.ModeName;

    public int Width => ReadIntAttribute("width", 320);
    public int Height => ReadIntAttribute("height", 240);
    public int Columns => Width / TileSize;
    public int Rows => Height / TileSize;

    public bool IsTower(string idOrName) => info.IsTower(idOrName);

    public string GetAttribute(string name) => Level.GetAttribute(name);

    public void SetAttribute(string name, string value) => Level.SetAttribute(name, value);

    public XmlElement? GetLayer(string name) => Level[name];

    public XmlElement GetOrCreateLayer(string name)
    {
        var element = Level[name];
        if (element is null)
        {
            element = Level.OwnerDocument.CreateElement(name);
            Level.AppendChild(element);
        }
        return element;
    }

    public void RemoveLayer(string name)
    {
        if (Level[name] is { } element)
        {
            Level.RemoveChild(element);
        }
    }

    public string? GetLayerText(string name) => Level[name]?.InnerText;

    public void SetLayerText(string name, string text) => GetOrCreateLayer(name).InnerText = text;

    public bool[,] GetBitGrid(string layer)
    {
        return Calc.GetBitData(GetLayerText(layer) ?? string.Empty, Columns, Rows);
    }

    public void SetBitGrid(string layer, bool[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        var sb = new StringBuilder(width * height + height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(grid[x, y] ? '1' : '0');
            }
            if (y < height - 1)
            {
                sb.Append('\n');
            }
        }
        SetLayerText(layer, sb.ToString());
    }

    public int[,] GetTileGrid(string layer)
    {
        return Calc.ReadCSVIntGrid(GetLayerText(layer) ?? string.Empty, Columns, Rows);
    }

    public void SetTileGrid(string layer, int[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        var sb = new StringBuilder();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x > 0)
                {
                    sb.Append(',');
                }
                sb.Append(grid[x, y].ToString(CultureInfo.InvariantCulture));
            }
            if (y < height - 1)
            {
                sb.Append('\n');
            }
        }
        SetLayerText(layer, sb.ToString());
    }

    public IReadOnlyList<XmlElement> GetEntities(string layer)
    {
        var result = new List<XmlElement>();
        if (Level[layer] is { } element)
        {
            foreach (XmlNode node in element.ChildNodes)
            {
                if (node is XmlElement e)
                {
                    result.Add(e);
                }
            }
        }
        return result;
    }

    public IReadOnlyList<XmlElement> GetEntities(string layer, string name)
    {
        var result = new List<XmlElement>();
        foreach (var e in GetEntities(layer))
        {
            if (e.Name == name)
            {
                result.Add(e);
            }
        }
        return result;
    }

    public XmlElement? GetEntityByID(string layer, string id)
    {
        foreach (var e in GetEntities(layer))
        {
            if (e.GetAttribute("id") == id)
            {
                return e;
            }
        }
        return null;
    }

    public XmlElement AddEntity(string layer, string name, IReadOnlyDictionary<string, string>? attributes = null)
    {
        var container = GetOrCreateLayer(layer);
        var entity = Level.OwnerDocument.CreateElement(name);
        if (attributes is null || !attributes.ContainsKey("id"))
        {
            entity.SetAttribute("id", NextEntityID(layer).ToString(CultureInfo.InvariantCulture));
        }
        if (attributes is not null)
        {
            foreach (var (key, value) in attributes)
            {
                entity.SetAttribute(key, value);
            }
        }
        container.AppendChild(entity);
        return entity;
    }

    public bool RemoveEntityByID(string layer, string id)
    {
        if (GetEntityByID(layer, id) is not { } entity)
        {
            return false;
        }
        entity.ParentNode!.RemoveChild(entity);
        return true;
    }

    public int RemoveEntities(string layer, string name)
    {
        var entities = GetEntities(layer, name);
        foreach (var e in entities)
        {
            e.ParentNode!.RemoveChild(e);
        }
        return entities.Count;
    }

    public int NextEntityID(string layer)
    {
        int max = -1;
        foreach (var e in GetEntities(layer))
        {
            if (int.TryParse(e.GetAttribute("id"), out int id) && id > max)
            {
                max = id;
            }
        }
        return max + 1;
    }

    private int ReadIntAttribute(string name, int fallback)
    {
        return int.TryParse(Level.GetAttribute(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? value
            : fallback;
    }
}

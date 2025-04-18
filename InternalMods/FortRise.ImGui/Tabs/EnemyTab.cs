using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Monocle;
using TowerFall;

namespace FortRise.ImGuiLib;

internal sealed class EnemyTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "Enemies";
    private bool dirty;
    private bool arrowDirty;
    private string searchBar = "";
    private int currentEnemy;
    private string[] cachedEnemies = null!;

    private bool hasWings;
    private bool hasShield;
    private bool isJester;
    private bool isBoss;
    private bool isMimic;
    private ArrowDB[]? arrowsCache;
    private int currentArrow;
    private readonly string[] vanillaArrows = [
        "Normal",
        "Bomb",
        "Bolt",
        "SuperBomb",
        "Bramble",
        "Drill",
        "Feather",
        "Laser",
        "Prism",
        "Toy",
        "Trigger"
    ];

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        var enemies = GetAllEnemies();        
        string[] arr;

        if (string.IsNullOrEmpty(searchBar))
        {
            arr = enemies;
        }
        else 
        {
            arr = [.. enemies.Where(x => x.Contains(searchBar, StringComparison.OrdinalIgnoreCase))];
        }

        ImGui.SeparatorText("Skeletons");

        ImGui.Checkbox("Wing", ref hasWings);
        ImGui.SameLine();
        ImGui.Checkbox("Shield", ref hasShield);
        ImGui.SameLine();
        ImGui.Checkbox("Jester", ref isJester);
        ImGui.Checkbox("Mimic", ref isMimic);
        ImGui.Checkbox("Boss", ref isBoss);

        var arrows = GetArrowList();
        string[] arrStr = [.. arrows.Select(x => x.Name)];

        ImGui.Combo("Arrows", ref currentArrow, arrStr, arrStr.Length);
        string skeleton = "";
        if (hasWings)
        {
            skeleton += "Wing";
        }
        if (isJester)
        {
            skeleton += "Jester";
        }
        if (isMimic)
        {
            skeleton += "Mimic";
        }
        if (isBoss)
        {
            skeleton += "Boss";
        }

        var arrowName = arrStr[currentArrow];

        bool vanillaContains = vanillaArrows.Contains(arrowName);

        if (arrowName != "Normal" && vanillaContains)
        {
            skeleton += arrowName;
        }

        skeleton += "Skeleton";

        if (hasShield)
        {
            skeleton += "S";
        }

        if (!vanillaContains)
        {
            skeleton += ":" + arrowName;
        }

        ImGui.LabelText("Result: ", skeleton);

        if (Engine.Instance.Scene is Level level)
        {			
            if (ImGui.Button("Summon"))
            {
                var portals = level.Layers[0].GetList<QuestSpawnPortal>();
                if (portals.Count == 0) 
                {
                    return;
                }
                portals.Shuffle(); 
                portals[0].AppearAndSpawn(skeleton);
            }
        }

        ImGui.SeparatorText("Enemies");

        ImGui.InputText("Search", ref searchBar, 100);

        if (ImGui.ListBox("Enemies", ref currentEnemy, arr, arr.Length, enemies.Length))
        {
            if (Engine.Instance.Scene is Level level2)
            {			
                var portals = level2.Layers[0].GetList<QuestSpawnPortal>();
                if (portals.Count == 0) 
                {
                    return;
                }
                portals.Shuffle(); 
                portals[0].AppearAndSpawn(arr[currentEnemy]);
            }
        }
    }

    private string[] GetAllEnemies()
    {
        if (dirty)
        {
            return cachedEnemies;
        }
        dirty = true;
        List<string> enemies = [
            "Worm",
            "Mole",
            "TechnoMage",
            "FlamingSkull",
            "Exploder",
            "Birdman",
            "DarkBirdman",
            "EvilCrystal",
            "BlueCrystal",
            "BoltCrystal",
            "PrismCrystal", 
            "Ghost", 
            "GreenGhost", 
            "Elemental", 
            "GreenElemental", 
            "Slime", 
            "RedSlime", 
            "BlueSlime", 
            "Bat", 
            "BombBat", 
            "SuperBombBat", 
            "Crow", 
            "Cultist", 
            "ScytheCultist", 
            "BossCultist",
        ]; 


        enemies.AddRange(EntityRegistry.EnemyLoader.Select(x => x.Key));


        return cachedEnemies = [.. enemies];
    }

    private ArrowDB[] GetArrowList()
    {
        if (!arrowDirty)
        {
            int len = 11 + ArrowsRegistry.ArrowDatas.Count;
            arrowsCache = new ArrowDB[len];

            for (int i = 0; i < len; i++)
            {
                var type = (ArrowTypes)i;
                arrowsCache[i] = new ArrowDB(type, ArrowTypesToName(type));
            }
            arrowDirty = true;
        }
        return arrowsCache!;
    }

    private record struct ArrowDB(ArrowTypes Type, string Name);

    private static string ArrowTypesToName(ArrowTypes type)
    {
        return type switch
        {
            ArrowTypes.Normal => nameof(ArrowTypes.Normal),
            ArrowTypes.Bomb => nameof(ArrowTypes.Bomb),
            ArrowTypes.Bolt => nameof(ArrowTypes.Bolt),
            ArrowTypes.SuperBomb => nameof(ArrowTypes.SuperBomb),
            ArrowTypes.Bramble => nameof(ArrowTypes.Bramble),
            ArrowTypes.Drill => nameof(ArrowTypes.Drill),
            ArrowTypes.Feather => nameof(ArrowTypes.Feather),
            ArrowTypes.Laser => nameof(ArrowTypes.Laser),
            ArrowTypes.Prism => nameof(ArrowTypes.Prism),
            ArrowTypes.Toy => nameof(ArrowTypes.Toy),
            ArrowTypes.Trigger => nameof(ArrowTypes.Trigger),
            _ => ArrowsRegistry.ArrowDatas[type].Name,
        };
    }
}
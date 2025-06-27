#nullable enable
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TowerFall;

namespace FortRise;

internal static class SpriteRegistry
{
    private static Dictionary<string, IBaseSpriteContainerEntry> spriteEntries = new();

    public static void AddSprite(IBaseSpriteContainerEntry entry)
    {
        spriteEntries[entry.Entry.ID] = entry;
    }

    public static ISpriteContainerEntry? GetSpriteEntry<T>(string id)
    {
        if (spriteEntries.TryGetValue(id, out IBaseSpriteContainerEntry? value))
        {
            return (ISpriteContainerEntry?)value;
        }

        return (ISpriteContainerEntry?)CreateVanillaEntry<T>(id, ContainerSpriteType.Main);
    }

    public static IMenuSpriteContainerEntry? GetMenuSpriteEntry<T>(string id)
    {
        if (spriteEntries.TryGetValue(id, out IBaseSpriteContainerEntry? value))
        {
            return (IMenuSpriteContainerEntry?)value;
        }

        return (IMenuSpriteContainerEntry?)CreateVanillaEntry<T>(id, ContainerSpriteType.Menu);
    }

    public static ICorpseSpriteContainerEntry? GetCorpseSpriteEntry<T>(string id)
    {
        if (spriteEntries.TryGetValue(id, out IBaseSpriteContainerEntry? value))
        {
            return (ICorpseSpriteContainerEntry?)value;
        }

        return (ICorpseSpriteContainerEntry?)CreateVanillaEntry<T>(id, ContainerSpriteType.Corpse);
    }

    public static IBGSpriteContainerEntry? GetBGSpriteEntry<T>(string id)
    {
        if (spriteEntries.TryGetValue(id, out IBaseSpriteContainerEntry? value))
        {
            return (IBGSpriteContainerEntry?)value;
        }

        return (IBGSpriteContainerEntry?)CreateVanillaEntry<T>(id, ContainerSpriteType.BG);
    }

    public static IBossSpriteContainerEntry? GetBossSpriteEntry<T>(string id)
    {
        if (spriteEntries.TryGetValue(id, out IBaseSpriteContainerEntry? value))
        {
            return (IBossSpriteContainerEntry?)value;
        }

        return (IBossSpriteContainerEntry?)CreateVanillaEntry<T>(id, ContainerSpriteType.Boss);
    }


    private static Dictionary<string, IBaseSpriteContainerEntry> vanillaCaches = new Dictionary<string, IBaseSpriteContainerEntry>();
    private static IBaseSpriteContainerEntry CreateVanillaEntry<T>(string id, ContainerSpriteType spriteType)
    {
        ref var vanilla = ref CollectionsMarshal.GetValueRefOrAddDefault(vanillaCaches, id, out bool exists);
        if (exists)
        {
            return vanilla!;
        }

        IBaseSpriteContainerEntry entry;

        switch (spriteType)
        {
            case ContainerSpriteType.Boss:
                entry = new SpriteContainerEntry(new SpriteEntry<T>(id, () => TFGame.BossSpriteData.GetXML(id)), spriteType);
                break;
            case ContainerSpriteType.BG:
                entry = new SpriteContainerEntry(new SpriteEntry<T>(id, () => TFGame.BGSpriteData.GetXML(id)), spriteType);
                break;
            case ContainerSpriteType.Menu:
                entry = new SpriteContainerEntry(new SpriteEntry<T>(id, () => TFGame.MenuSpriteData.GetXML(id)), spriteType);
                break;
            case ContainerSpriteType.Corpse:
                entry = new SpriteContainerEntry(new SpriteEntry<T>(id, () => TFGame.CorpseSpriteData.GetXML(id)), spriteType);
                break;
            default:
                entry = new SpriteContainerEntry(new SpriteEntry<T>(id, () => TFGame.SpriteData.GetXML(id)), spriteType);
                break;
        }

        vanillaCaches[id] = entry;
        return entry;
    }
}
#nullable enable
using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface IModArchers
{
    IReadOnlyDictionary<string, IArcherEntry> RegisteredArchers { get; }

    IArcherEntry RegisterArcher(string id, ArcherConfiguration configuration);
    IArcherEntry? GetArcher(string id);
    IReadOnlyList<IArcherEntry>? GetAllArchers();
}

internal sealed class ModArchers : IModArchers
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IArcherEntry> archerQueue;
    // what Invoke put in the arrays, and what was in the slot before (for alt/secret archers)
    private readonly Dictionary<IArcherEntry, (ArcherData Data, ArcherData? Previous)> injected = [];

    public IReadOnlyDictionary<string, IArcherEntry> RegisteredArchers => ArcherRegistry.ArcherEntries;

    internal ModArchers(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        archerQueue = manager.CreateQueue<IArcherEntry>(metadata, Invoke, Detach);
    }

    public IArcherEntry RegisterArcher(string id, ArcherConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        int idObtained;
        ArcherEntryType archerEntryType;

        if (configuration.AltFor != null)
        {
            if (configuration.SecretFor != null)
            {
                throw new Exception($"[{metadata.Name}] Alt Archers cannot be Secret Archers at the same time.");
            }

            idObtained = configuration.AltFor.Index;
            archerEntryType = ArcherEntryType.Alt;
        }
        else if (configuration.SecretFor != null)
        {
            idObtained = configuration.SecretFor.Index;
            archerEntryType = ArcherEntryType.Secret;
        }
        else
        {
            idObtained = IDPool.Obtain("archers");
            archerEntryType = ArcherEntryType.Normal;
        }
        IArcherEntry entry = new ArcherEntry(name, configuration, idObtained, archerEntryType);
        ArcherRegistry.AddArcher(entry);
        archerQueue.AddOrInvoke(entry);
        return entry;
    }

    public IArcherEntry? GetArcher(string id)
    {
        return ArcherRegistry.GetArcherEntry(id);
    }

    private void Invoke(IArcherEntry entry)
    {
        if (entry.Type == ArcherEntryType.Normal)
        {
            ArcherStatsSync.BeforeSlotsChange();
        }

        // resize this ahead of time
        int archerCount = ArcherData.Archers.Length;
        int archerNewCount = entry.Index + 1;
        // only grow: alt/secret archers or a reused slot can point below the current length
        if (archerNewCount > archerCount)
        {
            var archers = ArcherData.Archers;
            var altArchers = ArcherData.AltArchers;
            var secretArchers = ArcherData.SecretArchers;
            Array.Resize(ref archers, archerNewCount);
            Array.Resize(ref altArchers, archerNewCount);
            Array.Resize(ref secretArchers, archerNewCount);
            patch_ArcherData.Archers = archers;
            patch_ArcherData.AltArchers = altArchers;
            patch_ArcherData.SecretArchers = secretArchers;
        }

        ArcherData.HatInfo hat = default;
        if (entry.Configuration.Hat.TryGetValue(out HatInfo info))
        {
            hat.Material = info.Material;
            hat.Normal = info.Normal?.Subtexture;
            hat.Blue = info.Blue?.Subtexture;
            hat.Red = info.Red?.Subtexture;
        }

        ArcherData.PortraitInfo portrait = default;
        portrait.Win = entry.Configuration.Portraits.Win.Subtexture;
        portrait.Lose = entry.Configuration.Portraits.Lose.Subtexture;
        portrait.NotJoined = entry.Configuration.Portraits.NotJoined.Subtexture;
        portrait.Joined = entry.Configuration.Portraits.Joined.Subtexture;

        ArcherData.StatueInfo statue = default;
        statue.Glow = entry.Configuration.Statue.Glow.Subtexture;
        statue.Image = entry.Configuration.Statue.Image.Subtexture;

        ArcherData.BreathingInfo breathing = default;
        if (entry.Configuration.Breathing.TryGetValue(out var breathe))
        {
            breathing.DuckingOffset = breathe.DuckingOffset;
            breathing.Offset = breathe.Offset;
            breathing.Interval = breathe.Interval;
        }
        else
        {
            breathing.Interval = -1;
        }

        var archerData = new patch_ArcherData()
        {
            Name0 = entry.Configuration.TopName,
            Name1 = entry.Configuration.BottomName,
            ColorA = entry.Configuration.ColorA,
            ColorB = entry.Configuration.ColorB,
            LightbarColor = entry.Configuration.LightbarColor,
            Aimer = entry.Configuration.Aimer.Subtexture,
            Corpse = entry.Configuration.CorpseSprite.Entry.ID,
            StartNoHat = entry.Configuration.StartNoHat,
            VictoryMusic = entry.Configuration.VictoryMusic,
            PurpleParticles = entry.Configuration.PurpleParticles,
            SleepHeadFrame = -1,
            SFXID = entry.Configuration.SFX,
            Gender = entry.Configuration.Gender,
            Sprites = new ArcherData.SpriteInfo()
            {
                Body = entry.Configuration.Sprites.Body.Entry.ID,
                HeadNormal = entry.Configuration.Sprites.HeadNormal.Entry.ID,
                HeadNoHat = entry.Configuration.Sprites.HeadNoHat.Entry.ID,
                HeadCrown = entry.Configuration.Sprites.HeadCrown.Entry.ID,
                HeadBack = entry.Configuration.Sprites.HeadBack?.Entry.ID ?? "",
                Bow = entry.Configuration.Sprites.Bow.Entry.ID,
            },
            Hat = hat,
            Portraits = portrait,
            Gems = new ArcherData.GemInfo()
            {
                Gameplay = entry.Configuration.Gems.Gameplay.Entry.ID,
                Menu = entry.Configuration.Gems.Menu.Entry.ID
            },
            Statue = statue,
            Breathing = breathing,
            Hair = entry.Configuration.Hair.HasValue,
            ExtraHairData = entry.Configuration.Hair,
            Particle = entry.Configuration.Particles.HasValue ?
                entry.Configuration.Particles.Value.Name : null
        };

        ArcherData? previous = null;
        switch (entry.Type)
        {
            case ArcherEntryType.Normal:
                ArcherData.Archers[entry.Index] = archerData;
                break;
            case ArcherEntryType.Alt:
                previous = ArcherData.AltArchers[entry.Index];
                ArcherData.AltArchers[entry.Index] = archerData;
                break;
            case ArcherEntryType.Secret:
                previous = ArcherData.SecretArchers[entry.Index];
                ArcherData.SecretArchers[entry.Index] = archerData;
                break;
        }

        injected[entry] = (archerData, previous);

        if (entry.Type == ArcherEntryType.Normal)
        {
            ArcherStatsSync.AfterSlotsChange();
        }

        RequestParticleRefresh();
    }

    private static void RequestParticleRefresh()
    {
        ModuleManager.Instance.RequestRefresh("particles", TowerFall.Particles.Initialize);
    }

    private void Detach(IArcherEntry entry)
    {
        // back up while the entry is still registered and every archer is in its old slot
        if (entry.Type == ArcherEntryType.Normal)
        {
            ArcherStatsSync.BeforeSlotsChange();
        }

        RequestParticleRefresh();
        ArcherRegistry.RemoveArcher(entry);

        if (!injected.Remove(entry, out var slot))
        {
            return;
        }

        switch (entry.Type)
        {
            case ArcherEntryType.Normal:
                RemoveArcherSlot(Array.IndexOf(ArcherData.Archers, slot.Data));
                break;
            case ArcherEntryType.Alt:
            {
                int index = Array.IndexOf(ArcherData.AltArchers, slot.Data);
                if (index >= 0)
                {
                    ArcherData.AltArchers[index] = slot.Previous!;
                }
                break;
            }
            case ArcherEntryType.Secret:
            {
                int index = Array.IndexOf(ArcherData.SecretArchers, slot.Data);
                if (index >= 0)
                {
                    ArcherData.SecretArchers[index] = slot.Previous!;
                }
                break;
            }
        }
    }

    private static void RemoveArcherSlot(int index)
    {
        if (index < 0)
        {
            return;
        }

        patch_ArcherData.Archers = RemoveAt(ArcherData.Archers, index);
        patch_ArcherData.AltArchers = RemoveAt(ArcherData.AltArchers, index);
        patch_ArcherData.SecretArchers = RemoveAt(ArcherData.SecretArchers, index);

        ArcherRegistry.ShiftIndicesAfter(index);
        ArcherStatsSync.AfterSlotsChange();

        IDPool.PooledID["archers"] = ArcherData.Archers.Length - 1;
        if (IDPool.Freed.TryGetValue("archers", out var freed))
        {
            freed.Clear();
        }

        var characters = TFGame.Characters;
        for (int i = 0; i < characters.Length; i += 1)
        {
            if (characters[i] == index)
            {
                characters[i] = 0;
            }
            else if (characters[i] > index)
            {
                characters[i] -= 1;
            }
        }

        static ArcherData[] RemoveAt(ArcherData[] array, int at)
        {
            if (at >= array.Length)
            {
                return array;
            }

            var list = new List<ArcherData>(array);
            list.RemoveAt(at);
            return [.. list];
        }
    }

    public IReadOnlyList<IArcherEntry>? GetAllArchers()
    {
        return ArcherRegistry.GetAllArcherEntries();
    }
}

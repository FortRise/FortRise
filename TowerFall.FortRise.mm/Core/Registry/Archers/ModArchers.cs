#nullable enable
using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface IModArchers
{
    IArcherEntry RegisterArcher(string id, ArcherConfiguration configuration);
    IArcherEntry? GetArcher(string id);
}

internal sealed class ModArchers : IModArchers
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IArcherEntry> archerQueue;

    internal ModArchers(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        archerQueue = manager.CreateQueue<IArcherEntry>(Invoke);
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
        // resize this ahead of time
        int archerCount = ArcherData.Archers.Length;
        int archerNewCount = entry.Index + 1;
        if (archerNewCount != archerCount)
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
            ExtraHairData = entry.Configuration.Hair
        };

        switch (entry.Type)
        {
            case ArcherEntryType.Normal:
                ArcherData.Archers[entry.Index] = archerData;
                break;
            case ArcherEntryType.Alt:
                ArcherData.AltArchers[entry.Index] = archerData;
                break;
            case ArcherEntryType.Secret:
                ArcherData.SecretArchers[entry.Index] = archerData;
                break;
        }
    }
}
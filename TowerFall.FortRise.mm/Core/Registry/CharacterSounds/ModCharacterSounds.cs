#nullable enable
using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;

public interface IModCharacterSounds
{
    ICharacterSoundEntry RegisterCharacterSounds(string id, CharacterSoundConfiguration configuration);
}

internal sealed class ModCharacterSounds : IModCharacterSounds
{
    private readonly ModuleMetadata metadata;
    private readonly Dictionary<string, ICharacterSoundEntry> characterSoundEntries = new Dictionary<string, ICharacterSoundEntry>();
    private readonly RegistryQueue<ICharacterSoundEntry> characterSoundQueue;

    internal ModCharacterSounds(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        characterSoundQueue = manager.CreateQueue<ICharacterSoundEntry>(Invoke);
    }

    public ICharacterSoundEntry RegisterCharacterSounds(string id, CharacterSoundConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        ICharacterSoundEntry sfxEntry = new CharacterSoundEntry(name, IDPool.Obtain("characterSounds"), configuration);
        characterSoundEntries.Add(name, sfxEntry);
        characterSoundQueue.AddOrInvoke(sfxEntry);
        return sfxEntry;
    }

    private void Invoke(ICharacterSoundEntry entry)
    {
        CharacterSoundsRegistry.ModdedSounds.Add(new patch_CharacterSounds()
        {
            Ready = (SFXVaried)(object)entry.Configuration.Ready.SFXVaried!,
            Deselect = entry.Configuration.Deselect.BaseSFX,
            Aim = entry.Configuration.Aim.BaseSFX,
            AimCancel = entry.Configuration.AimCancel.BaseSFX,
            AimDir = entry.Configuration.AimDir.BaseSFX,
            Die = entry.Configuration.Die.BaseSFX,
            DieBomb = entry.Configuration.DieBomb.BaseSFX,
            DieLaser = entry.Configuration.DieLaser.BaseSFX,
            DieStomp = entry.Configuration.DieStomp.BaseSFX,
            DieEnv = entry.Configuration.DieEnv.BaseSFX,
            Duck = entry.Configuration.Duck.BaseSFX,
            FireArrow = entry.Configuration.FireArrow.BaseSFX,
            Grab = entry.Configuration.Grab.BaseSFX,
            Jump = entry.Configuration.Jump.BaseSFX,
            Land = entry.Configuration.Land.BaseSFX,
            NoFire = entry.Configuration.NoFire.BaseSFX,
            WallSlide = (SFXLooped)(object)entry.Configuration.WallSlide.SFXLooped!,
            ArrowSteal = entry.Configuration.ArrowSteal.BaseSFX,
            ArrowGrab = entry.Configuration.ArrowGrab.BaseSFX,
            ArrowRecover = entry.Configuration.ArrowRecover.BaseSFX,
            Sleep = (SFXLooped)(object)entry.Configuration.Sleep?.SFXLooped!,
            Revive = entry.Configuration.Revive.BaseSFX,
        });
    }
}

internal static class CharacterSoundsRegistry
{
    public static List<CharacterSounds> ModdedSounds = new List<CharacterSounds>();
}
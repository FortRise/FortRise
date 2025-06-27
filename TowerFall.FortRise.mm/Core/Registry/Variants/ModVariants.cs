#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using TowerFall;

namespace FortRise;

public interface IModVariants
{
    IVariantEntry RegisterVariant(string id, in VariantConfiguration configuration);
    IVariantPresetEntry RegisterPreset(string id, in PresetConfiguration configuration);
    IVariantEntry? GetVariant(string id);
    IReadOnlyDictionary<string, IVariantEntry> GetAllVariants();
    IReadOnlyList<IVariantPresetEntry> GetAllVariantPresets();
}

internal sealed class ModVariants : IModVariants
{
    private readonly Dictionary<string, IVariantEntry> variantEntries = new Dictionary<string, IVariantEntry>();
    private readonly Dictionary<string, IVariantPresetEntry> presetEntries = new Dictionary<string, IVariantPresetEntry>();
    private readonly ModuleMetadata metadata;
    private static Dictionary<string, IVariantEntry> vanillaCache = new();

    internal ModVariants(ModuleMetadata metadata)
    {
        this.metadata = metadata;
    }

    public IVariantEntry RegisterVariant(string id, in VariantConfiguration configuration)
    {
        var name = $"{metadata.Name}/{id}";

        string? header = configuration.Header;

        if (header is null)
        {
            header = metadata.Name;
        }

        IVariantEntry variant = new VariantEntry(name, configuration with { Header = header });
        variantEntries.Add(name, variant);
        VariantRegistry.Register(variant);
        return variant;
    }

    public IVariantPresetEntry RegisterPreset(string id, in PresetConfiguration configuration)
    {
        var name = $"{metadata.Name}/{id}";

        IVariantPresetEntry preset = new VariantPresetEntry(name, configuration);
        presetEntries.Add(name, preset);
        PresetRegistry.Register(preset);
        return preset;
    }

    public IVariantEntry? GetVariant(string id)
    {
        ReadOnlySpan<char> name = $"{metadata.Name}/{id}";
        var alternate = variantEntries.GetAlternateLookup<ReadOnlySpan<char>>();
        if (alternate.TryGetValue(name, out IVariantEntry? value))
        {
            return value;
        }

        var field = typeof(MatchVariants).GetField(id);
        if (field is null || field.FieldType != typeof(Variant))
        {
            return null;
        }

        var variant = CreateVanillaEntry(field);
        return variant;
    }

    private static IVariantEntry CreateVanillaEntry(FieldInfo info)
    {
        ref var cache = ref CollectionsMarshal.GetValueRefOrAddDefault(vanillaCache, info.Name, out bool exists);
        if (exists)
        {
            return cache!;
        }
        string variantTitle = GetVariantTitle(info);

        var customAttribs = info.GetCustomAttributes();
        Pickups[]? exclusions = null;
        string? description = null;
        string? header = null;
        CustomVariantFlags flags = CustomVariantFlags.None;

        foreach (Attribute attrib in customAttribs)
        {
            switch (attrib)
            {
                case Exclusions exc:
                    exclusions = exc.ItemExclusions;
                    break;
                case Description desc:
                    description = desc.Text;
                    break;
                case Header head:
                    header = head.Title;
                    break;
                case CanRandom:
                    flags |= CustomVariantFlags.CanRandom;
                    break;
                case TournamentRule1v1:
                    flags |= CustomVariantFlags.TournamentRule1v1;
                    break;
                case TournamentRule2v2:
                    flags |= CustomVariantFlags.TournamentRule2v2;
                    break;
                case Unlisted:
                    flags |= CustomVariantFlags.Unlisted;
                    break;
                case DarkWorldDLC:
                    flags |= CustomVariantFlags.DarkWorldDLC;
                    break;
                case CoOp coOp:
                    if (coOp.Value == -1)
                    {
                        flags |= CustomVariantFlags.CoopCurses;
                    }
                    else
                    {
                        flags |= CustomVariantFlags.CoopBlessing;
                    }
                    break;
            }
        }

        var variantConfiguration = new VariantConfiguration()
        {
            Title = variantTitle,
            Icon = new SubtextureEntry(null!, () => MatchVariants.GetVariantIconFromName(info.Name), SubtextureAtlasDestination.MenuAtlas),
            Exclusions = exclusions,
            Description = description,
            Header = header,
            Flags = flags
        };

        var variant = new VariantEntry(info.Name, variantConfiguration);
        return cache = variant;
    }

    private static string GetVariantTitle(FieldInfo field)
    {
        string text = field.Name;
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
            {
                text = string.Concat(text.AsSpan(0, i), " ", text.AsSpan(i));
                i++;
            }
        }
        return text.ToUpperInvariant();
    }

    public IReadOnlyDictionary<string, IVariantEntry> GetAllVariants() => VariantRegistry.Variants;
    public IReadOnlyList<IVariantPresetEntry> GetAllVariantPresets() => PresetRegistry.Presets;
}

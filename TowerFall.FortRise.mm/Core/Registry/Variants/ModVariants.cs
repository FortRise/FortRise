#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using Monocle;
using TowerFall;

namespace FortRise;

public class ModVariants
{
    private readonly Dictionary<string, IVariant> entries = new Dictionary<string, IVariant>();
    private readonly RegistryQueue<IVariant> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModVariants(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IVariant>(Invoke);
    }

    public IVariant RegisterVariant(string id, VariantConfiguration configuration) 
    {
        var name = $"{metadata.Name}/{id}";

        string? header = configuration.Header;

        if (header is null)
        {
            header = metadata.Name;
        }

        IVariant variant = new VariantMetadata(name, configuration with { Header = header });
        entries.Add(name, variant);
        registryQueue.AddOrInvoke(variant);
        return variant;
    }

    public IVariant? GetVariant(string id)
    {
        ReadOnlySpan<char> name = $"{metadata.Name}/{id}";
        var alternate = entries.GetAlternateLookup<ReadOnlySpan<char>>();
        if (alternate.TryGetValue(name, out IVariant? value))
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

    private static VariantMetadata CreateVanillaEntry(FieldInfo info)
    {
        string variantTitle = GetVariantTitle(info);
        Subtexture icon = MatchVariants.GetVariantIconFromName(info.Name);

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
                header =  head.Title;
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
            Icon = icon,
            Exclusions = exclusions,
            Description = description,
            Header = header,
            Flags = flags
        };

        var variant = new VariantMetadata(info.Name, variantConfiguration);
        return variant;
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

    internal void Invoke(IVariant variant)
    {
        VariantRegistry.Register(variant);
    }
}

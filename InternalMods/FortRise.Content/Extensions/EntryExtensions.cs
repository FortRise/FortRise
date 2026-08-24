using System;
using System.Xml;
using Monocle;

namespace FortRise.Content;

public static class EntryExtensions
{
    extension(IModSubtextures subtextures)
    {
        public ISubtextureEntry? GetTextureWithRelative(string id, SubtextureAtlasDestination dest)
        {
            var texture = subtextures.GetTexture(ResolveID(id), dest);
            texture ??= subtextures.GetTexture(id, dest);
            return texture;
        }
    }

    extension(IModSprites sprite)
    {
        public ISpriteContainerEntry? GetSpriteEntryWithRelative<T>(string id)
        {
            var sp = sprite.GetSpriteEntry<T>(ResolveID(id));
            sp ??= sprite.GetSpriteEntry<T>(id);
            return sp;
        }

        public IBGSpriteContainerEntry? GetBGSpriteEntryWithRelative<T>(string id)
        {
            var sp = sprite.GetBGSpriteEntry<T>(ResolveID(id));
            sp ??= sprite.GetBGSpriteEntry<T>(id);
            return sp;
        }

        public IMenuSpriteContainerEntry? GetMenuSpriteEntryWithRelative<T>(string id)
        {
            var sp = sprite.GetMenuSpriteEntry<T>(ResolveID(id));
            sp ??= sprite.GetMenuSpriteEntry<T>(id);
            return sp;
        }

        public ICorpseSpriteContainerEntry? GetCorpseSpriteEntryWithRelative<T>(string id)
        {
            var sp = sprite.GetCorpseSpriteEntry<T>(ResolveID(id));
            sp ??= sprite.GetCorpseSpriteEntry<T>(id);
            return sp;
        }
    }

    extension(IModSFXs sfx)
    {
        public ISFXEntry? GetSFXEntryWithRelative(string id)
        {
            var s = sfx.GetSFX(ResolveID(id));
            s ??= sfx.GetSFX(id);
            return s;
        }

        public ISFXInstancedEntry? GetSFXInstancedEntryWithRelative(string id)
        {
            var s = sfx.GetSFXInstanced(ResolveID(id));
            s ??= sfx.GetSFXInstanced(id);
            return s;
        }

        public ISFXLoopedEntry? GetSFXLoopedEntryWithRelative(string id)
        {
            var s = sfx.GetSFXLooped(ResolveID(id));
            s ??= sfx.GetSFXLooped(id);
            return s;
        }

        public ISFXVariedEntry? GetSFXVariedEntryWithRelative(string id)
        {
            var s = sfx.GetSFXVaried(ResolveID(id));
            s ??= sfx.GetSFXVaried(id);
            return s;
        }
    }

    extension(IModMusics music)
    {
        public IMusicEntry? GetMusicWithRelative(string id)
        {
            var m = music.GetMusic(ResolveID(id));
            m ??= music.GetMusic(id);
            return m;
        }
    }

    extension(IModTowers towers)
    {
        public ITowerTypeEntry? GetTowerTypeWithRelative(string id)
        {
            var m = towers.GetTowerType(ResolveID(id));
            m ??= towers.GetTowerType(id);
            return m;
        }
    }

    extension(IModArchers archers)
    {
        public IArcherEntry? GetArcherWithRelative(string id)
        {
            var archer = archers.GetArcher(ResolveID(id));

            archer ??= archers.GetArcher(id);

            return archer;
        }
    }

    extension(XmlElement xml)
    {
        public string ChildTextWithRelative(string childName, string? defaultValue) => ResolveID(xml.ChildText(childName, defaultValue).Trim());
        public string AttrWithRelative(string childName, string? defaultValue) => ResolveID(xml.Attr(childName, defaultValue));
        public string AttrWithRelative(string childName) => ResolveID(xml.Attr(childName));

        public string AttrOrError(string attr)
        {
            try
            {
                return xml.Attr(attr);
            }
            catch
            {
                throw new System.Exception($"'{attr}' attribute is not set for <{xml.Name}> and it is required.");
            }
        }
    }

    public static string ResolveID(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            if (id.StartsWith('@'))
            {
                return id.Replace("@", $"{ContentModule.CurrentModMetadata.Name}/");
            }
        }
        return id;
    }
}

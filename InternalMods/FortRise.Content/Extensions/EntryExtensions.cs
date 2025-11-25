using System.Xml;
using Monocle;

namespace FortRise.Content;

public static class EntryExtensions
{
    extension(IModSubtextures subtextures)
    {
        public ISubtextureEntry? GetTextureWithRelative(string id, SubtextureAtlasDestination dest) 
            => subtextures.GetTexture(ResolveID(id), dest);
    }

    extension(IModSprites sprite)
    {
        public ISpriteContainerEntry? GetSpriteEntryWithRelative<T>(string id) => sprite.GetSpriteEntry<T>(ResolveID(id))!;
        public IMenuSpriteContainerEntry? GetMenuSpriteEntryWithRelative<T>(string id) => sprite.GetMenuSpriteEntry<T>(ResolveID(id))!;
        public ICorpseSpriteContainerEntry? GetCorpseSpriteEntryWithRelative<T>(string id) => sprite.GetCorpseSpriteEntry<T>(ResolveID(id))!;
    }

    extension(IModMusics music)
    {
        public IMusicEntry? GetMusicWithRelative(string id) => music.GetMusic(ResolveID(id));
    }

    extension(IModArchers archers)
    {
        public IArcherEntry? GetArcherWithRelative(string id) => archers.GetArcher(ResolveID(id));
    }

    extension(XmlElement xml)
    {
        public string ChildTextWithRelative(string childName, string? defaultValue) => ResolveID(xml.ChildText(childName, defaultValue).Trim());
        public string AttrWithRelative(string childName, string? defaultValue) => ResolveID(xml.Attr(childName, defaultValue));
        public string AttrWithRelative(string childName) => ResolveID(xml.Attr(childName));
    }

    public static string ResolveID(string id)
    {
        if (!string.IsNullOrEmpty(id) && id.StartsWith('@'))
        {
            return id.Replace("@", $"{ContentModule.CurrentModMetadata.Name}/");
        }
        return id;
    }
}
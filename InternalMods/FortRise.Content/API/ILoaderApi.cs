namespace FortRise.Content;

public partial interface IFortRiseContentApi 
{
    public interface ILoaderAPI
    {
        IContentConfiguration? GetContentConfiguration(ModuleMetadata metadata);

        public interface IContentConfiguration
        {
            ILoaderConfiguration? Loaders { get; set; }
        }

        public interface ILoaderConfiguration
        {
            ILoader? ArcherData { get; set; }
            ILoader? SpriteData { get; set; }
            ILoader? MenuSpriteData { get; set; }
            ILoader? BgSpriteData { get; set; }
            ILoader? BossSpriteData { get; set; }
            ILoader? CorpseSpriteData { get; set; }
            ILoader? Atlas { get; set; }
            ILoader? MenuAtlas { get; set; }
            ILoader? BGAtlas { get; set; }
            ILoader? BossAtlas { get; set; }
        }

        public interface ILoader
        {
            string[]? Path { get; set; }
            bool Enabled { get; set; }
        }
    }
}


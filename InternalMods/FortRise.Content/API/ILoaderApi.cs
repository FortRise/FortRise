namespace FortRise.Content;

public partial interface IFortRiseContentApi 
{
    public interface ILoaderAPI
    {
        IContentConfiguration? GetContentConfiguration(ModuleMetadata metadata);

        public interface IContentConfiguration
        {
            ILoader? GetLoader(string loaderID);
        }

        public interface ILoader 
        {
            string[]? Path { get; }
            bool Enabled { get; }
        }
    }
}


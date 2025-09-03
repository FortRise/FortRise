using System.Collections.Generic;

namespace FortRise.Content;

public partial interface IFortRiseContentApi 
{
    public interface ILoaderAPI
    {
        IContentConfiguration? GetContentConfiguration(ModuleMetadata metadata);

        public interface IContentConfiguration
        {
            IReadOnlyDictionary<string, ILoader> Loaders { get; set; }
        }

        public interface ILoader
        {
            string[]? Path { get; set; }
            bool Enabled { get; set; }
        }
    }
}


using System.Xml;

namespace FortRise.Content;

public partial interface IFortRiseContentApi 
{
    public interface IVersusLoaderAPI
    {
        IVersusTowerEntry RegisterVersusTowerWithXml(
            IModContent content, 
            IModRegistry registry, 
            XmlElement towerXml,
            string levelID,
            string towerSet,
            IResourceInfo[] levels
        );
    }
}




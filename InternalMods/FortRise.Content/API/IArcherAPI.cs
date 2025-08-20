using System.Collections.Generic;
using System.Xml;

namespace FortRise.Content;

public partial interface IFortRiseContentApi 
{
    public interface IArcherAPI 
    {
        IArcherEntry RegisterArcherWithXml(string id, IModContent content, IModRegistry registry, XmlElement xml);
        IList<IArcherEntry> RegisterArchersWithXml(IModContent content, IModRegistry registry, XmlDocument xml);

        IArcherEntry RegisterArcherWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource);

        IList<IArcherEntry> RegisterArchersWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource);
    }
}


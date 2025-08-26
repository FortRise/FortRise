using System;
using System.Collections.Generic;
using System.Xml;
using Monocle;

namespace FortRise.Content;

internal sealed partial class ApiImplementation
{
    internal sealed class ArcherAPI : IFortRiseContentApi.IArcherAPI
    {
        public IArcherEntry RegisterArcherWithXml(string id, IModContent content, IModRegistry registry, XmlElement xml)
        {
            return ArcherLoader.LoadArcher(registry, content, xml, id, default);
        }

        public IArcherEntry RegisterArcherWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource)
        {
            var xml = xmlResource.Xml ?? throw new Exception($"[{content.Metadata.Name}] Invalid or null Xml file.)");

            var archer = xml["Archer"] ??
                throw new Exception($"[{content.Metadata.Name}] Missing Archers element.");

            var id = archer.Attr("id");

            return RegisterArcherWithXml(id, content, registry, archer);
        }

        public IList<IArcherEntry> RegisterArchersWithXml(IModContent content, IModRegistry registry, XmlDocument xml)
        {
            return ArcherLoader.LoadAll(content, registry, xml);
        }

        public IList<IArcherEntry> RegisterArchersWithXml(IModContent content, IModRegistry registry, IResourceInfo xmlResource)
        {
            var xml = xmlResource.Xml ?? throw new Exception($"[{content.Metadata.Name}] Invalid or null Xml file.)");
            return RegisterArchersWithXml(content, registry, xml);
        }

    }
}


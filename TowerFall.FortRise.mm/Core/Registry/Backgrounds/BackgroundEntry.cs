#nullable enable
using System.Xml;

namespace FortRise;

internal sealed class BackgroundEntry : IBackgroundEntry
{
    public string Name { get; init; }
    public BackgroundConfiguration Configuration { get; init; }

    public XmlElement Xml => GetActualXml();
    private XmlElement? xmlElementCache;

    public BackgroundEntry(string name, BackgroundConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }

    private XmlElement GetActualXml()
    {
        if (xmlElementCache != null)
        {
            return xmlElementCache;
        }

        var doc = new XmlDocument();
        var bg = doc.CreateElement("BG");
        bg.SetAttribute("id", Name);


        var background = doc.CreateElement("Background");
        background.SetAttribute("bgColor", Configuration.BackgroundColor.ColorToRGBHex());

        if (Configuration.Background != null)
        {
            foreach (var layer in Configuration.Background)
            {
                AddLayer(background, layer);
            }
        }

        bg.AppendChild(background);

        if (Configuration.Foreground != null)
        {
            var foreground = doc.CreateElement("Foreground");

            foreach (var layer in Configuration.Foreground)
            {
                AddLayer(foreground, layer);
            }

            bg.AppendChild(foreground);
        }

        return bg;

        void AddLayer(XmlElement element, BGLayer layer)
        {
            var childElm = doc.CreateElement(layer.Name);
            if (layer.Data != null)
            {
                foreach (var (key, data) in layer.Data)
                {
                    childElm.SetAttribute(key, data.ToString());
                }
            }

            if (layer.Childrens != null)
            {
                foreach (var child in layer.Childrens)
                {
                    AddLayer(childElm, child);
                }
            }
            else if (layer.SingleChildren != null)
            {
                childElm.InnerText = layer.SingleChildren;
            }

            element.AppendChild(element);
        }
    }
}

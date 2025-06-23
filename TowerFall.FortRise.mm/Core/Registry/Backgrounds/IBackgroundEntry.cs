using System.Xml;

namespace FortRise;
#nullable enable

public interface IBackgroundEntry
{
    public string Name { get; init; }
    public BackgroundConfiguration Configuration { get; init; }
    public XmlElement Xml { get; }
}

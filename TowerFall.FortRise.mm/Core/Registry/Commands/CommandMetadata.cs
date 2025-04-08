#nullable enable
namespace FortRise;

internal class CommandMetadata : ICommand
{
    public string Name { get; init; }
    public CommandConfiguration Configuration { get; init; }


    public CommandMetadata(string name, CommandConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }
}

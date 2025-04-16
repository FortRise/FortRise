#nullable enable
namespace FortRise;

internal class CommandEntry : ICommandEntry
{
    public string Name { get; init; }
    public CommandConfiguration Configuration { get; init; }


    public CommandEntry(string name, CommandConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }
}

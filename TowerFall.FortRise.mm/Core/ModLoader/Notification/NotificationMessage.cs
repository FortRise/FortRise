#nullable enable
namespace FortRise;

public class NotificationMessage
{
    public required string Message { get; init; }
    public NotificationType Type { get; init; }
    public float Duration { get; init; }
    public ModuleMetadata? SentBy { get; init; }

    public float Elapsed { get; internal set; }
    public float Slide { get; internal set; }
    public float Y { get; internal set; }

    internal string Header { get; set; } = string.Empty;
    internal string[]? Lines { get; set; }
    internal float Width { get; set; }
    internal float Height { get; set; }
}

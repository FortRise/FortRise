namespace FortRise;

public class NotificationHandler : INotification
{
    private readonly ModuleMetadata metadata;

    public NotificationHandler(ModuleMetadata metadata)
    {
        this.metadata = metadata;
    }

    public void Send(string message, NotificationType type, float duration = 5)
    {
        NotificationManager.Instance.Send(message, type, metadata, duration);
    }

    public void Send(string message, float duration = 5)
    {
        Send(message, NotificationType.Info, duration);
    }
}

#nullable enable
namespace FortRise;

public interface INotification 
{
    void Send(string message, NotificationType type, float durationInSeconds = 5f);
    void Send(string message, float durationInSeconds = 5f);
}


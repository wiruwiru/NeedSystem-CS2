namespace NeedSystem.Models;

public class NotificationRecord
{
    public string Uuid { get; set; } = string.Empty;
    public string ServerAddress { get; set; } = string.Empty;
    public int ConnectedPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public string MapName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
}
using System.Text.Json.Serialization;

namespace NeedSystem.Configs;

public class PlayerSettings
{
    [JsonPropertyName("NotifyAllPlayers")]
    public bool NotifyAllPlayers { get; set; } = false;

    [JsonPropertyName("DontCountSpecAdmins")]
    public bool DontCountSpecAdmins { get; set; } = false;

    [JsonPropertyName("AdminBypassFlag")]
    public string AdminBypassFlag { get; set; } = "@css/generic";
}
using System.Text.Json.Serialization;

namespace NeedSystem.Configs;

public class CommandSettings
{
    [JsonPropertyName("Command")]
    public List<string> Command { get; set; } = new List<string> { "css_need", ".need" };

    [JsonPropertyName("CommandCooldownSeconds")]
    public int CooldownSeconds { get; set; } = 120;
}
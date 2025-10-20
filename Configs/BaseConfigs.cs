using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace NeedSystem.Configs;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("Commands")]
    public CommandSettings Commands { get; set; } = new();

    [JsonPropertyName("ServerSettings")]
    public ServerSettings Server { get; set; } = new();

    [JsonPropertyName("DiscordSettings")]
    public DiscordSettings Discord { get; set; } = new();

    [JsonPropertyName("PlayerSettings")]
    public PlayerSettings Player { get; set; } = new();
}
using System.Text.Json.Serialization;

namespace NeedSystem.Configs;

public class ServerSettings
{
    [JsonPropertyName("IPandPORT")]
    public string IPandPORT { get; set; } = "45.235.99.18:27025";

    [JsonPropertyName("GetIPandPORTautomatic")]
    public bool GetIPandPORTautomatic { get; set; } = true;

    [JsonPropertyName("UseHostname")]
    public bool UseHostname { get; set; } = false;

    [JsonPropertyName("CustomDomain")]
    public string CustomDomain { get; set; } = "https://crisisgamer.com/connect";

    [JsonPropertyName("MaxServerPlayers")]
    public int MaxServerPlayers { get; set; } = 12;

    [JsonPropertyName("GetMaxServerPlayers")]
    public bool GetMaxServerPlayers { get; set; } = true;

    [JsonPropertyName("MinPlayers")]
    public int MinPlayers { get; set; } = 10;
}
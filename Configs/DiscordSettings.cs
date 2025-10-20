using System.Text.Json.Serialization;

namespace NeedSystem.Configs;

public class DiscordSettings
{
    [JsonPropertyName("WebhookUrl")]
    public string WebhookUrl { get; set; } = "";

    [JsonPropertyName("MentionRoleID")]
    public string MentionRoleID { get; set; } = "";

    [JsonPropertyName("MentionMessage")]
    public bool MentionMessage { get; set; } = true;

    [JsonPropertyName("PlayerNameList")]
    public bool ShowPlayerNameList { get; set; } = true;

    [JsonPropertyName("EmbedSettings")]
    public EmbedSettings Embed { get; set; } = new();
}
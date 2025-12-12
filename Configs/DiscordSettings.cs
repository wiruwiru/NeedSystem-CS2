using System.Text.Json.Serialization;

namespace NeedSystem.Configs;

public class DiscordSettings
{
    [JsonPropertyName("WebhookUrl")]
    public List<string> WebhookUrl { get; set; } = new() { "https://discord.com/api/webhooks/xxxxx/xxxxxxxxx" };

    [JsonPropertyName("MentionRoleID")]
    public string MentionRoleID { get; set; } = "";

    [JsonPropertyName("MentionMessage")]
    public bool MentionMessage { get; set; } = true;

    [JsonPropertyName("PlayerNameList")]
    public bool ShowPlayerNameList { get; set; } = true;

    [JsonPropertyName("EmbedSettings")]
    public EmbedSettings Embed { get; set; } = new();
}
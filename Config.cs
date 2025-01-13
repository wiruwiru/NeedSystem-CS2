using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace NeedSystem;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("WebhookUrl")]
    public string WebhookUrl { get; set; } = "";

    [JsonPropertyName("NotifyAllPlayers")]
    public bool NotifyAllPlayers { get; set; } = false;

    [JsonPropertyName("IPandPORT")]
    public string IPandPORT { get; set; } = "45.235.99.18:27025";

    [JsonPropertyName("GetIPandPORTautomatic")]
    public bool GetIPandPORTautomatic { get; set; } = true;

    [JsonPropertyName("UseHostname")]
    public bool UseHostname { get; set; } = true;

    [JsonPropertyName("CustomDomain")]
    public string CustomDomain { get; set; } = "https://crisisgamer.com/connect";

    [JsonPropertyName("MentionRoleID")]
    public string MentionRoleID { get; set; } = "";

    [JsonPropertyName("MentionMessage")]
    public bool MentionMessage { get; set; } = true;

    [JsonPropertyName("MaxServerPlayers")]
    public int MaxServerPlayers { get; set; } = 12;

    [JsonPropertyName("GetMaxServerPlayers")]
    public bool GetMaxServerPlayers { get; set; } = true;

    [JsonPropertyName("MinPlayers")]
    public int MinPlayers { get; set; } = 10;

    [JsonPropertyName("CommandCooldownSeconds")]
    public int CommandCooldownSeconds { get; set; } = 120;

    [JsonPropertyName("Command")]
    public List<string> Command { get; set; } = new List<string> { "css_need", ".need" };

    [JsonPropertyName("EmbedImage")]
    public bool EmbedImage { get; set; } = true;

    [JsonPropertyName("EmbedColor")]
    public string EmbedColor { get; set; } = "#ffb800";

    [JsonPropertyName("ImagesURL")]
    public string ImagesURL { get; set; } = "https://imagenes.lucauy.dev/CS2/{map}.png";

    [JsonPropertyName("PlayerNameList")]
    public bool PlayerNameList { get; set; } = true;

    [JsonPropertyName("EmbedFooter")]
    public bool EmbedFooter { get; set; } = true;

    [JsonPropertyName("EmbedFooterImage")]
    public string EmbedFooterImage { get; set; } = "https://avatars.githubusercontent.com/u/61034981?v=4";

    [JsonPropertyName("EmbedAuthor")]
    public bool EmbedAuthor { get; set; } = true;

    [JsonPropertyName("EmbedAuthorURL")]
    public string EmbedAuthorURL { get; set; } = "https://lucauy.dev";

    [JsonPropertyName("EmbedAuthorImage")]
    public string EmbedAuthorImage { get; set; } = "https://avatars.githubusercontent.com/u/61034981?v=4";

    [JsonPropertyName("EmbedThumbnail")]
    public bool EmbedThumbnail { get; set; } = true;

    [JsonPropertyName("EmbedThumbnailImage")]
    public string EmbedThumbnailImage { get; set; } = "https://avatars.githubusercontent.com/u/61034981?v=4";

}
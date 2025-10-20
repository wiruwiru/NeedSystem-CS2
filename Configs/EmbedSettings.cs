using System.Text.Json.Serialization;

namespace NeedSystem.Configs;

public class EmbedSettings
{
    [JsonPropertyName("EmbedColor")]
    public string Color { get; set; } = "#ffb800";

    [JsonPropertyName("EmbedImage")]
    public bool ShowImage { get; set; } = true;

    [JsonPropertyName("ImagesURL")]
    public string ImagesURL { get; set; } = "https://cdn.jsdelivr.net/gh/wiruwiru/MapsImagesCDN-CS/png/{map}.png";

    [JsonPropertyName("FooterSettings")]
    public EmbedFooterSettings Footer { get; set; } = new();

    [JsonPropertyName("AuthorSettings")]
    public EmbedAuthorSettings Author { get; set; } = new();

    [JsonPropertyName("ThumbnailSettings")]
    public EmbedThumbnailSettings Thumbnail { get; set; } = new();
}

public class EmbedFooterSettings
{
    [JsonPropertyName("EmbedFooter")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("EmbedFooterImage")]
    public string ImageUrl { get; set; } = "https://avatars.githubusercontent.com/u/61034981?v=4";
}

public class EmbedAuthorSettings
{
    [JsonPropertyName("EmbedAuthor")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("EmbedAuthorURL")]
    public string Url { get; set; } = "https://lucauy.dev";

    [JsonPropertyName("EmbedAuthorImage")]
    public string ImageUrl { get; set; } = "https://avatars.githubusercontent.com/u/61034981?v=4";
}

public class EmbedThumbnailSettings
{
    [JsonPropertyName("EmbedThumbnail")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("EmbedThumbnailImage")]
    public string ImageUrl { get; set; } = "https://avatars.githubusercontent.com/u/61034981?v=4";
}
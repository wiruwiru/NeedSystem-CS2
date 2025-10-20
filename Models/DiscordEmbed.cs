namespace NeedSystem.Models;

public class DiscordEmbedBuilder
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Color { get; set; }
    public List<EmbedField> Fields { get; set; } = new();
    public EmbedImage? Image { get; set; }
    public EmbedFooter? Footer { get; set; }
    public EmbedAuthor? Author { get; set; }
    public EmbedThumbnail? Thumbnail { get; set; }

    public object Build()
    {
        return new
        {
            title = Title,
            description = Description,
            color = Color,
            fields = Fields.Select(f => new
            {
                name = f.Name,
                value = f.Value,
                inline = f.Inline
            }),
            image = Image != null ? new { url = Image.Url } : null,
            footer = Footer != null ? new
            {
                text = Footer.Text,
                icon_url = Footer.IconUrl
            } : null,
            author = Author != null ? new
            {
                name = Author.Name,
                url = Author.Url,
                icon_url = Author.IconUrl
            } : null,
            thumbnail = Thumbnail != null ? new { url = Thumbnail.Url } : null
        };
    }
}

public class EmbedField
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Inline { get; set; }
}

public class EmbedImage
{
    public string Url { get; set; } = string.Empty;
}

public class EmbedFooter
{
    public string Text { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
}

public class EmbedAuthor
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
}

public class EmbedThumbnail
{
    public string Url { get; set; } = string.Empty;
}
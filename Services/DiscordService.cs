using System.Text;
using System.Text.Json;

using NeedSystem.Models;
using NeedSystem.Utils;

namespace NeedSystem.Services;

public class DiscordService
{
    private readonly List<string> _webhookUrls;
    private readonly HttpClient _httpClient;

    public DiscordService(List<string> webhookUrls)
    {
        _webhookUrls = webhookUrls ?? new List<string>();
        _httpClient = new HttpClient();
    }

    public async Task SendEmbedAsync(object embed, string? mentionRoleId = null, string? mentionMessage = null)
    {
        if (_webhookUrls == null || _webhookUrls.Count == 0)
        {
            LogWarning("No webhook URLs configured, skipping Discord notification.");
            return;
        }

        string content = BuildMentionContent(mentionRoleId, mentionMessage);

        var payload = new
        {
            content,
            embeds = new[] { embed }
        };

        var json = JsonSerializer.Serialize(payload);
        var contentString = new StringContent(json, Encoding.UTF8, "application/json");

        var tasks = _webhookUrls.Select(async webhookUrl =>
        {
            try
            {
                if (string.IsNullOrEmpty(webhookUrl))
                {
                    LogWarning($"Webhook URL is null or empty, skipping.");
                    return;
                }

                var response = await _httpClient.PostAsync(webhookUrl, contentString);
                LogResponse(response.IsSuccessStatusCode, response.StatusCode.ToString(), webhookUrl);
            }
            catch (Exception e)
            {
                LogError($"Error sending Discord embed to {webhookUrl}", e);
            }
        });

        await Task.WhenAll(tasks);
    }

    private string BuildMentionContent(string? mentionRoleId, string? mentionMessage)
    {
        if (string.IsNullOrEmpty(mentionMessage))
            return string.Empty;

        string mention = !string.IsNullOrEmpty(mentionRoleId)
            ? $"<@&{mentionRoleId}>"
            : string.Empty;

        return $"{mention} {ColorHelper.StripColorCodes(mentionMessage)}".Trim();
    }

    private void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[NeedSystem] {message}");
        Console.ResetColor();
    }

    private void LogResponse(bool success, string statusCode, string? webhookUrl = null)
    {
        Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
        string urlInfo = !string.IsNullOrEmpty(webhookUrl) ? $" to {webhookUrl}" : "";
        Console.WriteLine(success
            ? $"[NeedSystem] Discord notification sent successfully{urlInfo}"
            : $"[NeedSystem] Error sending Discord notification{urlInfo}: {statusCode}");
        Console.ResetColor();
    }

    private void LogError(string message, Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[NeedSystem] {message}: {e.Message}");
        Console.ResetColor();
    }
}
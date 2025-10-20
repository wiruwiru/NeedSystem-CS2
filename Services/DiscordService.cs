using System.Text;
using System.Text.Json;

using NeedSystem.Models;
using NeedSystem.Utils;

namespace NeedSystem.Services;

public class DiscordService
{
    private readonly string _webhookUrl;
    private readonly HttpClient _httpClient;

    public DiscordService(string webhookUrl)
    {
        _webhookUrl = webhookUrl;
        _httpClient = new HttpClient();
    }

    public async Task SendEmbedAsync(object embed, string? mentionRoleId = null, string? mentionMessage = null)
    {
        try
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                LogWarning("Webhook URL is null or empty, skipping Discord notification.");
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
            var response = await _httpClient.PostAsync(_webhookUrl, contentString);

            LogResponse(response.IsSuccessStatusCode, response.StatusCode.ToString());
        }
        catch (Exception e)
        {
            LogError("Error sending Discord embed", e);
            throw;
        }
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

    private void LogResponse(bool success, string statusCode)
    {
        Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(success
            ? "[NeedSystem] Discord notification sent successfully"
            : $"[NeedSystem] Error sending Discord notification: {statusCode}");
        Console.ResetColor();
    }

    private void LogError(string message, Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[NeedSystem] {message}: {e.Message}");
        Console.ResetColor();
    }
}
namespace NeedSystem.Utils;

public static class TextHelper
{
    public static string CleanPlayerName(string playerName)
    {
        return playerName
            .Replace("[Ready]", "")
            .Replace("[Not Ready]", "")
            .Trim();
    }

    public static string GenerateSteamLink(string playerName, ulong steamId)
    {
        var cleanName = CleanPlayerName(playerName);
        return $"[{cleanName}](https://steamcommunity.com/profiles/{steamId})";
    }
}
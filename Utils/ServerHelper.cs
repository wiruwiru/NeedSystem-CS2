using CounterStrikeSharp.API.Modules.Cvars;

namespace NeedSystem.Utils;

public static class ServerHelper
{
    public static string GetServerAddress(bool autoDetect, string fallbackIpPort)
    {
        if (!autoDetect)
        {
            return fallbackIpPort;
        }

        string? ip = ConVar.Find("ip")?.StringValue;
        string? port = ConVar.Find("hostport")?.GetPrimitiveValue<int>().ToString();

        if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(port))
        {
            return $"{ip}:{port}";
        }

        return fallbackIpPort;
    }

    public static string GetServerHostname(string fallback)
    {
        return ConVar.Find("hostname")?.StringValue ?? fallback;
    }

    public static int GetMaxPlayers(bool autoDetect, int maxPlayers, int serverMaxPlayers)
    {
        return autoDetect ? serverMaxPlayers : maxPlayers;
    }
}
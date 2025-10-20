using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;

using NeedSystem.Utils;

namespace NeedSystem.Services;

public class PlayerService
{
    private readonly bool _dontCountAdmins;
    private readonly string _adminBypassFlag;

    public PlayerService(bool dontCountAdmins, string adminBypassFlag)
    {
        _dontCountAdmins = dontCountAdmins;
        _adminBypassFlag = adminBypassFlag;
    }

    public int GetPlayerCount()
    {
        var players = Utilities.GetPlayers();
        return players.Where(p => !p.IsBot && !p.IsHLTV && ShouldCountPlayer(p)).Count();
    }

    public List<(string Name, ulong SteamId)> GetConnectedPlayers()
    {
        return Utilities.GetPlayers()
            .Where(p => !p.IsBot && !p.IsHLTV && p.Connected == PlayerConnectedState.PlayerConnected)
            .Where(ShouldCountPlayer)
            .Select(p => (p.PlayerName, p.SteamID))
            .ToList();
    }

    public string GetFormattedPlayerList(string noPlayersMessage)
    {
        var players = GetConnectedPlayers();

        if (!players.Any())
        {
            return ColorHelper.StripColorCodes(noPlayersMessage);
        }

        var playerLinks = players
            .Select(p => TextHelper.GenerateSteamLink(p.Name, p.SteamId))
            .ToList();

        return string.Join(", ", playerLinks);
    }

    private bool ShouldCountPlayer(CCSPlayerController player)
    {
        if (!_dontCountAdmins || string.IsNullOrEmpty(_adminBypassFlag))
            return true;

        try
        {
            if (!AdminManager.PlayerHasPermissions(player, _adminBypassFlag))
                return true;

            return player.Team == CsTeam.Terrorist || player.Team == CsTeam.CounterTerrorist;
        }
        catch
        {
            return true;
        }
    }
}
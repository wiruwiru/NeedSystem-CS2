using System.Text;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;

namespace NeedSystem;

[MinimumApiVersion(290)]
public class NeedSystemBase : BasePlugin, IPluginConfig<BaseConfigs>
{
    private string _currentMap = "";
    private DateTime _lastCommandTime = DateTime.MinValue;
    private Translator _translator;

    public override string ModuleName => "NeedSystem";
    public override string ModuleVersion => "1.1.6";
    public override string ModuleAuthor => "luca.uy";
    public override string ModuleDescription => "Allows players to send a message to discord requesting players.";

    public NeedSystemBase(IStringLocalizer localizer)
    {
        _translator = new Translator(localizer);
    }

    public CounterStrikeSharp.API.Modules.Timers.Timer? intervalMessages;

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnMapStart>(mapName =>
        {
            _currentMap = mapName;
        });

        foreach (var command in Config.Command)
        {
            AddCommand(command, "", (controller, info) =>
            {
                if (controller == null) return;

                int secondsRemaining;
                if (!CheckCommandCooldown(out secondsRemaining))
                {
                    controller.PrintToChat(_translator["Prefix"] + " " + _translator["CommandCooldownMessage", secondsRemaining]);
                    return;
                }

                int numberOfPlayers = GetNumberOfPlayers();

                if (numberOfPlayers >= MinPlayers())
                {
                    controller.PrintToChat(_translator["Prefix"] + " " + _translator["EnoughPlayersMessage"]);
                    return;
                }

                NeedCommand(controller, controller?.PlayerName ?? _translator["UnknownPlayer"]);

                _lastCommandTime = DateTime.Now;

                if (Config.NotifyAllPlayers)
                {
                    Server.PrintToChatAll(_translator["Prefix"] + " " + _translator["NotifyAllPlayersMessage", controller?.PlayerName ?? _translator["UnknownPlayer"], Config.CommandCooldownSeconds]);
                }
                else
                {
                    controller?.PrintToChat(_translator["Prefix"] + " " + _translator["NotifyPlayersMessage"]);
                }

            });
        }

        if (hotReload)
        {
            _currentMap = Server.MapName;
        }

        AddCommandListener("say", Listener_Say);
        AddCommandListener("say_team", Listener_Say);
    }

    private HookResult Listener_Say(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null) return HookResult.Continue;

        string message = command.GetCommandString;

        if (Config.Command.Any(cmd => message.Contains(cmd, StringComparison.OrdinalIgnoreCase)))
        {
            ExecuteNeedCommand(caller);
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    private void ExecuteNeedCommand(CCSPlayerController? caller)
    {
        if (caller == null) return;

        int secondsRemaining;
        if (!CheckCommandCooldown(out secondsRemaining))
        {
            caller.PrintToChat(_translator["Prefix"] + " " + _translator["CommandCooldownMessage", secondsRemaining]);
            return;
        }

        int numberOfPlayers = GetNumberOfPlayers();

        if (numberOfPlayers >= MinPlayers())
        {
            caller.PrintToChat(_translator["Prefix"] + " " + _translator["EnoughPlayersMessage"]);
            return;
        }

        NeedCommand(caller, caller?.PlayerName ?? _translator["UnknownPlayer"]);

        _lastCommandTime = DateTime.Now;

        if (Config.NotifyAllPlayers)
        {
            Server.PrintToChatAll(_translator["Prefix"] + " " + _translator["NotifyAllPlayersMessage", caller?.PlayerName ?? _translator["UnknownPlayer"], Config.CommandCooldownSeconds]);
        }
        else
        {
            caller?.PrintToChat(_translator["Prefix"] + " " + _translator["NotifyPlayersMessage"]);
        }
    }

    public required BaseConfigs Config { get; set; }

    public void OnConfigParsed(BaseConfigs config)
    {
        Config = config;
    }

    private bool CheckCommandCooldown(out int secondsRemaining)
    {
        var secondsSinceLastCommand = (int)(DateTime.Now - _lastCommandTime).TotalSeconds;
        secondsRemaining = Config.CommandCooldownSeconds - secondsSinceLastCommand;
        return secondsRemaining <= 0;
    }

    public int GetNumberOfPlayers()
    {
        var players = Utilities.GetPlayers();
        return players.Where(p => !p.IsBot && !p.IsHLTV && ShouldShowPlayerInList(p)).Count();
    }

    private bool ShouldShowPlayerInList(CCSPlayerController player)
    {
        if (!Config.DontCountAdmins || string.IsNullOrEmpty(Config.AdminBypassFlag))
            return true;

        try
        {
            if (!AdminManager.PlayerHasPermissions(player, Config.AdminBypassFlag))
                return true;

            return player.Team == CsTeam.Terrorist || player.Team == CsTeam.CounterTerrorist;
        }
        catch
        {
            return true;
        }
    }

    private int ConvertHexToColor(string hex)
    {
        if (hex.StartsWith("#"))
        {
            hex = hex[1..];
        }
        return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
    }

    public void NeedCommand(CCSPlayerController? caller, string clientName)
    {
        if (caller == null) return;

        clientName = clientName.Replace("[Ready]", "").Replace("[Not Ready]", "").Trim();

        string imageUrl = Config.ImagesURL.Replace("{map}", _currentMap);
        string hour = DateTime.Now.ToString("HH:mm");
        string playerList = string.Empty;

        if (Config.PlayerNameList)
        {
            var players = Utilities.GetPlayers()
                .Where(p => !p.IsBot && !p.IsHLTV && p.Connected == PlayerConnectedState.PlayerConnected)
                .Where(ShouldShowPlayerInList)
                .Select(p => new { p.PlayerName, p.SteamID })
                .ToList();

            if (players.Any())
            {
                var playerDetails = players
                    .Select(p => $"[{p.PlayerName.Replace("[Ready]", "").Replace("[Not Ready]", "").Trim()}](https://steamcommunity.com/profiles/{p.SteamID})")
                    .ToList();
                playerList = string.Join(", ", playerDetails);
            }
            else
            {
                playerList = _translator["NoPlayersConnectedMessage"];
            }
        }

        var fields = new List<object>
        {
            new
            {
                name = _translator["ServerFieldTitle"],
                value = $"```{GetIP()}```",
                inline = true
            },
            new
            {
                name = _translator["PlayersFieldTitle"],
                value = $"```{GetNumberOfPlayers()}/{MaxServerPlayers()}```",
                inline = true
            },
            new
            {
                name = _translator["Hour"],
                value = $"```{hour}```",
                inline = true
            },
            new
            {
                name = _translator["MapFieldTitle"],
                value = $"```{_currentMap}```",
                inline = true
            },
            new
            {
                name = _translator["RequestFieldTitle"],
                value = $"```{clientName}```",
                inline = true
            },

        };

        if (Config.PlayerNameList)
        {
            fields.Add(new
            {
                name = _translator["PlayerListTitle"],
                value = playerList,
                inline = false
            });
        }

        fields.Add(new
        {
            name = _translator["ConnectionFieldTitle"],
            value = $"[**`connect {GetIP()}`**]({GetCustomDomain()}?ip={GetIP()})  {_translator["ClickToConnect"]}",
            inline = false
        });

        var embed = new
        {
            title = Config.UseHostname ? (ConVar.Find("hostname")?.StringValue ?? _translator["EmbedTitle"]) : _translator["EmbedTitle"],
            description = _translator["EmbedDescription"],
            color = ConvertHexToColor(Config.EmbedColor),
            fields,
            image = Config.EmbedImage ? new
            {
                url = imageUrl
            } : null,
            footer = Config.EmbedFooter ? new
            {
                text = _translator["EmbedFooterText"],
                icon_url = Config.EmbedFooterImage
            } : null,
            author = Config.EmbedAuthor ? new
            {
                name = _translator["EmbedAuthorName"],
                url = Config.EmbedAuthorURL,
                icon_url = Config.EmbedAuthorImage
            } : null,
            thumbnail = Config.EmbedThumbnail ? new
            {
                url = Config.EmbedThumbnailImage,
            } : null
        };

        Task.Run(() => SendEmbedToDiscord(embed));
    }

    private async Task SendEmbedToDiscord(object embed)
    {
        try
        {
            var webhookUrl = GetWebhook();

            if (string.IsNullOrEmpty(webhookUrl))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Webhook URL is null or empty, skipping Discord notification.");
                return;
            }

            var httpClient = new HttpClient();

            string content = string.Empty;
            if (Config.MentionMessage)
            {
                string mention = !string.IsNullOrEmpty(MentionRoleID()) ? $"<@&{MentionRoleID()}>" : string.Empty;
                content = $"{mention} {_translator["NeedInServerMessage"]}";
            }

            var payload = new
            {
                content,
                embeds = new[] { embed }
            };

            var json = JsonSerializer.Serialize(payload);
            var contentString = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(webhookUrl, contentString);

            Console.ForegroundColor = response.IsSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(response.IsSuccessStatusCode ? "Success" : $"Error: {response.StatusCode}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private string GetWebhook()
    {
        return Config.WebhookUrl;
    }
    private string GetCustomDomain()
    {
        return Config.CustomDomain;
    }
    private string GetIP()
    {
        if (Config.GetIPandPORTautomatic)
        {
            string? ip = ConVar.Find("ip")?.StringValue;
            string? port = ConVar.Find("hostport")?.GetPrimitiveValue<int>().ToString();

            if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(port))
            {
                return $"{ip}:{port}";
            }
            else
            {
                return Config.IPandPORT;
            }
        }
        else
        {
            return Config.IPandPORT;
        }
    }
    private string MentionRoleID()
    {
        return Config.MentionRoleID;
    }

    private string MaxServerPlayers()
    {
        if (Config.GetMaxServerPlayers)
        {
            return Server.MaxPlayers.ToString();
        }
        else
        {
            return Config.MaxServerPlayers.ToString();
        }
    }

    private int MinPlayers()
    {
        return Config.MinPlayers;
    }
}
using System.Text;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;

namespace NeedSystem;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("WebhookUrl")]
    public string WebhookUrl { get; set; } = "";

    [JsonPropertyName("IPandPORT")]
    public string IPandPORT { get; set; } = "45.235.99.18:27025";

    [JsonPropertyName("CustomDomain")]
    public string CustomDomain { get; set; } = "https://crisisgamer.com/redirect/connect.php";

    [JsonPropertyName("MentionRoleID")]
    public string MentionRoleID { get; set; } = "";

    [JsonPropertyName("MaxServerPlayers")]
    public int MaxServerPlayers { get; set; } = 13;

    [JsonPropertyName("MinPlayers")]
    public int MinPlayers { get; set; } = 10;

    [JsonPropertyName("CommandCooldownSeconds")]
    public int CommandCooldownSeconds { get; set; } = 120;

    [JsonPropertyName("Command")]
    public List<string> Command { get; set; } = new List<string> { "css_need", "css_needplayers" };

    [JsonPropertyName("EmbedImage")]
    public bool EmbedImage { get; set; } = true;

    [JsonPropertyName("EmbedColor")]
    public string EmbedColor { get; set; } = "#ffb800";

    [JsonPropertyName("ImagesURL")]
    public string ImagesURL { get; set; } = "https://imagenes.redage.es/CS2/{map}.png";

    [JsonPropertyName("PlayerNameList")]
    public bool PlayerNameList { get; set; } = true;

}

public class NeedSystemBase : BasePlugin, IPluginConfig<BaseConfigs>
{
    private string _currentMap = "";
    private DateTime _lastCommandTime = DateTime.MinValue;
    private Translator _translator;

    public override string ModuleName => "NeedSystem";
    public override string ModuleVersion => "1.0.8";
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

                controller?.PrintToChat(_translator["Prefix"] + " " + _translator["NotifyPlayersMessage"]);
            });
        }

        if (hotReload)
        {
            _currentMap = Server.MapName;
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
        return players.Count(p => !p.IsBot && !p.IsHLTV);
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
                .Select(p => new { p.PlayerName, p.SteamID })
                .ToList();

            if (players.Any())
            {
                var playerDetails = players
                    .Select(p => $"[{p.PlayerName}](https://steamcommunity.com/profiles/{p.SteamID})")
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
            title = _translator["EmbedTitle"],
            description = _translator["EmbedDescription"],
            color = ConvertHexToColor(Config.EmbedColor),
            fields,
            image = Config.EmbedImage ? new
            {
                url = imageUrl
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

            string mention = !string.IsNullOrEmpty(MentionRoleID()) ? $"<@&{MentionRoleID()}>" : string.Empty;

            var payload = new
            {
                content = $"{mention} {_translator["NeedInServerMessage"]}",
                embeds = new[] { embed }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(webhookUrl, content);

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
        return Config.IPandPORT;
    }
    private string MentionRoleID()
    {
        return Config.MentionRoleID;
    }
    private int MaxServerPlayers()
    {
        return Config.MaxServerPlayers;
    }
    private int MinPlayers()
    {
        return Config.MinPlayers;
    }
}
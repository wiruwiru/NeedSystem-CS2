using System.Text;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;

namespace NeedSystem
{
    public class NeedSystem : BasePlugin
    {
        private string _currentMap = "";
        private DateTime _lastCommandTime = DateTime.MinValue;
        private Dictionary<string, DateTime> _playerLastCommandTimes = new();

        private Translator _translator;

        public override string ModuleAuthor => "luca.uy";
        public override string ModuleName => "NeedSystem";
        public override string ModuleVersion => "v1.0.4";

        private Config _config = null!;

        public NeedSystem(IStringLocalizer localizer)
        {
            _translator = new Translator(localizer);
        }

        public override void Load(bool hotReload)
        {
            _config = LoadConfig();

            RegisterListener<Listeners.OnMapStart>(mapName =>
            {
                _currentMap = mapName;
            });

            foreach (var command in _config.Command)
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

                    controller.PrintToChat(_translator["Prefix"] + " " + _translator["NotifyPlayersMessage"]);
                });
            }
        }

        private bool CheckCommandCooldown(out int secondsRemaining)
        {
            var secondsSinceLastCommand = (int)(DateTime.Now - _lastCommandTime).TotalSeconds;
            secondsRemaining = _config.CommandCooldownSeconds - secondsSinceLastCommand;
            return secondsRemaining <= 0;
        }

        public int GetNumberOfPlayers()
        {
            var players = Utilities.GetPlayers();
            return players.Count(p => !p.IsBot && !p.IsHLTV);
        }

        public void NeedCommand(CCSPlayerController? caller, string clientName)
        {
            if (caller == null) return;

            clientName = clientName.Replace("[Ready]", "").Replace("[Not Ready]", "").Trim();

            var embed = new
            {
                title = _translator["EmbedTitle"],
                description = _translator["EmbedDescription"],
                color = 9246975,
                fields = new[]
                {
                    new
                    {
                        name = _translator["ServerFieldTitle"],
                        value = $"```{GetIP()}```",
                        inline = false
                    },
                    new
                    {
                        name = _translator["RequestFieldTitle"],
                        value = $"``` {clientName} ```",
                        inline = false
                    },
                    new
                    {
                        name = _translator["MapFieldTitle"],
                        value = $"```{_currentMap}```",
                        inline = false
                    },
                    new
                    {
                        name = _translator["PlayersFieldTitle"],
                        value = $"```{GetNumberOfPlayers()}/{MaxServerPlayers()}```",
                        inline = false
                    },
                    new
                    {
                        name = _translator["ConnectionFieldTitle"],
                        value = $"[**`connect {GetIP()}`**]({GetCustomDomain()}?ip={GetIP()})  {_translator["ClickToConnect"]}",
                        inline = false
                    }
                }
            };

            Task.Run(() => SendEmbedToDiscord(embed));
        }

        private async Task SendEmbedToDiscord(object embed)
        {
            try
            {
                var webhookUrl = GetWebhook();

                if (string.IsNullOrEmpty(webhookUrl)) return;

                var httpClient = new HttpClient();

                var payload = new
                {
                    content = $"<@&{MentionRoleID()}> {_translator["NeedInServerMessage"]}",
                    embeds = new[] { embed }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(webhookUrl, content);

                Console.ForegroundColor = response.IsSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(response.IsSuccessStatusCode
                    ? "Success"
                    : $"Error: {response.StatusCode}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private Config LoadConfig()
        {
            var configPath = Path.Combine(ModuleDirectory, "config.json");

            if (!File.Exists(configPath)) return CreateConfig(configPath);

            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

            return config;
        }

        private Config CreateConfig(string configPath)
        {
            var config = new Config
            {
                WebhookUrl = "",
                IPandPORT = "45.235.99.18:27025",
                CustomDomain = "https://crisisgamer.com/redirect/connect.php",
                MentionRoleID = "",
                MaxServerPlayers = 13,
                MinPlayers = 10,
                CommandCooldownSeconds = 120,
                Command = new List<string> { "css_need", "css_needplayers" }
            };

            File.WriteAllText(configPath,
                JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("[NeedSystem] The configuration was successfully saved to a file: " + configPath);
            Console.ResetColor();

            return config;
        }

        private string GetWebhook()
        {
            return _config.WebhookUrl;
        }
        private string GetCustomDomain()
        {
            return _config.CustomDomain;
        }
        private string GetIP()
        {
            return _config.IPandPORT;
        }
        private string MentionRoleID()
        {
            return _config.MentionRoleID;
        }
        private int MaxServerPlayers()
        {
            return _config.MaxServerPlayers;
        }
        private int MinPlayers()
        {
            return _config.MinPlayers;
        }
    }

    public class Config
    {
        public string WebhookUrl { get; set; } = "";
        public string IPandPORT { get; set; } = "";
        public string CustomDomain { get; set; } = "https://crisisgamer.com/redirect/connect.php";
        public string MentionRoleID { get; set; } = "";
        public int MaxServerPlayers { get; set; } = 13;
        public int MinPlayers { get; set; } = 10;
        public int CommandCooldownSeconds { get; set; } = 120;
        public List<string> Command { get; set; } = new List<string> { "css_need" };
    }
}

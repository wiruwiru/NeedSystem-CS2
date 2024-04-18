using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Localization;

namespace NeedSystem
{
    public class NeedSystem : BasePlugin
    {
        private string _currentMap = "";
        private DateTime _lastCommandTime = DateTime.MinValue;
        private const int CommandCooldownSeconds = 120; // dos minutos

        private Translator _translator;

        public override string ModuleAuthor => "luca.uy";
        public override string ModuleName => "NeedSystem";
        public override string ModuleVersion => "v1.0.0";

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

            AddCommand("css_need", "", (controller, info) =>
            {
                if (controller == null) return;

                if (!CheckCommandCooldown())
                {
                    controller.PrintToChat(_translator["Prefix"] + " " + _translator["CommandCooldownMessage", CommandCooldownSeconds]);
                    return;
                }

                int numberOfPlayers = GetNumberOfPlayers();

                if (numberOfPlayers >= MinPlayers())
                {
                    controller.PrintToChat(_translator["Prefix"] + " " + _translator["EnoughPlayersMessage"]);
                    return;
                }

                NeedCommand(controller, controller?.PlayerName ?? "Desconocido");

                _lastCommandTime = DateTime.Now;

                controller.PrintToChat(_translator["Prefix"] + " " + _translator["NotifyPlayersMessage"]);
            });
        }

        private bool CheckCommandCooldown()
        {
            return (DateTime.Now - _lastCommandTime).TotalSeconds >= CommandCooldownSeconds;
        }

        public int GetNumberOfPlayers()
        {
            var players = Utilities.GetPlayers();
            return players.Count();
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
                        value = $"[**`connect {GetIP()}`**](https://crisisgamer.com/redirect/retakecs2.php?ip={GetIP()})  {_translator["ClickToConnect"]}",
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
                    content = $"¡<@&{MentionRoleID()}> {_translator["NeedInServerMessage"]}!",
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
                WebhookUrl = "", // Debes crearlo en el canal donde enviaras los avisos.
                IP = "45.235.99.18:27025", // Remplaza por la dirección IP de tu servidor.
                MentionRoleID = "", // Debes tener activado el modo desarrollador de discord, click derecho en el rol y copias su ID.
                MaxServerPlayers = 13, // La cantidad maxima de slots que tiene tu servidord.
                MinPlayers = 10 // En este caso si hay diez o mas jugadores conectados el comando no se puede utilizar.
            };

            File.WriteAllText(configPath,
                JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("[NeedSystem] La configuración se ha guardado correctamente en el archivo: " + configPath);
            Console.ResetColor();

            return config;
        }

        private string GetWebhook()
        {
            return _config.WebhookUrl;
        }
        private string GetIP()
        {
            return _config.IP;
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
        public string IP { get; set; } = "";
        public string MentionRoleID { get; set; } = "";
        public int MaxServerPlayers { get; set; } = 13;
        public int MinPlayers { get; set; } = 10;
    }
}

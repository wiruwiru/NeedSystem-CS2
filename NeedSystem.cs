using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Admin;

using NeedSystem.Services;
using NeedSystem.Utils;
using NeedSystem.Models;
using NeedSystem.Constants;
using NeedSystem.Configs;

namespace NeedSystem;

[MinimumApiVersion(342)]
public class NeedSystemBase : BasePlugin, IPluginConfig<BaseConfigs>
{
    private CooldownService? _cooldownService;
    private PlayerService? _playerService;
    private DiscordService? _discordService;
    private DatabaseService? _databaseService;

    private string _currentMap = string.Empty;

    public override string ModuleName => "NeedSystem";
    public override string ModuleVersion => "1.1.8";
    public override string ModuleAuthor => "luca.uy";
    public override string ModuleDescription => "Allows players to send a message to discord requesting players.";

    public required BaseConfigs Config { get; set; }

    public override void Load(bool hotReload)
    {
        InitializeServices();
        RegisterEventListeners();
        RegisterCommands();

        if (hotReload)
        {
            _currentMap = Server.MapName;
        }

        _ = InitializeDatabaseAsync();
    }

    public void OnConfigParsed(BaseConfigs config)
    {
        Config = config;
        InitializeServices();
        _ = InitializeDatabaseAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            if (_databaseService != null && Config.Database.Enabled)
            {
                await _databaseService.InitializeDatabase();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[NeedSystem] Database initialized successfully!");
                Console.ResetColor();
            }
            else if (_databaseService != null && !Config.Database.Enabled)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[NeedSystem] Database is disabled in configuration");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[NeedSystem] Failed to initialize database: {ex.Message}");
            Console.ResetColor();
        }
    }

    private void InitializeServices()
    {
        _cooldownService = new CooldownService(Config.Commands.CooldownSeconds);
        _playerService = new PlayerService(Config.Player.DontCountSpecAdmins, Config.Player.AdminBypassFlag);
        _discordService = new DiscordService(Config.Discord.WebhookUrl);
        _databaseService = new DatabaseService(Config.Database);
    }

    private void RegisterEventListeners()
    {
        RegisterListener<Listeners.OnMapStart>(mapName =>
        {
            _currentMap = mapName;
        });

        AddCommandListener("say", OnSayCommand);
        AddCommandListener("say_team", OnSayCommand);
    }

    private void RegisterCommands()
    {
        foreach (var command in Config.Commands.Command)
        {
            AddCommand(command, "Request more players on Discord", OnNeedCommand);
        }

        foreach (var command in Config.Commands.ForceCommand)
        {
            AddCommand(command, "Force request more players on Discord (bypasses cooldown and minimum players)", OnForceNeedCommand);
        }
    }

    private void OnNeedCommand(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null) return;
        ExecuteNeedCommand(controller, false);
    }

    private void OnForceNeedCommand(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null) return;

        if (!HasForcePermission(controller))
        {
            controller.PrintToChat($"{Localizer[LocalizationKeys.Prefix]} {Localizer[LocalizationKeys.NoPermissionMessage]}");
            return;
        }

        ExecuteNeedCommand(controller, true);
    }

    private HookResult OnSayCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null) return HookResult.Continue;

        string message = command.GetCommandString;

        if (Config.Commands.Command.Any(cmd => message.Contains(cmd, StringComparison.OrdinalIgnoreCase)))
        {
            ExecuteNeedCommand(caller, false);
            return HookResult.Handled;
        }

        if (Config.Commands.ForceCommand.Any(cmd => message.Contains(cmd, StringComparison.OrdinalIgnoreCase)))
        {
            if (HasForcePermission(caller))
            {
                ExecuteNeedCommand(caller, true);
            }
            else
            {
                caller.PrintToChat($"{Localizer[LocalizationKeys.Prefix]} {Localizer[LocalizationKeys.NoPermissionMessage]}");
            }
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    private bool HasForcePermission(CCSPlayerController controller)
    {
        return AdminManager.PlayerHasPermissions(controller, Config.Commands.ForceCommandFlag);
    }

    private void ExecuteNeedCommand(CCSPlayerController controller, bool bypassRestrictions)
    {
        if (_cooldownService == null || _playerService == null) return;

        if (!bypassRestrictions)
        {
            if (!_cooldownService.CanExecute(out int secondsRemaining))
            {
                controller.PrintToChat($"{Localizer[LocalizationKeys.Prefix]} {Localizer[LocalizationKeys.CommandCooldownMessage, secondsRemaining]}");
                return;
            }

            int playerCount = _playerService.GetPlayerCount();
            if (playerCount >= Config.Server.MinPlayers)
            {
                controller.PrintToChat($"{Localizer[LocalizationKeys.Prefix]} {Localizer[LocalizationKeys.EnoughPlayersMessage]}");
                return;
            }
        }

        string playerName = controller.PlayerName ?? Localizer[LocalizationKeys.UnknownPlayer];
        SendDiscordNotification(playerName);
        _cooldownService.UpdateLastExecution();

        NotifyPlayers(controller, playerName);
    }

    private void NotifyPlayers(CCSPlayerController controller, string playerName)
    {
        if (Config.Player.NotifyAllPlayers)
        {
            Server.PrintToChatAll($"{Localizer[LocalizationKeys.Prefix]} {Localizer[LocalizationKeys.NotifyAllPlayersMessage, playerName, Config.Commands.CooldownSeconds]}");
        }
        else
        {
            controller.PrintToChat($"{Localizer[LocalizationKeys.Prefix]} {Localizer[LocalizationKeys.NotifyPlayersMessage]}");
        }
    }

    private void SendDiscordNotification(string playerName)
    {
        if (_discordService == null || _playerService == null) return;

        var embed = BuildDiscordEmbed(playerName);
        string? mentionMessage = Config.Discord.MentionMessage ? Convert.ToString(Localizer[LocalizationKeys.NeedInServerMessage]) : null;

        NotificationRecord? notificationRecord = null;
        if (_databaseService != null && _databaseService.IsEnabled())
        {
            string cleanPlayerName = TextHelper.CleanPlayerName(playerName);
            string serverAddress = ServerHelper.GetServerAddress(Config.Server.GetIPandPORTautomatic, Config.Server.IPandPORT);
            int maxPlayers = ServerHelper.GetMaxPlayers(Config.Server.GetMaxServerPlayers, Config.Server.MaxServerPlayers, Server.MaxPlayers);
            int playerCount = _playerService.GetPlayerCount();

            notificationRecord = new NotificationRecord
            {
                Uuid = Guid.NewGuid().ToString(),
                ServerAddress = serverAddress,
                ConnectedPlayers = playerCount,
                MaxPlayers = maxPlayers,
                MapName = _currentMap,
                Timestamp = DateTime.Now,
                RequestedBy = cleanPlayerName
            };
        }

        Task.Run(async () =>
        {
            await _discordService.SendEmbedAsync(
                embed.Build(),
                Config.Discord.MentionRoleID,
                mentionMessage
            );

            if (notificationRecord != null && _databaseService != null)
            {
                await SaveNotificationToDatabase(notificationRecord);
            }
        });
    }

    private async Task SaveNotificationToDatabase(NotificationRecord record)
    {
        if (_databaseService == null) return;

        try
        {
            bool saved = await _databaseService.SaveNotification(record);
            if (saved)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[NeedSystem] Notification record saved to database (UUID: {record.Uuid})");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[NeedSystem] Error saving notification to database: {ex.Message}");
            Console.ResetColor();
        }
    }

    private DiscordEmbedBuilder BuildDiscordEmbed(string playerName)
    {
        string cleanPlayerName = TextHelper.CleanPlayerName(playerName);
        string serverAddress = ServerHelper.GetServerAddress(Config.Server.GetIPandPORTautomatic, Config.Server.IPandPORT);
        int maxPlayers = ServerHelper.GetMaxPlayers(Config.Server.GetMaxServerPlayers, Config.Server.MaxServerPlayers, Server.MaxPlayers);
        string currentTime = DateTime.Now.ToString("HH:mm");

        var embedBuilder = new DiscordEmbedBuilder
        {
            Title = Config.Server.UseHostname
                ? ServerHelper.GetServerHostname(ColorHelper.StripColorCodes(Localizer[LocalizationKeys.EmbedTitle]))
                : ColorHelper.StripColorCodes(Localizer[LocalizationKeys.EmbedTitle]),
            Description = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.EmbedDescription]),
            Color = ColorHelper.ConvertHexToColor(Config.Discord.Embed.Color)
        };

        AddBasicFields(embedBuilder, serverAddress, maxPlayers, currentTime, cleanPlayerName);

        if (Config.Discord.ShowPlayerNameList && _playerService != null)
        {
            string playerList = _playerService.GetFormattedPlayerList(Localizer[LocalizationKeys.NoPlayersConnectedMessage]);
            embedBuilder.Fields.Add(new EmbedField
            {
                Name = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.PlayerListTitle]),
                Value = playerList,
                Inline = false
            });
        }

        AddConnectionField(embedBuilder, serverAddress);
        ConfigureVisualElements(embedBuilder);

        return embedBuilder;
    }

    private void AddBasicFields(DiscordEmbedBuilder builder, string serverAddress, int maxPlayers, string time, string playerName)
    {
        if (_playerService == null) return;

        builder.Fields.AddRange(
        [
            new EmbedField
            {
                Name = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.ServerFieldTitle]),
                Value = $"```{serverAddress}```",
                Inline = true
            },
            new EmbedField
            {
                Name = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.PlayersFieldTitle]),
                Value = $"```{_playerService.GetPlayerCount()}/{maxPlayers}```",
                Inline = true
            },
            new EmbedField
            {
                Name = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.Hour]),
                Value = $"```{time}```",
                Inline = true
            },
            new EmbedField
            {
                Name = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.MapFieldTitle]),
                Value = $"```{_currentMap}```",
                Inline = true
            },
            new EmbedField
            {
                Name = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.RequestFieldTitle]),
                Value = $"```{playerName}```",
                Inline = true
            }
        ]);
    }

    private void AddConnectionField(DiscordEmbedBuilder builder, string serverAddress)
    {
        string clickToConnect = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.ClickToConnect]);
        string connectionUrl = $"{Config.Server.CustomDomain}?ip={serverAddress}";

        builder.Fields.Add(new EmbedField
        {
            Name = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.ConnectionFieldTitle]),
            Value = $"[**`connect {serverAddress}`**]({connectionUrl})  {clickToConnect}",
            Inline = false
        });
    }

    private void ConfigureVisualElements(DiscordEmbedBuilder builder)
    {
        if (Config.Discord.Embed.ShowImage)
        {
            builder.Image = new EmbedImage
            {
                Url = Config.Discord.Embed.ImagesURL.Replace("{map}", _currentMap)
            };
        }

        if (Config.Discord.Embed.Footer.Enabled)
        {
            builder.Footer = new EmbedFooter
            {
                Text = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.EmbedFooterText]),
                IconUrl = Config.Discord.Embed.Footer.ImageUrl
            };
        }

        if (Config.Discord.Embed.Author.Enabled)
        {
            builder.Author = new EmbedAuthor
            {
                Name = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.EmbedAuthorName]),
                Url = Config.Discord.Embed.Author.Url,
                IconUrl = Config.Discord.Embed.Author.ImageUrl
            };
        }

        if (Config.Discord.Embed.Thumbnail.Enabled)
        {
            builder.Thumbnail = new EmbedThumbnail
            {
                Url = Config.Discord.Embed.Thumbnail.ImageUrl
            };
        }
    }
}
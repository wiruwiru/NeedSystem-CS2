using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes;

using NeedSystem.Services;
using NeedSystem.Utils;
using NeedSystem.Models;
using NeedSystem.Constants;

namespace NeedSystem;

[MinimumApiVersion(342)]
public class NeedSystemBase : BasePlugin, IPluginConfig<BaseConfigs>
{
    private CooldownService? _cooldownService;
    private PlayerService? _playerService;
    private DiscordService? _discordService;

    private string _currentMap = string.Empty;

    public override string ModuleName => "NeedSystem";
    public override string ModuleVersion => "1.1.7";
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
    }

    public void OnConfigParsed(BaseConfigs config)
    {
        Config = config;
        InitializeServices();
    }

    private void InitializeServices()
    {
        _cooldownService = new CooldownService(Config.CommandCooldownSeconds);
        _playerService = new PlayerService(Config.DontCountAdmins, Config.AdminBypassFlag);
        _discordService = new DiscordService(Config.WebhookUrl);
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
        foreach (var command in Config.Command)
        {
            AddCommand(command, "Request more players on Discord", OnNeedCommand);
        }
    }

    private void OnNeedCommand(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null) return;
        ExecuteNeedCommand(controller);
    }

    private HookResult OnSayCommand(CCSPlayerController? caller, CommandInfo command)
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

    private void ExecuteNeedCommand(CCSPlayerController controller)
    {
        if (_cooldownService == null || _playerService == null) return;

        if (!_cooldownService.CanExecute(out int secondsRemaining))
        {
            controller.PrintToChat($"{Localizer[LocalizationKeys.Prefix]} {Localizer[LocalizationKeys.CommandCooldownMessage, secondsRemaining]}");
            return;
        }

        int playerCount = _playerService.GetPlayerCount();
        if (playerCount >= Config.MinPlayers)
        {
            controller.PrintToChat($"{Localizer[LocalizationKeys.Prefix]} {Localizer[LocalizationKeys.EnoughPlayersMessage]}");
            return;
        }

        string playerName = controller.PlayerName ?? Localizer[LocalizationKeys.UnknownPlayer];
        SendDiscordNotification(playerName);
        _cooldownService.UpdateLastExecution();

        NotifyPlayers(controller, playerName);
    }

    private void NotifyPlayers(CCSPlayerController controller, string playerName)
    {
        if (Config.NotifyAllPlayers)
        {
            Server.PrintToChatAll($"{Localizer[LocalizationKeys.Prefix]} {Localizer[LocalizationKeys.NotifyAllPlayersMessage, playerName, Config.CommandCooldownSeconds]}");
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
        string? mentionMessage = Config.MentionMessage ? Convert.ToString(Localizer[LocalizationKeys.NeedInServerMessage]) : null;

        Task.Run(() => _discordService.SendEmbedAsync(
            embed.Build(),
            Config.MentionRoleID,
            mentionMessage
        ));
    }

    private DiscordEmbedBuilder BuildDiscordEmbed(string playerName)
    {
        string cleanPlayerName = TextHelper.CleanPlayerName(playerName);
        string serverAddress = ServerHelper.GetServerAddress(Config.GetIPandPORTautomatic, Config.IPandPORT);
        int maxPlayers = ServerHelper.GetMaxPlayers(Config.GetMaxServerPlayers, Config.MaxServerPlayers, Server.MaxPlayers);
        string currentTime = DateTime.Now.ToString("HH:mm");

        var embedBuilder = new DiscordEmbedBuilder
        {
            Title = Config.UseHostname
                ? ServerHelper.GetServerHostname(ColorHelper.StripColorCodes(Localizer[LocalizationKeys.EmbedTitle]))
                : ColorHelper.StripColorCodes(Localizer[LocalizationKeys.EmbedTitle]),
            Description = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.EmbedDescription]),
            Color = ColorHelper.ConvertHexToColor(Config.EmbedColor)
        };

        AddBasicFields(embedBuilder, serverAddress, maxPlayers, currentTime, cleanPlayerName);
        if (Config.PlayerNameList && _playerService != null)
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
        string connectionUrl = $"{Config.CustomDomain}?ip={serverAddress}";

        builder.Fields.Add(new EmbedField
        {
            Name = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.ConnectionFieldTitle]),
            Value = $"[**`connect {serverAddress}`**]({connectionUrl})  {clickToConnect}",
            Inline = false
        });
    }

    private void ConfigureVisualElements(DiscordEmbedBuilder builder)
    {
        if (Config.EmbedImage)
        {
            builder.Image = new EmbedImage
            {
                Url = Config.ImagesURL.Replace("{map}", _currentMap)
            };
        }

        if (Config.EmbedFooter)
        {
            builder.Footer = new EmbedFooter
            {
                Text = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.EmbedFooterText]),
                IconUrl = Config.EmbedFooterImage
            };
        }

        if (Config.EmbedAuthor)
        {
            builder.Author = new EmbedAuthor
            {
                Name = ColorHelper.StripColorCodes(Localizer[LocalizationKeys.EmbedAuthorName]),
                Url = Config.EmbedAuthorURL,
                IconUrl = Config.EmbedAuthorImage
            };
        }

        if (Config.EmbedThumbnail)
        {
            builder.Thumbnail = new EmbedThumbnail
            {
                Url = Config.EmbedThumbnailImage
            };
        }
    }
}
# NeedSystem CS2
Allows players to send a message to discord requesting players.
## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

2. Download NeedSystem.zip

3. Unzip the archive and upload it to the game server

4. Start the server and wait for the config.json file to be generated.

5. Complete the configuration file with the parameters of your choice.

# Config
| Parameter | Description | Required     |
| :------- | :------- | :------- |
| `WebhookUrl` | You must create it in the channel where you will send the notices. |**YES** |
| `IP` | Replace with the IP address of your server. |**YES** |
| `MentionRoleID` | You must have the discord developer mode activated, right click on the role and copy its ID. |**YES** |
| `MaxServerPlayers` | Maximum number of slots your server has. |**YES** |
| `MinPlayers` | In this case if there are ten or more players connected the command cannot be used. | **YES** |
| `CommandCooldownSeconds` | Command cooldown time in seconds. | **YES** |

## Configuration example
```
{
    "WebhookUrl": "https://discord.com/api/webhooks/xxxxx/xxxxxxxxx,
    "IP": "45.235.99.18:27025",
    "MentionRoleID": "1111767358881681519",
    "MaxServerPlayers": 13,
    "MinPlayers": 10,
    "CommandCooldownSeconds": 120
}
```

# Lang configuration

In the 'lang' folder, you'll find various files. For instance, 'es.json' is designated for the Spanish language. You're welcome to modify this file to better suit your style and language preferences. The language utilized depends on your settings in 'core.json' of CounterStrikeSharp.

# Commands
`css_need` `!need` - Send message

## TO-DO
- Change configuration file location
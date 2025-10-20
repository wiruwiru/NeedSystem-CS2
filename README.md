# NeedSystem CS2
Allows players to send a message to discord requesting players.

https://github.com/user-attachments/assets/fca8fed1-c07c-4546-9972-dc1cd49ab769

## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

2. Download [NeedSystem.zip](https://github.com/wiruwiru/NeedSystem-CS2/releases) from the releases section.

3. Unzip the archive and upload it to the game server

4. Start the server and wait for the config.json file to be generated.

5. Complete the configuration file with the parameters of your choice.

# Config

## Commands Settings
| Parameter | Description | Required |
| :------- | :------- | :------- |
| `Command` | You can change the command to be used by the players or add extra commands. | **YES** |
| `CommandCooldownSeconds` | Command cooldown time in seconds. | **YES** |

## Server Settings
| Parameter | Description | Required |
| :------- | :------- | :------- |
| `IPandPORT` | Replace with the IP address of your server. | **YES** |
| `GetIPandPORTautomatic` | When you activate this option the plugin will try to get the IP:PORT of your server automatically, in case it is not possible use the IPandPORT configuration. | **YES** |
| `UseHostname` | If you set this configuration to true, the "EmbedTitle" of the translation will be replaced by the hostname you have configured in your server.cfg file. | **YES** |
| `CustomDomain` | You can replace it with your domain if you want, the connect.php file is available in the main branch. | **YES** |
| `MaxServerPlayers` | Maximum number of slots your server has. | **YES** |
| `GetMaxServerPlayers` | When you activate this option the plugin will try to get the maximum number of players on the server automatically, in case it is not possible use the MaxServerPlayers configuration. | **YES** |
| `MinPlayers` | In this case if there are ten or more players connected the command cannot be used. | **YES** |

## Discord Settings
| Parameter | Description | Required |
| :------- | :------- | :------- |
| `WebhookUrl` | You must create it in the channel where you will send the notices. | **YES** |
| `MentionRoleID` | You must have the discord developer mode activated, right click on the role and copy its ID. | **NO** |
| `MentionMessage` | You can use this option to deactivate the mention message completely, with this deactivated only the embed will be sent. | **YES** |
| `PlayerNameList` | Displays a list of the names and profiles of the users who are logged in at the time the command is sent. | **YES** |

### Embed Settings
| Parameter | Description | Required |
| :------- | :------- | :------- |
| `EmbedColor` | You can change this to your favorite color, in Hex format. | **YES** |
| `EmbedImage` | Enables or disables the map image to be shown in the Embed. | **YES** |
| `ImagesURL` | Url from where the map images are taken, recommended to use your own url if you use workshop maps. | **YES** |

### Footer Settings
| Parameter | Description | Required |
| :------- | :------- | :------- |
| `EmbedFooter` | You can use this option to disable or enable the embed footer. | **YES** |
| `EmbedFooterImage` | It will be the image (logo) that will appear in the embed footer. | **YES** |

### Author Settings
| Parameter | Description | Required |
| :------- | :------- | :------- |
| `EmbedAuthor` | You can use this option to disable or enable the embed author. | **YES** |
| `EmbedAuthorURL` | This will be the url that will be redirected to when a user clicks on the embed author. | **YES** |
| `EmbedAuthorImage` | It will be the image (logo) that will appear as the author of the embed. | **YES** |

### Thumbnail Settings
| Parameter | Description | Required |
| :------- | :------- | :------- |
| `EmbedThumbnail` | You can use this option to disable or enable the embed thumbnail. | **YES** |
| `EmbedThumbnailImage` | It will be the image (logo) that will appear as the thumbnail of the embed. | **YES** |

## Player Settings
| Parameter | Description | Required |
| :------- | :------- | :------- |
| `NotifyAllPlayers` | When a user uses !need it notifies the whole server that the command was used and how long it takes to be able to use the command again, if it is set to false only the user who used the command will be notified. | **YES** |
| `DontCountSpecAdmins` | When enabled, admins in spectator mode won't be counted towards the player count. | **YES** |
| `AdminBypassFlag` | The admin flag required to bypass spectator counting (only works if DontCountSpecAdmins is enabled). | **YES** |

## Database Settings
| Parameter | Description | Required |
| :------- | :------- | :------- |
| `Enabled` | Enables or disables the database functionality. When disabled, no database operations will be performed, maintaining optimal performance. | **YES** |
| `Host` | MySQL database host address. | **NO*** |
| `Port` | MySQL database port (default: 3306). | **NO*** |
| `User` | MySQL database username. | **NO*** |
| `Password` | MySQL database password. | **NO*** |
| `DatabaseName` | Name of the database to use. | **NO*** |

**Required only if `Enabled` is set to `true`*

### Database Features
When the database is enabled, the plugin will automatically store the following information for each notification sent:
- **UUID**: Unique identifier for each notification
- **Server Address**: IP:PORT of the server
- **Connected Players**: Number of players online at the time
- **Max Players**: Server capacity
- **Map Name**: Current map being played
- **Timestamp**: Date and time of the notification
- **Requested By**: Player who triggered the command

This data is stored in the `need_notifications` table and can be used for statistics, analytics, or historical tracking.

## Configuration example
```json
{
  "Commands": {
    "Command": ["css_need", ".need"],
    "CommandCooldownSeconds": 120
  },
  "ServerSettings": {
    "IPandPORT": "45.235.99.18:27025",
    "GetIPandPORTautomatic": true,
    "UseHostname": true,
    "CustomDomain": "https://crisisgamer.com/connect",
    "MaxServerPlayers": 12,
    "GetMaxServerPlayers": true,
    "MinPlayers": 10
  },
  "DiscordSettings": {
    "WebhookUrl": "https://discord.com/api/webhooks/xxxxx/xxxxxxxxx",
    "MentionRoleID": "1111767358881681519",
    "MentionMessage": true,
    "PlayerNameList": true,
    "EmbedSettings": {
      "EmbedColor": "#ffb800",
      "EmbedImage": true,
      "ImagesURL": "https://cdn.jsdelivr.net/gh/wiruwiru/MapsImagesCDN-CS/png/{map}.png",
      "FooterSettings": {
        "EmbedFooter": true,
        "EmbedFooterImage": "https://avatars.githubusercontent.com/u/61034981?v=4"
      },
      "AuthorSettings": {
        "EmbedAuthor": true,
        "EmbedAuthorURL": "https://lucauy.dev",
        "EmbedAuthorImage": "https://avatars.githubusercontent.com/u/61034981?v=4"
      },
      "ThumbnailSettings": {
        "EmbedThumbnail": true,
        "EmbedThumbnailImage": "https://avatars.githubusercontent.com/u/61034981?v=4"
      }
    }
  },
  "PlayerSettings": {
    "NotifyAllPlayers": false,
    "DontCountSpecAdmins": false,
    "AdminBypassFlag": "@css/generic"
  },
  "Database": {
    "Enabled": false,
    "Host": "localhost",
    "Port": 3306,
    "User": "",
    "Password": "",
    "DatabaseName": ""
  }
}
```

# Lang configuration
In the 'lang' folder, you'll find various files. For instance, 'es.json' is designated for the Spanish language. You're welcome to modify this file to better suit your style and language preferences. The language utilized depends on your settings in 'core.json' of CounterStrikeSharp.

# Custom domain configuration
To configure CustomDomain you must first upload the “connect.php” file to your web hosting, after you have done this step you must place the url of this file in the configuration file. It should look like this `https://domain.com/redirect/connect.php` (EXAMPLE URL). In case you don't have a web hosting you can leave the default url.
You can download the **`connect.php`** file directly from here: [Download connect.php](https://raw.githubusercontent.com/wiruwiru/NeedSystem-CS2/main/connect.php). 
> **Note:** Right-click the link and select "Save link as..." to download the file directly.

# Default commands
`!need` `.need` - Send message to Discord
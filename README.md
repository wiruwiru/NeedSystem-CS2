# NeedSystem CS2
Allows players to send a message to discord requesting players.

https://github.com/user-attachments/assets/0bc0217f-4371-44a0-bfc6-36a5531376a7

## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

2. Download [NeedSystem.zip](https://github.com/wiruwiru/NeedSystem-CS2/releases) from the releases section.

3. Unzip the archive and upload it to the game server

4. Start the server and wait for the config.json file to be generated.

5. Complete the configuration file with the parameters of your choice.

# Config
| Parameter | Description | Required     |
| :------- | :------- | :------- |
| `WebhookUrl` | You must create it in the channel where you will send the notices. |**YES** |
| `IPandPORT` | Replace with the IP address of your server. |**YES** |
| `CustomDomain` | You can replace it with your domain if you want, the connect.php file is available in the main branch  |**YES** |
| `MentionRoleID` | You must have the discord developer mode activated, right click on the role and copy its ID. |**NO** |
| `MentionMessage` | You can use this option to deactivate the mention message completely, with this deactivated only the embed will be sent. |**YES** |
| `MaxServerPlayers` | Maximum number of slots your server has. |**YES** |
| `MinPlayers` | In this case if there are ten or more players connected the command cannot be used. | **YES** |
| `CommandCooldownSeconds` | Command cooldown time in seconds. | **YES** |
| `Command` | You can change the command to be used by the players or add extra commands. | **YES** |
| `EmbedImage` | Enables or disables the map image to be shown in the Embed. | **YES** |
| `EmbedColor` | You can change this to your favorite color, in Hex format. | **YES** |
| `ImagesURL` | Url from where the map images are taken, recommended to use your own url if you use workshop maps. | **YES** |
| `PlayerNameList` | Displays a list of the names and profiles of the users who are logged in at the time the command is sent. | **YES** |
| `EmbedFooter` | You can use this option to disable or enable the embed footer. | **YES** |
| `EmbedFooterImage` | It will be the image (logo) that will appear in the embed footer. | **YES** |
| `EmbedAuthor` | You can use this option to disable or enable the embed author. | **YES** |
| `EmbedAuthorURL` | This will be the url that will be redirected to when a user clicks on the embed author. | **YES** |
| `EmbedAuthorImage` | It will be the image (logo) that will appear as the author of the embed. | **YES** |

## Configuration example
```
{
    "WebhookUrl": "https://discord.com/api/webhooks/xxxxx/xxxxxxxxx,
    "IPandPORT": "45.235.99.18:27025",
    "CustomDomain": "https://crisisgamer.com/redirect/connect.php",
    "MentionRoleID": "1111767358881681519",
    "MaxServerPlayers": 12,
    "MinPlayers": 10,
    "CommandCooldownSeconds": 120,
    "Command": [ "css_need", ".need" ],
    "EmbedImage": true,
    "EmbedColor": "#ffb800",
    "ImagesURL": "https://imagenes.redage.es/CS2/{map}.png",
    "PlayerNameList": true,
    "EmbedFooter": false,
    "EmbedFooterImage": "https://avatars.githubusercontent.com/u/61034981?v=4",
    "EmbedAuthor": false,
    "EmbedAuthorURL": "https://lucauy.dev",
    "EmbedAuthorImage": "https://avatars.githubusercontent.com/u/61034981?v=4"
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

## TO-DO
- [x] Change configuration file location
- Any improvement you propose to me that you feel would be a good option
# MediaRichPresence

High configurable .NET 6 application which shares your activity from plex and/or emby in discord!

## Features

 - Plex
	 - Movies
	 - Shows
	 - Live TV with program name
 - Emby
	 - Movies
	 - Shows
	 - LiveTV with program name

> All implementations will show the remaining time of your current playback and also if your playback is paused or not.
> The Logo of the show, movie or live tv channel will also be shown in your discord activity.

The dev branch currently only supports movies and shows for Plex and Emby. For Emby it also supports Live TV. More is coming soon and when all is included there will be a release (+ tutorial)
## To-Do
- Add Plex Live TV support
- Check possibility to add Recordings, Music and audiobooks
- Add the possibility to remove the Provider prefix in the rich presence
- add the possibility to disable certain activity types from being shared
- Better documentation with installation guide and all relevant information
- Add a ready to launch executable as a release
- Add an option to disable rich presence between certain hours
- Add tray icon with a possible configuration window
- Add show and hide console to the tray menu (default will be hidden)

## Example Config
```json
{
  "RichPresence": {
    "RefreshConfigOnEveryCheck": false, //Should be disabled for default setup, could be helpful to test some settings
    "ShowTimeLeftIfPossible": true, //If true it will try to show how much time of the playback is left, if false it will show how long the current rich presence is set
    "RefreshIntervalInSeconds": 3,
    "PriorityMode": 0, //0 = Plex first, than Emby | 1 = Emby first, than Plex
    "WatchingTV": "Watching TV",
    "WatchingMovie": "Watching a movie",
    "WatchingShow": "Watching a show",
    "WatchingUnknown": "Watching something",
    "Paused": "Paused", //Small icon tooltip,
    "Playing": "Playing" //Small icon tooltip
  },
  "Discord": {
    "ApplicationId": "DiscordApplicationId"
  },
  "Plex": {
    "Enabled": true,
    "Url": "http://plex.exmaple.com:32400",
    "ProfileName": "UsedProfileName",
    "AuthToken": "PlexAuthToken",
    "HiddenLibraries": []
  },
  "Emby": {
    "Enabled": true,
    "Url": "http://emby.example.com:8096",
    "ProfileName": "UsedProfileName",
    "EpgHourOffset": 2, //EpgOffset
    "ApiKey": "EmbyApiKey",
    "HiddenLibraries": []
  },
  "Images": {
    //Set both false to disable images and display the image of the provider
    "UseProviderImageLinks": false, //If true the program will use the direct links from plex or emby, could make problems with authentication and security -> It is not recommended
    "UseProviderImageLinksAsFallback": true, //If false only the provider logos will be shown when imgur does not work (Rate Limitation or Downtime)
    "UseImgur": true,
    "ImgurClientId": "ImgurApiKey"
  },
  "ImageTemplateLinks": {
    "Playing": "https://i.imgur.com/cK2Tn8l.png",
    "Paused": "https://i.imgur.com/VomKC7b.png",
    "Plex": "https://i.imgur.com/Zji9d94.png", //Will be used as the rich presence logo if DisablePosters is true
    "Emby": "https://i.imgur.com/vLQYhPk.png" //Will be used as the rich presence logo if DisablePosters is true
  }
}
```

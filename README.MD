# qBittorrent Companion (QBC)

## What is QBC?
qBittorrent Companion (QBC) is a companion app to qBittorrent itself. It will not function without a network accessible qBittorrent 'host'. 

Like the WebUI, QBC provides remote access. But it aims to outdo the WebUI by offering features like:
* Launching files or directories (if they're on the device or a network share)
* Drag-and-drop support for `.torrent` files
* Multi-tab search with support for simultaneous queries
  * Includes a `local mode` bypassing the need for qBittorrent to function as a middleman and slowing things down
* Hotkey navigation
* Automatic light/dark theme matching based on OS preference
    * The splash of color in the UI is based on your system (<abbr title="Operating System">OS</abbr>) accent color (green in the screenshots)
* Live previewing of RSS rule results 
* Editing torrent trackers (with some limitations[^1])
* A real-time line graph of download &amp; upload speeds

But wait! There's more. QBC introduces some features unique to itself:
* RSS rule generation from file/torrent names via a plugin-based system[^2]
* Support for test data in RSS rules[^3] (useful when the target content isn’t currently in the feed)
* Built-in search plugin manager
  * Add or remove local search plugins without ever leaving the app
* Transfers tab filtering without the sidebar
  * A dropdown in the 'Name' field exposes all filter options
* Syntax highlighting for regular expressions used in RSS rules
* Pausable peer updates (prevents accidentally selecting the wrong peer)
* Customizable application icon (Set the colors to match your environment or to increase contrast)

[^1]: Due to limitations of the WebUI all trackers will be placed on the same tier if anything is editted.
[^2]: RSS plugin support: Automatically create RSS rules from torrent names, accessible throughout the app. Manual input is possible as well.
[^3]: The use is case is that you might want to match a specific text that isn't currently part of the feed. By adding it to the test data you can preview for yourself all within the app if a match would be made or not.

![preview image](https://i.imgur.com/uoMtuEA.gif)

>[!NOTE]
> *Dedicated means it runs on your local system. However, a 'host' to connect to that runs qBittorrent with the WebUI enabled is still required.

> [!CAUTION]
> qBittorrent Companion is still considered to be an Alpha version. Don't expect it to be perfect, errors and bugs may and probably will be part of the experience.

## Installation
**Short version:** Grab [the executable](https://github.com/Axeia/qBittorrentCompanion/releases/), unzip it, *run it.<br/>
<sub>*Linux users will have to run `chmod +x qBittorrentCompanion.Desktop` before being able to run it</sub>

**Long Version:**
1. Install qBittorrent on your 'server' PC. qBittorrent Companion requires logging into a qBittorrent WebUI.
2. Set up qBittorrent and enable the WebUI. Test it on the server by visiting http://127.0.0.1:8080 (default port is 8080; adjust the URL if you changed the port).
If it works, try accessing http://your.server.ip.address:8080 from the device you intend to run qBittorrent Companion on.
3. Download the most recent [release](https://github.com/Axeia/qBittorrentCompanion/releases/) of qBittorrent Companion. 
Unzip it to your desired location (no installer available yet).
4. Run it and follow the instructions under the help tab of the login window if you have trouble figuring out how to proceed.

    | Windows                              | Linux                                |
    | ------------------------------------ | ------------------------------------ |
    | Run qBittorrentCompanion.desktop.exe | run `chmod +x qBittorrent.Companion` |
    |                                      | run `.\qBittorrent.Companion`        |

>[!NOTE]
> Make sure your firewall if you're using one is forwarding the relevant port (8080 by default, but customizable in qBittorrents settings)

## Known Issues / Limitations
* Under the `Transfers` tab when selecting a torrent and clicking the `Content` (sub-)tab the content is displayed, but rendering glitches happen (only when the content is big enough to require a scrollbar). You (and I) might have to wait for Avalonia (the UI framework this app uses) to get updated to get it fixed.
* Under the `RSS rules` tab the previews are done using regular expressions in C# rather than in Perl (which qBittorrent itself uses). This could lead to decrepencies although I haven't seen any yet.
* **Preferences Window**<br/>
  Although you can save settings, the input fields don't have many restrictions on them yet. This likely allows you to input erroneous information which might lead to it not saving or not saving properly.

  Fine tuning this will take time and is low priority for now. Feel free to submit bug reports regarding this and I'll get things fixed.

## Planned Features
* Monitoring a local folder for .torrent files to automatically start downloading

## Developer Guide
Everything below is aimed more at developers wondering why some things are the way they are and is not aimed at regular users (unless you're just super curious).

## Build command
So far only Windows & Linux are supported - however Android support will likely be added some point. <br/>
iOS/Mac users might be out of luck. Although technically possible actually doing so is hard for someone not in the ecosystem.
### Windows ⊞<br/>
```dotnet publish ./qBittorrentCompanion.Desktop/qBittorrentCompanion.Desktop.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true```

### Linux 🐧<br/>
```dotnet publish ./qBittorrentCompanion.Desktop/qBittorrentCompanion.Desktop.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true```<br/>
<br/>
Don't forget to run `chmod +x qBittorrentCompanion.Desktop` to make it executable

## License
* This project is released under the MIT license by Pascal Bakhuis
    * The nova3 folder and its contents are the one exception to this, it falls under the GPLv3 license (unless specified otherwise in the file itself) as it's from the qBittorrent project (without any modifications)

## Credits
* qBittorrent Companion: Pascal Bakhuis 
* qBittorrent.Client: [Pavel Fedarovich](https://github.com/fedarovich/qBittorrent-net-client)
* UI:  [Avalonia](https://avaloniaui.net/) (and [fluent icons](https://github.com/davidxuang/FluentIcons))
* Flag icons: [flagpedia.net](https://flagpedia.net/) (displayed in the `Peers` tab when selecting a torrent)
* JSON: [Newtonsoft's JSON.net](https://www.newtonsoft.com/json) (used for [re]storing settings and some JSON tasks)

And of course [qBittorrent(.org)](https://www.qBittorrent.org/) itself. 
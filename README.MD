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
    * The splash of color in the UI is based on your (<abbr title="Operating System">OS</abbr>) accent color (green in the screenshots)
* Live previewing of RSS rule results 
* Editing torrent trackers (with some limitations[^1])
* A real-time line graph of download &amp; upload speeds

<span title="Yes that's a Billy Mays reference">But wait! There's more</span>. QBC has features not even found in qBittorrent itself:
* RSS rule generation from file/torrent names via a plugin-based system[^2]
* Support for test data in RSS rules (useful when the target content isn’t currently in the feed)
* Built-in search plugin manager
  * Add or remove local search plugins without ever leaving the app
* Transfers tab filtering without the sidebar
  * A dropdown in the 'Name' field exposes all filter options
* Syntax highlighting for regular expressions used in RSS rules
* Peer updates can be paused (prevents them from jumping around causing selection mishaps)
* Customizable application icon (Set the colors to match your environment or to increase contrast)

[^1]: Due to limitations of the WebUI all trackers will be placed on the same tier if anything is editted.
[^2]: RSS plugin support: Automatically create RSS rules from torrent names, accessible throughout the app. Manual input is possible as well.

![preview image](https://i.imgur.com/uoMtuEA.gif)

>[!NOTE]
> *Dedicated means it runs on your local system. However, a 'host' to connect to that runs qBittorrent with the WebUI enabled is still required.

> [!CAUTION]
> qBittorrent Companion is still in very early and active development. Expect bugs and hard crashes and please create a github issue when you encounter them.

## Installation
**Short version:** Grab [the executable](https://github.com/Axeia/qBittorrentCompanion/releases/), unzip it, *run it.<br/>
<sub>*Linux users will have to run `chmod +x qBittorrentCompanion.Desktop` before being able to run it</sub>

**Long Version:**
1. Install qBittorrent on your 'server' PC. qBittorrent Companion requires logging into a qBittorrent WebUI.
2. Set up qBittorrent and enable the WebUI. Test it on the server by visiting http://127.0.0.1:8080 (default port is `8080`; adjust the URL if you changed the port).
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
* **`Transfers` tab**<br/>
  The `Content` (sub-)tab has some rendering glitches, they only happen when there's enough content for a vertical scrollbar. The size calculations aren't what they should be and the column widths might jump around. I don't think this can be fixed by me but rather requires Avalonia's TreeDataGrid to get updated.
* **`RSS rules` tab**<br/> 
  Result previews are done differently from qBittorrent. This could lead to decrepencies although I haven't seen any yet. (QBC uses C#'s regex implementation, qBittorrent uses PERL)
* **Preferences Window**<br/>
  Very little testing has been and it's very low priority to me personally. I expect it to be more non-functional than functional. Feel free to submit bug reports regarding this and I'll get things fixed.
* **RSS feeds**<br/>
  In the details pane the description displayed uses HTML formatting. This could have security implications so I only want to support the most basic of HTML and have written my own `SimpleHtmlTextBlock` as I couldn't find an existing control doing so.

  As HTML can get complex and it's impossible to test all cases this might not render things as expected for certain feeds. Let me know if you run into issues by creating an issue on GitHub.

## Planned Features
* Monitoring a local folder for .torrent files to automatically start downloading
* Support for the Android operating system

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
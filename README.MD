# qBittorrent Companion (QBC)

## What is QBC?
qBittorrent Companion (QBC) is a companion app to qBittorrent itself. It will not function without a network accessible qBittorrent 'host'. 
See it as an alternative to the WebUI but as an application rather than a webpage.
![preview image](https://images2.imgbox.com/bf/9c/l5fdCkeA_o.gif)
## Features
QBC should do everything you are able to do in the WebUI (create an issue if you're missing something). QBC however tries to feature match qBittorrent itself and restores these features you may have found the WebUI lacking:
* Monitoring directories for `.torrent` files
* Drag-and-drop support for `.torrent` files
* Multi-tab search with support for simultaneous queries
* Hotkey navigation
* Light/dark mode (matches the OS) - certain third party WebUI themes support this as well to be fair
* Live previewing of RSS rule results 
* Editing torrent trackers (although with caviats due to <abbr title="Application Programming Interface">API</abbr> limitations[^1])
* A real-time line graph of download &amp; upload speeds
* Rather than closing QBC keeps running in the system tray (with an option to close it from there of course)

### Unique features
<span title="Yes that's a Billy Mays reference">But wait! There's more</span>. QBC has features not even found in qBittorrent itself:
* RSS rule generation from file/torrent names via a plugin-based system[^2]
* Support for test data in RSS rules (useful when the target content isn’t currently in the feed)
* Built-in search plugin manager
  * Search plugins are sourced from the [qBittorrent wiki](https://github.com/qbittorrent/search-plugins/wiki/Unofficial-search-plugins) and presented in a way where you can simply install them with the click of a button
  * Search plugins can optionally be installed 'on device'. QBC uses the same plugin system as qBittorrent so the results should be the same, just presented a lot faster as they're not proxied through qBittorrent first.
  <br/>*Local searches require python to be installed*
* Filtering torrents on certain properties without having to sacrifice space to a sidebar (sidebar is still the default)
  * A dropdown accessible by clicking a button offers the same filter options as the sidebar
* ~~Syntax highlighting for regular expressions used in RSS rules~~<br/>Currently broken due to a problem with a NuGet package
* Peer updates can be paused (prevents them from jumping around causing selection mishaps)
* Customizable application icon (Set the colors to match your environment or to increase contrast)

<Details>
<Summary>Warning for local search plugin use:</Summary>
As local searches are <b>not proxied</b> through qBittorrent, search and search results data is send between your QBC device and the search plugin site.
<br/>This may have legal implications, especially if your QBC and qBittorrent devices are in different regions or countries.
</Details>

[^1]: Due to limitations of the WebUI all trackers will be placed on the same tier if anything is editted.
[^2]: RSS plugin support: Automatically create RSS rules from torrent names, accessible throughout the app. Manual input is possible as well.

> [!CAUTION]
> qBittorrent Companion is Alpha software. It may crash or behave unpredictably.

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
  In the details pane the description displayed uses HTML formatting. To keep thing as safe as possible I opted to only support a subset of HTML.
  If this leads to problems for you please create a GitHub issue with a link to the RSS Feed.

## Planned Features
* Support for the Android operating system

## Developer Guide
Everything below is aimed more at developers wondering why some things are the way they are and is not aimed at regular users (unless you're just super curious).

## Build command
So far only Windows & Linux are supported - however Android support will likely be added some point. <br/>
iOS/Mac users might be out of luck. The tech stack supports it, but Apple makes it very hard for people not buying their devices to develop for it.
<br/>*If anyone feels like donating a MacBook or Mac Mini, I'll do my best ;)*

### Windows ⊞<br/>
```dotnet publish ./qBittorrentCompanion.Desktop/qBittorrentCompanion.Desktop.csproj -c Release -r win-x64 -f net8.0-windows --self-contained```

### Linux 🐧<br/>
```dotnet publish ./qBittorrentCompanion.Desktop/qBittorrentCompanion.Desktop.csproj -c Release -r linux-x64 -f net8.0 --self-contained```<br/>
<br/>
>[!NOTE]
> Don't forget to run `chmod +x qBittorrentCompanion.Desktop` to make it executable

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
# qBittorrent Companion (QBC)

## What is QBC?
qBittorrent Companion like the qBittorrent browser based WebUI provides remote access to [qBittorrent](https://www.qBittorrent.org/). Rather than being browser based
QBC takes a different approach and is a dedicated application.

QBC not only matches the functionality of the WebUI, it comes closer to parity with qBittorrent itself. Like it, QBC can do the following:
* Live previewing of RSS rules 
* Respecting the OS dark/light mode
* Opening files/directories (if they're on an accessible network share)
* Hotkey support
* Drag and drop support 
* Editing trackers for a torrent (Although with some limitations[^1])
* A linegraph displaying recent up and download speed (just click on the statusbar download/upload speed)
* Multiple searches (either remote or local)    
    * Remote searches work like they do in the WebUI using qBittorrent as a proxy
    * Local searches are run 'on device' speeding up the results 
        * The exact same plugin system as qBittorrent is used *(you can literally copy over the plugin .py file and it'll work)*

But why stop there, QBC also has some features not found in qBittorrent itself, such as:
* RSS rule generation through its own unique plugin based system[^2]
* Ability to use test data for RSS rules[^3]
* No need to manually download 'Search Plugin' `.py` files to the correct folder, QBC comes with its own search plugin manager
* A dropdown in the Transfers tab filter field allowing access to special filter options normally hidden once the sidebar is disabled
* Syntax highlighting for RSS rules
* Pausable peer updates to avoid clicking (and potentially banning) the wrong peer by accident
* Application icon that's easily customizable to suit your OS/Theme

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
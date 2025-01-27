# What is  qBittorrent Companion?
qBittorrent Companion like the qBittorrent browser based WebUI provides remote access to [qBittorrent](https://www.qbittorrent.org/). Rather than being browser based
qBittorrent Companion takes a different approach and is a dedicated application allowing you to:

* Launch downloaded files directly from qBittorrent Companion
* Set it as the default app to handle `.torrent` files and `magnet:` links 
* Enjoy a modern UI that matches your OS Dark/Light theme setting

Additionally there's some improvements over the WebUI (and in some cases even over QBittorrent itself):

|                                                              | QBC      | QBittorrent | WebUI |
|--------------------------------------------------------------|----------|-------------|-------|
| RSS Rules matches previewed as you type                      | Yes      | Yes         | No    |
| Add your own RSS test data*                                  | Yes      | No          | No    |
| Edit trackers for a torrent                                  | Partial* | Yes         | No    |
| Status/Category/Tag/Tracker filters whilst sidebar is hidden | Yes      | No          | No    |
| Linegraph displayed recent up and download speed             | Yes      | No          | Yes   |

*The use is case is that you might want to match a specific text that isn't currently part of the feed. By adding it to the test data you can preview for yourself all within the app if a match would be made or not<br/>
**Due to limitations of the WebUI all trackers will be placed on the same tier if anything is editted.

[image](https://i.imgur.com/qBNeqvu.gif)

>[!NOTE]
> *Dedicated means it runs on your local system. However, it still needs to connect to a system running qBittorrent with the WebUI enabled.

> [!CAUTION]
> This is an early alpha release. It is not feature complete yet and bugs are to be expected. The aim is feature parity with  the WebUI and adding a couple of extras.

## How to use
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

## Expected missing functionality/odd behaviour

* Under the `Transfers` tab when selecting a torrent and clicking the `Content` (sub-)tab the content is displayed, but rendering glitches happen. This seems to be due to the used control being virtualised rather than pre-rendering everything, I have yet to find a workaround to fix this. You (and I) might have to wait for Avalonia itself to get updated with a fix.
* Under the `RSS` tab and `RSS Rules` (sub-)tab
	* **Matches**<br/>
	Regular expressions are applied although using C#'s logic rather than PERL as qBittorrent would use. This might get a work-around at some point if it proves problematic
	* **Test Data**<br/>
	Displayed but non-functional
* **Preferences Window**<br/>
  Although you can save settings, the input fields don't have many restrictions on them yet. This likely allows you to input erroneous information which might lead to it not saving or not saving properly.

  Fine tuning this will take time and is low priority for now. Feel free to submit bug reports regarding this and I'll get things fixed.


## Not implemented
qBittorrent Companion should have feature parity with the qBittorrents WebUI. If you find yourself unable to do something that can be done in the WebUI please do create an issue with a feature request.

## New functionalities
Some things I think that can make this better than the WebUI:
### Implemented
* Live preview of RSS Rules
* Configure where the Download/Temporary folders are located from the device qBittorrent Companion runs on (probably a network share) and you can then open torrent contents.
* Drag & drop support (drop a torrent on the application and it will be downloaded)
* Tracker editing is possible (although not full featured due to API limitations).
* Peer updates can be paused and resumed to avoid accidental clicks

### Not implemented
* Testing RSS Rules on test data as RSS feeds might not contain what you want to test on at that moment (partially implemented but non-functional)
* Monitoring a local folder for .torrent files to automatically start downloading
* Generating a RSS rule from a torrents name
* Optionally allowing the search to work locally instead of using qBittorrent as a proxy (should be faster)
* Allowing multiple simultaneous searches 


## Developer notes
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
This project is released under the MIT license by Pascal Bakhuis.

## Credit
* qBittorrent Companion: Pascal Bakhuis 
* QBittorrent.Client: [fedarovich](https://github.com/fedarovich/qbittorrent-net-client)
* UI:  [Avalonia](https://avaloniaui.net/) (and [fluent icons](https://github.com/davidxuang/FluentIcons))
* Flag icons: [flagpedia.net](https://flagpedia.net/) (displayed in the `Peers` tab when selecting a torrent)

And of course [qBittorrent(.org)](https://www.qbittorrent.org/) itself. 
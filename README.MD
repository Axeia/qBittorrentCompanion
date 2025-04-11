# What is  qBittorrent Companion (QBC)?
qBittorrent Companion like the qBittorrent browser based WebUI provides remote access to [qBittorrent](https://www.qBittorrent.org/). Rather than being browser based
QBC takes a different approach and is a dedicated application.

QBC not only matches the functionality of the WebUI, it comes closer to parity with qBittorrent itself. Like it, QBC can do the following:
* Multiple searches 
* Live previewing of RSS rules 
* Respecting the OS dark/light mode
* Opening files/directories (if they're on an accessible network share)
* Hotkey support
* Drag and drop support 
* Editing trackers for a torrent (Although with some limitations[^1])

But why stop there, QBC also has some features not found in qBittorrent itself, such as:
* RSS rule generation through a plugin based system[^2]
* Supplying your own RSS test data for RSS rules[^3]
* The sidebar under 'Transfers' can be hidden but the options stay usable through a dropdown 
* Syntax highlighting for RSS rules
* A linegraph displaying recent up and download speed (just click on the statusbar download/upload speed)
* Peer updates can be paused and resumed to avoid clicking the wrong peer by accident

[^1]: Due to limitations of the WebUI all trackers will be placed on the same tier if anything is editted.
[^2]: RSS plugin support: Automatically create RSS rules from torrent names, accessible throughout the app. Manual input is possible as well.
[^3]: The use is case is that you might want to match a specific text that isn't currently part of the feed. By adding it to the test data you can preview for yourself all within the app if a match would be made or not.

[image](https://pouch.jumpshare.com/preview/S-U622FlBTzfSuDM4uqUixZ2zQV001kuJbNG-B0Gyx7wR8x9dy9fcJO6h1ArjgtdUEkB7alpqK57r97ETaCVvENOKKnpjKaPoyIxmVkhfU0)

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

## Not implemented but likely to get added
* Monitoring a local folder for .torrent files to automatically start downloading
* Optionally allowing the search to work locally instead of using qBittorrent as a proxy (should be faster)

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
* qBittorrent.Client: [fedarovich](https://github.com/fedarovich/qBittorrent-net-client)
* UI:  [Avalonia](https://avaloniaui.net/) (and [fluent icons](https://github.com/davidxuang/FluentIcons))
* Flag icons: [flagpedia.net](https://flagpedia.net/) (displayed in the `Peers` tab when selecting a torrent)

And of course [qBittorrent(.org)](https://www.qBittorrent.org/) itself. 
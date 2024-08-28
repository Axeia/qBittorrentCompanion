# What is  qBittorrent Companion?
qBittorrent Companion like the qBittorrent browser based WebUI provides remote access to [qBittorrent](https://www.qbittorrent.org/). Rather than being browser based
qBittorrent Companion takes a different approach and is a dedicated application allowing you to:

* Launch downloaded files directly from qBittorrent Companion
* Set it as the default app to handle `.torrent` files and `magnet:` links 
* Enjoy a modern UI that match your OS Dark/Light theme setting

Additionally there's some improvements over the WebUI:

* RSS Rule results are previewed live as you type them

![image](https://i.imgur.com/qBNeqvu.gif)

>[!NOTE]
> *Dedicated means it runs on your local system. However, it still needs to connect to a system running qBittorrent with the WebUI enabled.

> [!CAUTION]
> This is an early alpha release. It is not feature complete yet and buggy behaviour is to be expected.

## How to use
**Short version:** Grab [the executable](https://github.com/Axeia/qBittorrentCompanion/releases/), unzip it, run it.

**Long Version:**
1. Install qBittorrent on your 'server' PC. qBittorrent Companion requires logging into a qBittorrent WebUI.
2. Set up qBittorrent and enable the WebUI. Test it on the server by visiting http://127.0.0.1:8080 (default port is 8080; adjust the URL if you changed the port).
If it works, try accessing http://your.server.ip.address:8080 from the device you intend to run qBittorrent Companion on.
3. Download the most recent release of qBittorrent Companion. Newer versions are recommended.
Unzip it to your desired location (no installer available yet).
4. Run qBittorrentCompanion.desktop.exe.
>[!NOTE]
> Make sure your firewall if you're using one is forwarding the relevant port (8080 by default, but customizable in qBittorrents settings)

## Expected missing functionality/odd behaviour
Most functionality not found in menus or through shortcuts is implemented, however some of it might not work as expected.
* Under the `Transfers` tab when selecting a torrent and clicking the `Content` (sub-)tab the content is displayed, but:
    * The priority control does nothing yet
    * Displayed sizes get misscalculated when scrolling 
* Under the `RSS` tab and `RSS Rules` (sub-)tab
	* **Matches**<br/>
	Regular expressions are applied although using C#'s logic rather than PERL as qBittorrent would use. This might get a work-around at some point if it proves problematic
	* **Test Data**<br/>
	Displayed but non-functional
* **Preferences Window**<br/>
  Although you can save settings the input fields don't have many restrictions on them yet. This likely allows you to input erroneous information which might lead to it not saving or not saving properly.

  Fine tuning this will take time and is low priority for now. Feel free to submit bug reports regarding this and I'll get things fixed.


## Not implemented
* Anything accessed through right click menus
* Anything other than ⊞ Windows support, I'll probably look into creating an Android and 🐧Linux version. 
Although Mac and iOS should be possible I don't know how feasible it is for me to do so,  I don't own any devices running either OS.

## New functionalities
Some things I think that can make this better than the WebUI:
### Implemented
* Live preview of RSS Rules
* Configure where the Download/Temporary folders are located from the device qBittorrent Companion runs on (probably a network share) and you can then open torrent contents.
* Drag & drop support (drop a torrent on the application and it will be downloaded)

### Not implemented
* Testing RSS Rules on test data as RSS feeds might not contain what you want to test on at that moment (partially implemented but non-functional)
* Monitoring a local folder for .torrent files to automatically start downloading
* Generating a RSS rule from a torrents name
* Optionally allowing the search to work locally instead of using qBittorrent as a proxy (should be faster)
* Allowing multiple simultaneous searches 


## Developer notes
Everything below is aimed more at developers wondering why some things are the way they are and is not aimed at regular users (unless you're just super curious).
### TreeDataGrid
Torrent Content uses a TreeDataGrid and it has some weird glitches (the Avalonia documentation does state this component is in development / considered a beta). I've noticed the file size for folders (which are calculated dynamically by summing up the size of the contents in it) can change when you scroll up and down in the Tree.

## Build command
Desktop build for windows:<br/>
```dotnet publish -p:PublishSingleFile=true -c Release -r win-x64 --self-contained```

## Credit
* qBittorrent Companion: Pascal Bakhuis 
* QBittorrent.Client: [fedarovich](https://github.com/fedarovich/qbittorrent-net-client)
* UI:  [Avalonia](https://avaloniaui.net/) (and [fluent icons](https://github.com/davidxuang/FluentIcons))
* Flag icons: [flagpedia.net](https://flagpedia.net/) (displayed in the `Peers` tab when selecting a torrent)

And of course [qBittorrent(.org)](https://www.qbittorrent.org/) itself. 

## License
This project is released under the MIT license by Pascal Bakhuis.
# What is  qBittorrent Companion?
[qBittorrent](https://www.qbittorrent.org/) is a torrent client that comes with a WebUI. This project aims to replicate the same functionality but as a local application. Think of it as a remote client, the advantage being that you don't need to sacrifice a browser tab and that you can associate .torrent files and magnet links with it easily.
* Just want to use it? **Short version:** Grab [the executable](https://github.com/Axeia/qBittorrentCompanion/releases/), unzip and run it.<br /> **Long version:** Scroll down to How to use
* Got suggestions or discovered a bug? Use the issue tracker.
* Want to have a fiddle with the code? Scroll down for some potentially useful info.
![image](https://i.imgur.com/qBNeqvu.gif)

> [!CAUTION]
> This is very much in an early alpha state. Expect things to bug out and the UI has weird spacing in places.
## How to use
1. The first step is to grab [qBittorrent](https://www.qbittorrent.org/) itself - qBittorrent Companion can't do anything before logging into a qBittorrent WebUI.
    * It should be installed on your 'server' PC. The one you plan to log in into using this companion app. 
2. Set qBittorrent up to enable the WebUI and figure out where it runs
    * I recommend testing it first on the server itself by visiting http://127.0.0.1:8080 8080 being the default port number, if you changed it - change it in the URL as well.
    	* If that worked then try visiting http://your.server.ip.address:8080  from the device you intend to run qBittorrent Companion on. 
3. Grab the most [recent release of qBittorrent Companion](https://github.com/Axeia/qBittorrentCompanion/releases) - newer is better!
    * Unzip it to where you want it installed (there's no installer yet) 
4. Run `qBittorrentCompanion.desktop.exe`
>[!NOTE]
> Make sure your firewall if you're using one is forwarding the relevant port (8080 by default, but customizable in qBittorrents settings)
```Mermaid
mindmap
  root((".<br/>Connection<br/> "))
  ::icon(fa fa-arrow-down-up-across-line)
    qBittorrent <br/> «remote»
    ::icon(fa fa-globe)
      Downloading <br/> «Leeching»
      Uploading <br/>  «Seeding»
      Monitoring <br/> RSS Feeds
    qBittorrent Companion 
    ::icon(fa fa-house-laptop)
      Opening Torrents
      Displaying information
      Controlling torrents<br/>and RSS Feeds
```


## Implemented vs To Implement
### Implemented 
Current functionality that *should* work:
* **Transfers Tab**
	* Adding torrents via a link, via a file or by 'open with' in the file explorer
	* Displaying & Managing torrents (pausing/resuming/deleting)
	* Filtering torrents (on text or by status)
	* Selected Torrent information - *Displaying fine but changing any properties isn't implemented yet*
		* General 
		* Trackers
		* Peers - *Displaying mostly fine, although a default flag icon might be used a bit too often.*
		* HTTP Sources 
		* Content 
	* Managing categories & tags
* **RSS Tab**
	* **RSS Feeds**
		* Managing RSS feeds
	* **RSS Rules**
		* Managing rules
		* **Matches**
			* Regular expressions are applied although using C#'s logic rather than PERL as qBittorrent would use. This might get changed at some point if it proves problematic
			* Wildcards cannot be implemented the exact same way as in qBittorrent itself because it uses functionality specific to the Qt framework (and this specific functionality is deprecated to boot!)
		* **Test Data**
			* Doesn't work yet
### Not implemented

* Search Tab (just completely blank at the moment)
* Anything accessed through the menus
* Anything other than ⊞ Windows support, I'll probably look into creating an Android and 🐧Linux version. 
Although Mac and iOS should be possible I don't know how feasible it is for me to do so,  I don't own any devices running those.

### New functionalities
Some things I think that can make this better than the WebUI:
#### Implemented
* Live preview of RSS Rules
* Opening .torrent files 

#### Not implemented
* Testing RSS Rules on test data as RSS feeds might not contain what you want to test on at that moment (partially implemented but non-functional)
* Monitoring a local folder for .torrent files to automatically start downloading
* Setting up a folder to be the proxy of the downloads folder
	* e.g. If you have the actual qBittorrent client running on the 'Home Server and access what it downloads over a network share. Tell qBittorrent where this folder is and be able to use "open folder" or just launch files directly from qBittorrent Companion.

## Some rambling about the development
Everything below is aimed more at developers wondering why some things are the way they are and is not aimed at regular users (unless you're just super curious).
### Oddities
There might still be some code lingering around from a very early version where I wrote my own API code, then I found the qBittorrent-net-client project and it was almost a drop in replacement (a couple of small name changes mostly).
<hr/>

The Torrent Content tab uses a list rather than a tree. I don't know if it's due to my limited skills or [Avalonia TreeDataGrid's](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid) *"early beta"* status, but I could not get `ComboBox` working properly - they kept closing on their own even after trying various approaches and even disabling updates entirely. 
Switching over to a `DataGrid` I did eventually get it working*. Obviously `DataGrid` and `TreeDataGrid` aren't the same so it looks and functions a little bit different from qBittorrents own.

**DataGrid seems to have its own problem where setting the source to one `class` and then filling it with other classes inheriting from this class leads to the scroll behaviour breaking (It would just get stuck at the top). 
I got around this by consolidating everything into a single (confusing) class - which isn't good design but at least it works.*

<hr/>
Other oddities would likely be to this being my first C# project in quite some time and might get worked out over time.


## Dependencies
This project uses the following NuGet packages:
* Avalonia
* Avalonia.Diagnostics
* Avalonia.Fonts.Inter
* Avalonia.ReactiveUI
	* I've used it where it makes sense - ViewModels that are simply (or at least mostly) wrappers around a really simple model use dot net's own `PropertyChangedEventHandler`.
* Avalonia.Themes.Fluent
* Avalonia.Controls.DataGrid
* Avalonia.Desktop
* QBittorrent.Client
	* Newtonsoft.Json (used by qBittorrent.Client for handling API requests, but this project uses it to store settings as well)

## Build command
Desktop build for windows:<br/>
```dotnet publish -p:PublishSingleFile=true -c Release -r win-x64 --self-contained```

## License
This project is released under the MIT license by Pascal Bakhuis.

## Credit
* qBittorrent Companion : Pascal Bakhuis 
* QBittorrent.Client: [fedarovich](https://github.com/fedarovich/qbittorrent-net-client)
* UI:  [Avalonia](https://avaloniaui.net/) (and [fluent icons](https://avaloniaui.github.io/icons.html))
* Flag icons: [flagpedia.net](https://flagpedia.net/) (displayed in the `Peers` tab when selecting a torrent)

And of course [qBittorrent(.org)](https://www.qbittorrent.org/)  itself. 
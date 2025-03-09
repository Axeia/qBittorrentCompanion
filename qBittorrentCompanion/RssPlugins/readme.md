# RSS Plugins
QBittorrent Companion supports plugins and by default comes with two:
* SeriesRssPlugin.dll
* FossRssPlugin.dll

The idea is that they write the hard part of a RSS rule for you, the "must match" field. It can be hard to write a proper one that matches just what you want it to and nothing else.

Support for them in QBittorent is basically anywhere where you see a torrents name either through a button or menu item.
* Under the "RSS rules" tab you'll find an input field for the torrent name if you want to type in the name yourself.
* Under the "RSS feeds" tab you can define a default plugin per RSS feed. From then on every time you select the feed the associated plugin will  be selected automatically .


## Plugin details
Plugins are a fairly simple concept, they take in a string and return one.
What they take in is a torrent name and what they return is a regular expression that should match the given name and other names like it. 

## Development
Want to develop your own? There's no documentation at the time of writing this. However copy and pasting the code of the existing ones should provide a solid base to work off.

Once you've written something you'ree happy with - build the project and paste the resulting .dll in the /RssPlugins/ directory and reboot QBittorrent Companion.
# ChanThreadWatch

## Fork of the original ChanThreadWatch.

This project is a fork of the original ChanThreadWatch. All credit goes to the original developper.
You can find the official site here: [https://sites.google.com/site/chanthreadwatch/](https://sites.google.com/site/chanthreadwatch/)

### 1.7.5 (2014-Jan-23):
- **Original project forked**
- Fix for 4chan.org images retrieval.

### 1.7.4 (2012-Sep-30):
- Fix spoiler images not being downloaded.

### 1.7.3 (2012-May-13):
- Fix "Stopped: Unknown error" caused by URL parsing.

### 1.7.2 (2012-May-13):
- Restore Mono compatibility.
- Improve character encoding detection.

### 1.7.1 (2012-May-03):
- Supports 4chan's new HTML.
- HTML post-processing makes most URLs absolute for better display and functionality of saved HTML (e.g. external stylesheet/scripts).
- Supports Krautchan.
- Supports loading on .NET 4.0 runtime (not required; still 2.0 compatible).

### 1.7.0 (2011-Jan-02):
- Added download progress window.
- Fixed broken image paths in HTML with folder renaming.
- Allows 30 second check interval on /b/.
- Allows multiple instances using different settings folders.
- Aborts downloads when stopping thread.
- Detects truncated downloads (hash check already caught this on 4chan).
- Prompts before opening more than 5 folders or URLs.
- Workarounds for several issues in Mono.

### 1.6.0 (2010-Dec-18):
- Replaced URL column with user customizable description.
- Added "Last Image On" and "Added On" columns.
- Made columns sortable.
- Option to rename thread download folders with the description.
- Ability to add threads by dropping a link or favicon.
- Added an application icon.
- Added shortcut key Ctrl+A to select all items.

### 1.5.0 (2010-Dec-05):
- Downloads multiple files within a thread concurrently.
- Workaround for downloads stalling when site is sluggish.
- Saves thread list when updated instead of just on exit.
- Saves stopped threads in thread list.
- Ability to add URL(s) from clipboard with single click.
- Ability to delete stopped threads from disk in the context menu.
- Allows only one instance to run at a time.

### 1.4.3 (2010-Nov-26):
- Download folder in thread list is stored as relative path.

### 1.4.2 (2010-Apr-02):
- Fixed crash with non-4chan threads containing mailto links.
- Enabled auto-scaling to fix text truncation with larger font sizes.
- Enter key is a shortcut for "Add Thread".
- Backs up the page before redownloading in case it 404s in the middle of downloading.
- Fixed non-breaking spaces being converted to spaces when post-processing HTML.

### 1.4.1 (2010-Jan-01):
- Workaround for crash in Mono when program update checking is enabled.

### 1.4.0 (2010-Jan-01):
- Option to download thumbnails and post-process HTML to create a mostly-working thread backup (no external CSS, no embedded images other than thumbnails, etc).
- Fixed handling of special characters in filenames.
- Ability to restart stopped threads in the context menu.

### 1.3.0 (2009-Dec-28):
- Option to verify hash of downloaded images (currently 4chan only).
- Option to save images with original filenames (currently 4chan only).
- Option to automatically check for program updates (disabled by default).
- Various fixes related to URL parsing, file error handling, etc.

### 1.2.3 (2009-Dec-25):
- Restores 4chan and AnonIB support.
- Custom link parsing and other code to allow for automatic downloading of multiple page threads (not implemented for any site at this time as I didn't find any I wanted to support).
### 1.2.2 (2009-Aug-22):
- Download folder can be relative to the executable folder.
- Settings and thread list can be saved in the executable folder instead of the application data folder.
- Detects image wrapper pages and sends referer for better site compatibility.
- Locking is utilized properly when exiting.

### 1.2.1 (2009-May-06):
- Works with HTTPS sites (stupid bug).

### 1.2.0 (2009-May-05):
- Settings are remembered across runs.
- Thread list is remembered across runs, with a prompt at start before reloading.
- Main window is resizable.
- Download location is configurable. Default download location is now in My Documents for limited user account compatibility.
- User Agent is configurable.
- Added context menu for the thread list. Moved Stop and Open Folder buttons there and added new features: Open URL, Copy URL, Check Now, Check Every X Minutes.
- Double-clicking a thread can open its folder or URL (configurable).

### 1.1.3 (2008-Jul-08):
- Restores AnonIB support.

### 1.1.2 (2008-Jun-29):
- Ignores duplicate filenames when creating image URL list; fixes incorrect image count on 4chan.
- Doesn't convert page URL to lowercase; fixes 404 problem when page URL contains uppercase characters.

### 1.1.1 (2008-Jan-16):
- Workarounds for Mono's form scaling problems and HttpWebResponse LastModified bug.

### 1.1.0 (2008-Jan-07):
- Fixed UI sluggishness and freezing caused by accidentally leaving a Sleep inside one of the locks for debugging.
- Supports AnonIB.

### 1.0.0 (2007-Dec-05):
- Initial release.

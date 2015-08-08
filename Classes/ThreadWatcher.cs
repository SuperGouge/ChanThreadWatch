using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace JDP {
    public class ThreadWatcher {
        private const int _maxDownloadTries = 3;

        private static WorkScheduler _workScheduler = new WorkScheduler();

        private WorkScheduler.WorkItem _nextCheckWorkItem;
        private object _settingsSync = new object();
        private bool _isStopping;
        private StopReason _stopReason;
        private Dictionary<long, Action> _downloadAborters = new Dictionary<long, Action>();
        private bool _hasRun;
        private bool _hasInitialized;
        private ManualResetEvent _checkFinishedEvent = new ManualResetEvent(true);
        private ManualResetEvent _reparseFinishedEvent = new ManualResetEvent(true);
        private bool _isWaiting;
        private string _pageURL;
        private string _pageAuth;
        private string _imageAuth;
        private bool _oneTimeDownload;
        private int _checkIntervalSeconds;
        private int _minCheckIntervalSeconds;
        private string _mainDownloadDirectory = Settings.AbsoluteDownloadDirectory;
        private string _threadDownloadDirectory;
        private long _nextCheckTicks;
        private string _description = String.Empty;
        private object _tag;
        private SiteHelper _siteHelper;
        private Dictionary<string, ThreadWatcher> _childThreads = new Dictionary<string, ThreadWatcher>();
        private string _pageID;
        private string _category = String.Empty;
        private bool _autoFollow;

        static ThreadWatcher() {
            // HttpWebRequest uses ThreadPool for asynchronous calls
            General.EnsureThreadPoolMaxThreads(500, 1000);

            // Shouldn't matter since the limit is supposed to be per connection group
            ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;

            // Ignore invalid certificates (workaround for Mono)
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, errors) => true;
        }

        public ThreadWatcher(string pageURL) {
            _pageURL = pageURL;
            _siteHelper = SiteHelper.GetInstance(PageHost);
            _siteHelper.SetURL(PageURL);
            _pageID = _siteHelper.GetPageID();
            _threadName = _siteHelper.GetThreadName();
        }

        public string PageURL {
            get { return _pageURL; }
        }

        public string PageHost {
            get { return (new Uri(PageURL)).Host; }
        }

        public string PageID {
            get { return _pageID; }
        }

        public string ThreadName {
            get { return _threadName; }
        }

        public SiteHelper SiteHelper {
            get { return _siteHelper; }
        }

        public string PageAuth {
            get { return _pageAuth; }
            set { SetSetting(out _pageAuth, value, true, false); }
        }

        public string ImageAuth {
            get { return _imageAuth; }
            set { SetSetting(out _imageAuth, value, true, false); }
        }

        public bool OneTimeDownload {
            get { return _oneTimeDownload; }
            set { SetSetting(out _oneTimeDownload, value, true, false); }
        }

        public bool AutoFollow {
            get { return _autoFollow; }
            set { SetSetting(out _autoFollow, value, true, false); }
        }

        public bool DoNotRename { get; set; }

        public int CheckIntervalSeconds {
            get { lock (_settingsSync) { return _checkIntervalSeconds; } }
            set {
                lock (_settingsSync) {
                    int newCheckIntervalSeconds = (_hasInitialized && value < _minCheckIntervalSeconds) ?
                        _minCheckIntervalSeconds : value;
                    int changeAmount = newCheckIntervalSeconds - _checkIntervalSeconds;
                    _checkIntervalSeconds = newCheckIntervalSeconds;
                    NextCheckTicks += changeAmount * 1000;
                }
            }
        }

        public string MainDownloadDirectory {
            get { lock (_settingsSync) { return _mainDownloadDirectory; } }
        }

        public string ThreadDownloadDirectory {
            get { lock (_settingsSync) { return _threadDownloadDirectory; } }
            set { SetSetting(out _threadDownloadDirectory, value, false, false); }
        }

        private bool ThreadDownloadDirectoryPendingRename {
            get { return ThreadDownloadDirectoryPendingDescriptionRename || ThreadDownloadDirectoryPendingCategoryRename; }
        }

        private bool ThreadDownloadDirectoryPendingDescriptionRename {
            get {
                lock (_settingsSync) {
                    return !String.IsNullOrEmpty(_threadDownloadDirectory) &&
                           Settings.RenameDownloadFolderWithDescription == true &&
                           !String.IsNullOrEmpty(_description) &&
                           !String.Equals(General.GetLastDirectory(_threadDownloadDirectory), General.CleanFileName(_description), StringComparison.Ordinal);
                }
            }
        }

        private bool ThreadDownloadDirectoryPendingCategoryRename {
            get {
                lock (_settingsSync) {
                    return !String.IsNullOrEmpty(_threadDownloadDirectory) &&
                           Settings.RenameDownloadFolderWithCategory == true &&
                           !String.Equals(General.GetLastDirectory(_threadDownloadDirectory), General.CleanFileName(_category), StringComparison.Ordinal);
                }
            }
        }

        public int MillisecondsUntilNextCheck {
            get { return Math.Max((int)(NextCheckTicks - TickCount.Now), 0); }
            set { NextCheckTicks = TickCount.Now + value; }
        }

        private long NextCheckTicks {
            get { lock (_settingsSync) { return _nextCheckTicks; } }
            set {
                lock (_settingsSync) {
                    _nextCheckTicks = value;
                    if (_nextCheckWorkItem != null) {
                        _nextCheckWorkItem.RunAtTicks = _nextCheckTicks;
                    }
                }
            }
        }

        public string Description {
            get { lock (_settingsSync) { return _description; } }
            set {
                lock (_settingsSync) {
                    _description = value;
                }
                if (ThreadDownloadDirectoryPendingDescriptionRename) {
                    TryRenameThreadDownloadDirectory(false);
                }
                if (Settings.RenameDownloadFolderWithParentThreadDescription == true) {
                    foreach (ThreadWatcher descendantThread in RootThread.DescendantThreads.Values) {
                        descendantThread.TryRenameThreadDownloadDirectory(false);
                    }
                }
            }
        }

        public string ParentThreadFormattedDescription {
            get {
                if (ParentThread == null || Settings.RenameDownloadFolderWithParentThreadDescription != true ||
                    (!String.IsNullOrEmpty(Settings.ParentThreadDescriptionFormat) && _description.EndsWith(Settings.ParentThreadDescriptionFormat.Replace("{Parent}", ParentThread.Description))))
                {
                    return String.Empty;
                }
                return Settings.ParentThreadDescriptionFormat.Replace("{Parent}", ParentThread.Description);
            }
        }

        public object Tag {
            get { lock (_settingsSync) { return _tag; } }
            set { lock (_settingsSync) { _tag = value; } }
        }

        public Dictionary<string, ThreadWatcher> ChildThreads {
            get { lock (_settingsSync) { return _childThreads; } }
        }

        public Dictionary<string, ThreadWatcher> DescendantThreads {
            get {
                Dictionary<string, ThreadWatcher> dictionary = new Dictionary<string, ThreadWatcher>();
                foreach (ThreadWatcher childThread in ChildThreads.Values) {
                    if (!dictionary.ContainsKey(childThread.PageID)) dictionary.Add(childThread.PageID, childThread);
                }
                foreach (ThreadWatcher childThread in ChildThreads.Values) {
                    foreach (ThreadWatcher descendantThread in childThread.DescendantThreads.Values) {
                        if (!dictionary.ContainsKey(descendantThread.PageID)) dictionary.Add(descendantThread.PageID, descendantThread);
                    }
                }
                return dictionary;
            }
        }

        public ThreadWatcher ParentThread { get; set; }

        public ThreadWatcher RootThread {
            get {
                ThreadWatcher thread = this;
                while (thread.IsCrossLink) {
                    thread = thread.ParentThread;
                }
                return thread;
            }
        }

        public bool IsCrossLink {
            get { return ParentThread != null; }
        }

        public string Category {
            get { lock (_settingsSync) { return _category; } }
            set {
                lock (_settingsSync) {
                    _category = value;
                }
                if (ThreadDownloadDirectoryPendingCategoryRename) {
                    TryRenameThreadDownloadDirectory(false);
                }
            }
        }

        private void SetSetting<T>(out T field, T value, bool canChangeAfterRunning, bool canChangeWhileRunning) {
            lock (_settingsSync) {
                if (!canChangeAfterRunning && _hasRun) {
                    throw new Exception("This setting cannot be changed after the watcher has run.");
                }
                if (!canChangeWhileRunning && IsRunning) {
                    throw new Exception("This setting cannot be changed while the watcher is running.");
                }
                field = value;
            }
        }

        public void Start() {
            lock (_settingsSync) {
                if (IsRunning) {
                    throw new Exception("The watcher is already running.");
                }
                _isStopping = false;
                _stopReason = StopReason.Other;
                _hasRun = true;
                _hasInitialized = false;
                _nextCheckWorkItem = _workScheduler.AddItem(TickCount.Now, Check, PageHost);
            }
        }

        public void Stop(StopReason reason) {
            bool stoppingNow = false;
            bool checkFinished = false;
            List<Action> downloadAborters = null;
            lock (_settingsSync) {
                if (!IsStopping) {
                    stoppingNow = true;
                    _isStopping = true;
                    _stopReason = reason;
                    _hasRun = true;
                    if (_nextCheckWorkItem != null) {
                        _workScheduler.RemoveItem(_nextCheckWorkItem);
                        _nextCheckWorkItem = null;
                    }
                    checkFinished = _checkFinishedEvent.WaitOne(0, false);
                    if (checkFinished) {
                        _isWaiting = false;
                    }
                    else {
                        lock (_downloadAborters) {
                            downloadAborters = new List<Action>(_downloadAborters.Values);
                        }
                    }
                }
            }
            if (stoppingNow) {
                if (checkFinished) {
                    OnStopStatus(new StopStatusEventArgs(reason));
                }
                else {
                    foreach (Action abortDownload in downloadAborters) {
                        abortDownload();
                    }
                }
            }
        }

        public void BeginReparse() {
            _workScheduler.AddItem(TickCount.Now, Reparse);
        }

        private void Reparse() {
            lock (_settingsSync) {
                _reparseFinishedEvent.Reset();
            }

            List<PageInfo> pageList = new List<PageInfo> {
                new PageInfo {
                    URL = _pageURL
                }
            };

            string threadDir = ThreadDownloadDirectory;
            string imageDir = ThreadDownloadDirectory;
            string thumbDir = Path.Combine(ThreadDownloadDirectory, "thumbs");

            for (int pageIndex = 0; pageIndex < pageList.Count; pageIndex++) {
                PageInfo pageInfo = pageList[pageIndex];
                pageInfo.Path = Path.Combine(threadDir, General.CleanFileName(_threadName) + ((pageIndex == 0) ? String.Empty : ("_" + (pageIndex + 1))) + ".html");
                if (!File.Exists(pageInfo.Path)) continue;

                try {
                    byte[] bytes = File.ReadAllBytes(pageInfo.Path);
                    pageInfo.Encoding = General.DetectHTMLEncoding(bytes, null);
                }
                catch {
                    pageInfo.Encoding = Encoding.UTF8;
                }

                string html;
                try { html = File.ReadAllText(pageInfo.Path); }
                catch { continue; }
                HTMLParser parser = !String.IsNullOrEmpty(html) ? new HTMLParser(html) : null;
                if (parser == null) continue;
                SiteHelper siteHelper = SiteHelper.GetInstance(PageHost);
                siteHelper.SetHTMLParser(parser);
                siteHelper.SetURL(pageInfo.Path);

                OnReparseStatus(new ReparseStatusEventArgs(ReparseType.Page, pageIndex + 1, pageList.Count));

                pageInfo.ReplaceList = new List<ReplaceInfo>();
                List<ThumbnailInfo> thumbs = new List<ThumbnailInfo>();
                List<ImageInfo> images = siteHelper.GetImages(pageInfo.ReplaceList, thumbs, true);
                if (images.Count == 0) continue;
                
                Dictionary<string, DownloadInfo> completedImages = new Dictionary<string, DownloadInfo>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, DownloadInfo> completedThumbs = new Dictionary<string, DownloadInfo>(StringComparer.OrdinalIgnoreCase);

                if (!Directory.Exists(thumbDir)) {
                    try {
                        Directory.CreateDirectory(thumbDir);
                    }
                    catch (Exception ex) {
                        Logger.Log(ex.ToString());
                        continue;
                    }
                }

                foreach (ThumbnailInfo thumb in thumbs) {
                    completedThumbs[thumb.FileName] = new DownloadInfo {
                        FileName = thumb.FileName
                    };
                }

                int maxFileNameLengthBaseDir = General.GetMaximumFileNameLength(imageDir);
                foreach (ImageInfo image in images) {
                    OnReparseStatus(new ReparseStatusEventArgs(ReparseType.Image, completedImages.Count, images.Count));
                    string currentPath = new Uri(image.URL).LocalPath;
                    if (!File.Exists(currentPath)) continue;

                    string saveFileNameNoExtension;
                    string saveExtension;
                    string savePath;
                    bool pathTooLong = false;

                    int maxFileNameLength;
                    if (Settings.SortImagesByPoster == true && !String.IsNullOrEmpty(image.Poster)) {
                        try {
                            Directory.CreateDirectory(Path.Combine(imageDir, image.Poster));
                        }
                        catch (Exception ex) {
                            Logger.Log(ex.ToString());
                            completedImages[image.FileName] = new DownloadInfo {
                                Folder = Settings.SortImagesByPoster == true ? image.Poster : String.Empty,
                                FileName = Path.GetFileName(currentPath)
                            };
                            continue;
                        }
                        maxFileNameLength = General.GetMaximumFileNameLength(Path.Combine(imageDir, image.Poster));
                    }
                    else {
                        maxFileNameLength = maxFileNameLengthBaseDir;
                    }

                    MakeImagePath:
                    if ((Settings.UseOriginalFileNames == true) && !String.IsNullOrEmpty(image.OriginalFileName) && !pathTooLong) {
                        saveFileNameNoExtension = Path.GetFileNameWithoutExtension(image.OriginalFileName);
                        saveExtension = Path.GetExtension(image.OriginalFileName);
                    }
                    else {
                        saveFileNameNoExtension = Path.GetFileNameWithoutExtension(image.FileName);
                        saveExtension = Path.GetExtension(image.FileName);
                    }

                    int iSuffix = 1;
                    string saveFileName;
                    do {
                        savePath = Path.Combine(Path.Combine(imageDir, Settings.SortImagesByPoster == true ? image.Poster : String.Empty), saveFileNameNoExtension + ((iSuffix == 1) ?
                            String.Empty : ("_" + iSuffix)) + saveExtension);
                        saveFileName = Path.GetFileName(savePath);
                        iSuffix++;
                    } while (currentPath != savePath && File.Exists(savePath));

                    if (saveFileName.Length > maxFileNameLength && !pathTooLong) {
                        pathTooLong = true;
                        goto MakeImagePath;
                    }

                    if (String.IsNullOrEmpty(saveFileName)) continue;
                    completedImages[image.FileName] = new DownloadInfo {
                        Folder = Settings.SortImagesByPoster == true ? image.Poster : String.Empty,
                        FileName = saveFileName
                    };

                    try {
                        File.Move(currentPath, savePath);
                        string imageFolder = General.RemoveLastDirectory(currentPath);
                        if (imageFolder != ThreadDownloadDirectory && Directory.GetFiles(imageFolder).Length == 0 && Directory.GetDirectories(imageFolder).Length == 0) {
                            Directory.Delete(imageFolder);
                        }
                    }
                    catch (Exception ex) {
                        Logger.Log(ex.ToString());
                    }
                }
                siteHelper.SetURL(pageInfo.URL);
                Process(pageInfo, siteHelper, threadDir, imageDir, thumbDir, completedImages, completedThumbs);
                OnStopStatus(new StopStatusEventArgs(StopReason));
            }

            lock (_settingsSync) {
                _reparseFinishedEvent.Set();
            }
        }

        public void WaitUntilStopped() {
            WaitUntilStopped(Timeout.Infinite);
        }

        public bool WaitUntilStopped(int timeout) {
            return _checkFinishedEvent.WaitOne(timeout, false);
        }

        public bool WaitReparse(int timeout = Timeout.Infinite) {
            return _reparseFinishedEvent.WaitOne(timeout, false);
        }

        public bool IsRunning {
            get {
                lock (_settingsSync) {
                    return !_checkFinishedEvent.WaitOne(0, false) || _nextCheckWorkItem != null;
                }
            }
        }

        public bool IsWaiting {
            get { lock (_settingsSync) { return _isWaiting; } }
        }

        public bool IsStopping {
            get { lock (_settingsSync) { return _isStopping; } }
        }

        public bool IsReparsing {
            get {
                lock (_settingsSync) {
                    return !_reparseFinishedEvent.WaitOne(0, false);
                }
            }
        }

        public StopReason StopReason {
            get { lock (_settingsSync) { return _stopReason; } }
        }

        public event EventHandler<ThreadWatcher, DownloadStatusEventArgs> DownloadStatus;

        public event EventHandler<ThreadWatcher, EventArgs> WaitStatus;

        public event EventHandler<ThreadWatcher, StopStatusEventArgs> StopStatus;

        public event EventHandler<ThreadWatcher, ReparseStatusEventArgs> ReparseStatus;

        public event EventHandler<ThreadWatcher, EventArgs> ThreadDownloadDirectoryRename;

        public event EventHandler<ThreadWatcher, DownloadStartEventArgs> DownloadStart;

        public event EventHandler<ThreadWatcher, DownloadProgressEventArgs> DownloadProgress;

        public event EventHandler<ThreadWatcher, DownloadEndEventArgs> DownloadEnd;

        public event EventHandler<ThreadWatcher, AddThreadEventArgs> AddThread;

        private void OnDownloadStatus(DownloadStatusEventArgs e) {
            var evt = DownloadStatus;
            if (evt != null)
                try { evt(this, e); }
                catch { }
        }

        private void OnWaitStatus(EventArgs e) {
            var evt = WaitStatus;
            if (evt != null)
                try { evt(this, e); }
                catch { }
        }

        private void OnStopStatus(StopStatusEventArgs e) {
            var evt = StopStatus;
            if (evt != null)
                try { evt(this, e); }
                catch { }
        }

        private void OnReparseStatus(ReparseStatusEventArgs e) {
            var evt = ReparseStatus;
            if (evt != null)
                try { evt(this, e); }
                catch { }
        }

        private void OnThreadDownloadDirectoryRename(EventArgs e) {
            var evt = ThreadDownloadDirectoryRename;
            if (evt != null)
                try { evt(this, e); }
                catch { }
        }

        private void OnDownloadStart(DownloadStartEventArgs e) {
            var evt = DownloadStart;
            if (evt != null)
                try { evt(this, e); }
                catch { }
        }

        private void OnDownloadProgress(DownloadProgressEventArgs e) {
            var evt = DownloadProgress;
            if (evt != null)
                try { evt(this, e); }
                catch { }
        }

        private void OnDownloadEnd(DownloadEndEventArgs e) {
            var evt = DownloadEnd;
            if (evt != null)
                try { evt(this, e); }
                catch { }
        }

        private void OnAddThread(AddThreadEventArgs e) {
            var evt = AddThread;
            if (evt != null)
                try { evt(this, e); }
                catch { }
        }

        private List<PageInfo> _pageList;
        private HashSet<string> _imageDiskFileNames;
        private Dictionary<string, DownloadInfo> _completedImages;
        private Dictionary<string, DownloadInfo> _completedThumbs;
        private int _maxFileNameLength;
        private int _maxFileNameLengthBaseDir;
        private string _threadName;

        private void Check() {
            try {
                SiteHelper siteHelper = SiteHelper.GetInstance(PageHost);

                try {
                    lock (_settingsSync) {
                        _nextCheckWorkItem = null;
                        _checkFinishedEvent.Reset();
                        _isWaiting = false;

                        if (!_hasInitialized) {
                            siteHelper.SetURL(_pageURL);

                            _pageList = new List<PageInfo> {
                                new PageInfo {
                                    URL = _pageURL
                                }
                            };
                            _imageDiskFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            _completedImages = new Dictionary<string, DownloadInfo>(StringComparer.OrdinalIgnoreCase);
                            _completedThumbs = new Dictionary<string, DownloadInfo>(StringComparer.OrdinalIgnoreCase);
                            _maxFileNameLengthBaseDir = 0;

                            if (String.IsNullOrEmpty(_threadDownloadDirectory)) {
                                _threadDownloadDirectory = Path.Combine(
                                    Path.Combine(_mainDownloadDirectory, Settings.RenameDownloadFolderWithCategory == true ? General.CleanFileName(_category) : String.Empty),
                                    General.CleanFileName(String.Format("{0}_{1}_{2}{3}", siteHelper.GetSiteName(), siteHelper.GetBoardName(), _threadName, ParentThreadFormattedDescription)));
                            }
                            if (!Directory.Exists(_threadDownloadDirectory)) {
                                Directory.CreateDirectory(_threadDownloadDirectory);
                            }
                            if (String.IsNullOrEmpty(_description)) {
                                _description = General.CleanFileName(String.Format("{0}_{1}_{2}", siteHelper.GetSiteName(), siteHelper.GetBoardName(), _threadName));
                            }
                            _minCheckIntervalSeconds = siteHelper.IsBoardHighTurnover() ? 30 : 60;
                            _checkIntervalSeconds = Math.Max(_checkIntervalSeconds, _minCheckIntervalSeconds);

                            _hasInitialized = true;
                        }
                    }
                }
                catch (Exception ex) {
                    if (ex is IOException || ex is UnauthorizedAccessException) {
                        Stop(StopReason.IOError);
                        Logger.Log(ex.ToString());
                    }
                    else throw;
                }

                string threadDir = ThreadDownloadDirectory;
                string imageDir = ThreadDownloadDirectory;
                string thumbDir = Path.Combine(ThreadDownloadDirectory, "thumbs");

                Queue<ImageInfo> pendingImages = new Queue<ImageInfo>();
                Queue<ThumbnailInfo> pendingThumbs = new Queue<ThumbnailInfo>();

                foreach (PageInfo pageInfo in _pageList) {
                    // Reset the fresh flag on all of the pages before downloading starts so that
                    // they're valid even if stopping before all the pages have been downloaded
                    pageInfo.IsFresh = false;
                }

                int pageIndex = 0;
                OnDownloadStatus(new DownloadStatusEventArgs(DownloadType.Page, 0, _pageList.Count));
                while (pageIndex < _pageList.Count && !IsStopping) {
                    string saveFileName = General.CleanFileName(_threadName) + ((pageIndex == 0) ? String.Empty : ("_" + (pageIndex + 1))) + ".html";
                    HTMLParser previousParser = null;
                    HTMLParser pageParser = null;

                    PageInfo pageInfo = _pageList[pageIndex];
                    pageInfo.Path = Path.Combine(threadDir, saveFileName);

                    if (File.Exists(pageInfo.Path)) {
                        string previousText = null;
                        try { previousText = File.ReadAllText(pageInfo.Path); }
                        catch { }
                        previousParser = !String.IsNullOrEmpty(previousText) ? new HTMLParser(previousText) : null;
                    }

                    ManualResetEvent downloadEndEvent = new ManualResetEvent(false);
                    DownloadPageEndCallback downloadEnd = (result, content, lastModifiedTime, encoding) => {
                        if (result == DownloadResult.Completed) {
                            pageInfo.IsFresh = true;
                            pageParser = new HTMLParser(content);
                            pageInfo.CacheTime = lastModifiedTime;
                            pageInfo.Encoding = encoding;
                            pageInfo.ReplaceList = (Settings.SaveThumbnails != false) ? new List<ReplaceInfo>() : null;
                        }
                        downloadEndEvent.Set();
                    };
                    DownloadPageAsync(pageInfo.Path, pageInfo.URL, PageAuth, pageInfo.CacheTime, downloadEnd);
                    downloadEndEvent.WaitOne();
                    downloadEndEvent.Close();

                    if (pageParser != null) {
                        siteHelper.SetURL(pageInfo.URL);
                        siteHelper.SetHTMLParser(pageParser);
                        siteHelper.ResurrectDeadPosts(previousParser, pageInfo.ReplaceList);

                        if (AutoFollow) {
                            foreach (string crossLink in siteHelper.GetCrossLinks(pageInfo.ReplaceList, Settings.InterBoardAutoFollow != false)) {
                                SiteHelper crossLinkSiteHelper = SiteHelper.GetInstance((new Uri(crossLink)).Host);
                                crossLinkSiteHelper.SetURL(crossLink);
                                string crossLinkID = crossLinkSiteHelper.GetPageID();
                                if (!RootThread.DescendantThreads.ContainsKey(crossLinkID) && RootThread.PageID != crossLinkID) OnAddThread(new AddThreadEventArgs(crossLink));
                            }
                        }

                        List<ThumbnailInfo> thumbs = new List<ThumbnailInfo>();
                        List<ImageInfo> images = siteHelper.GetImages(pageInfo.ReplaceList, thumbs);
                        if (_completedImages.Count == 0) {
                            foreach (ImageInfo image in images) {
                                for (int iName = 0; iName < 2; iName++) {
                                    string baseFileName = (iName == 0) ? image.OriginalFileName : image.FileName;
                                    string baseFileNameNoExtension = Path.GetFileNameWithoutExtension(baseFileName);
                                    string baseExtension = Path.GetExtension(baseFileName);
                                    int iSuffix = 1;
                                    string fileName;
                                    do {
                                        fileName = baseFileNameNoExtension + ((iSuffix == 1) ? String.Empty :
                                            ("_" + iSuffix)) + baseExtension;
                                        iSuffix++;
                                    } while (_imageDiskFileNames.Contains(fileName));
                                    string path = Path.Combine(Path.Combine(imageDir, Settings.SortImagesByPoster == true ? image.Poster : String.Empty), fileName);
                                    if (File.Exists(path)) {
                                        _imageDiskFileNames.Add(fileName);
                                        _completedImages[image.FileName] = new DownloadInfo {
                                            Folder = Settings.SortImagesByPoster == true ? image.Poster : String.Empty,
                                            FileName = fileName,
                                            Skipped = false
                                        };
                                        break;
                                    }
                                }
                            }
                            foreach (ThumbnailInfo thumb in thumbs) {
                                string path = Path.Combine(thumbDir, thumb.FileName);
                                if (File.Exists(path)) {
                                    _completedThumbs[thumb.FileName] = new DownloadInfo {
                                        FileName = thumb.FileName,
                                        Skipped = false
                                    };
                                }
                            }
                        }
                        foreach (ImageInfo image in images) {
                            if (!_completedImages.ContainsKey(image.FileName)) {
                                pendingImages.Enqueue(image);
                            }
                        }
                        foreach (ThumbnailInfo thumb in thumbs) {
                            if (!_completedThumbs.ContainsKey(thumb.FileName)) {
                                pendingThumbs.Enqueue(thumb);
                            }
                        }

                        string nextPageURL = siteHelper.GetNextPageURL();
                        if (!String.IsNullOrEmpty(nextPageURL)) {
                            PageInfo nextPageInfo = new PageInfo {
                                URL = nextPageURL
                            };
                            if (pageIndex == _pageList.Count - 1) {
                                _pageList.Add(nextPageInfo);
                            }
                            else if (_pageList[pageIndex + 1].URL != nextPageURL) {
                                _pageList[pageIndex + 1] = nextPageInfo;
                            }
                        }
                        else if (pageIndex < _pageList.Count - 1) {
                            _pageList.RemoveRange(pageIndex + 1, _pageList.Count - (pageIndex + 1));
                        }
                    }

                    pageIndex++;
                    OnDownloadStatus(new DownloadStatusEventArgs(DownloadType.Page, pageIndex, _pageList.Count));
                }

                MillisecondsUntilNextCheck = CheckIntervalSeconds * 1000;

                if (pendingImages.Count != 0 && !IsStopping) {
                    if (_maxFileNameLengthBaseDir == 0) {
                        _maxFileNameLengthBaseDir = General.GetMaximumFileNameLength(imageDir);
                    }

                    List<ManualResetEvent> downloadEndEvents = new List<ManualResetEvent>();
                    int completedImageCount = 0;
                    foreach (KeyValuePair<string, DownloadInfo> item in _completedImages) {
                        if (!item.Value.Skipped) completedImageCount++;
                    }
                    int totalImageCount = completedImageCount + pendingImages.Count;
                    OnDownloadStatus(new DownloadStatusEventArgs(DownloadType.Image, completedImageCount, totalImageCount));
                    while (pendingImages.Count != 0 && !IsStopping) {
                        string saveFileNameNoExtension;
                        string saveExtension;
                        string savePath;
                        ImageInfo image = pendingImages.Dequeue();
                        bool pathTooLong = false;

                        if (Settings.SortImagesByPoster == true && !String.IsNullOrEmpty(image.Poster)) {
                            try {
                                Directory.CreateDirectory(Path.Combine(imageDir, image.Poster));
                            }
                            catch (Exception ex) {
                                Stop(StopReason.IOError);
                                Logger.Log(ex.ToString());
                            }
                            _maxFileNameLength = General.GetMaximumFileNameLength(Path.Combine(imageDir, image.Poster));
                        }
                        else {
                            _maxFileNameLength = _maxFileNameLengthBaseDir;
                        }

                        MakeImagePath:
                        if ((Settings.UseOriginalFileNames == true) && !String.IsNullOrEmpty(image.OriginalFileName) && !pathTooLong) {
                            saveFileNameNoExtension = Path.GetFileNameWithoutExtension(image.OriginalFileName);
                            saveExtension = Path.GetExtension(image.OriginalFileName);
                        }
                        else {
                            saveFileNameNoExtension = Path.GetFileNameWithoutExtension(image.FileName);
                            saveExtension = Path.GetExtension(image.FileName);
                        }

                        int iSuffix = 1;
                        bool fileNameTaken;
                        string saveFileName;
                        do {
                            savePath = Path.Combine(Path.Combine(imageDir, Settings.SortImagesByPoster == true ? image.Poster : String.Empty), saveFileNameNoExtension + ((iSuffix == 1) ?
                                String.Empty : ("_" + iSuffix)) + saveExtension);
                            saveFileName = Path.GetFileName(savePath);
                            fileNameTaken = _imageDiskFileNames.Contains(saveFileName);
                            iSuffix++;
                        } while (fileNameTaken);

                        if (saveFileName.Length > _maxFileNameLength && !pathTooLong) {
                            pathTooLong = true;
                            goto MakeImagePath;
                        }
                        _imageDiskFileNames.Add(saveFileName);

                        HashType hashType = (Settings.VerifyImageHashes != false) ? image.HashType : HashType.None;
                        ManualResetEvent downloadEndEvent = new ManualResetEvent(false);
                        DownloadFileEndCallback onDownloadEnd = (result) => {
                            if (result == DownloadResult.Completed || result == DownloadResult.Skipped) {
                                lock (_completedImages) {
                                    _completedImages[image.FileName] = new DownloadInfo {
                                        Folder = Settings.SortImagesByPoster == true ? image.Poster : String.Empty,
                                        FileName = saveFileName,
                                        Skipped = (result == DownloadResult.Skipped)
                                    };
                                    if (result != DownloadResult.Skipped) {
                                        completedImageCount++;
                                    }
                                    else {
                                        totalImageCount--;
                                    }
                                    OnDownloadStatus(new DownloadStatusEventArgs(DownloadType.Image, completedImageCount, totalImageCount));
                                }
                            }
                            downloadEndEvent.Set();
                        };
                        downloadEndEvents.Add(downloadEndEvent);
                        DownloadFileAsync(savePath, image.URL, ImageAuth, image.Referer, hashType, image.Hash, onDownloadEnd);
                    }
                    foreach (ManualResetEvent downloadEndEvent in downloadEndEvents) {
                        downloadEndEvent.WaitOne();
                        downloadEndEvent.Close();
                    }
                }

                if (Settings.SaveThumbnails != false) {
                    if (pendingThumbs.Count != 0 && !IsStopping) {
                        if (!Directory.Exists(thumbDir)) {
                            try {
                                Directory.CreateDirectory(thumbDir);
                            }
                            catch (Exception ex) {
                                Stop(StopReason.IOError);
                                Logger.Log(ex.ToString());
                            }
                        }

                        List<ManualResetEvent> downloadEndEvents = new List<ManualResetEvent>();
                        int completedThumbCount = 0;
                        foreach (KeyValuePair<string, DownloadInfo> item in _completedThumbs) {
                            if (!item.Value.Skipped) completedThumbCount++;
                        }
                        int totalThumbCount = completedThumbCount + pendingThumbs.Count;
                        OnDownloadStatus(new DownloadStatusEventArgs(DownloadType.Thumbnail, completedThumbCount, totalThumbCount));
                        while (pendingThumbs.Count != 0 && !IsStopping) {
                            ThumbnailInfo thumb = pendingThumbs.Dequeue();
                            string savePath = Path.Combine(thumbDir, thumb.FileName);

                            ManualResetEvent downloadEndEvent = new ManualResetEvent(false);
                            DownloadFileEndCallback onDownloadEnd = (result) => {
                                if (result == DownloadResult.Completed || result == DownloadResult.Skipped) {
                                    lock (_completedThumbs) {
                                        _completedThumbs[thumb.FileName] = new DownloadInfo {
                                            FileName = thumb.FileName,
                                            Skipped = (result == DownloadResult.Skipped)
                                        };
                                        if (result != DownloadResult.Skipped) {
                                            completedThumbCount++;
                                        }
                                        else {
                                            totalThumbCount--;
                                        }
                                        OnDownloadStatus(new DownloadStatusEventArgs(DownloadType.Thumbnail, completedThumbCount, totalThumbCount));
                                    }
                                }
                                downloadEndEvent.Set();
                            };
                            downloadEndEvents.Add(downloadEndEvent);
                            DownloadFileAsync(savePath, thumb.URL, PageAuth, thumb.Referer, HashType.None, null, onDownloadEnd);
                        }
                        foreach (ManualResetEvent downloadEndEvent in downloadEndEvents) {
                            downloadEndEvent.WaitOne();
                            downloadEndEvent.Close();
                        }
                    }

                    if (!IsStopping || StopReason != StopReason.IOError) {
                        foreach (PageInfo pageInfo in _pageList) {
                            if (pageInfo.IsFresh) {
                                Process(pageInfo, siteHelper, threadDir, imageDir, thumbDir, _completedImages, _completedThumbs);
                            }
                        }
                    }
                }

                if (OneTimeDownload) {
                    Stop(StopReason.DownloadComplete);
                }
            }
            catch (Exception ex) {
                Stop(StopReason.Other);
                Logger.Log(ex.ToString());
            }

            if (ThreadDownloadDirectoryPendingRename) {
                TryRenameThreadDownloadDirectory(true);
            }
            lock (_settingsSync) {
                _checkFinishedEvent.Set();
                if (!IsStopping) {
                    _nextCheckWorkItem = _workScheduler.AddItem(NextCheckTicks, Check, PageHost);
                    _isWaiting = MillisecondsUntilNextCheck > 0;
                }
            }
            if (IsStopping) {
                OnStopStatus(new StopStatusEventArgs(StopReason));
            }
            else if (IsWaiting) {
                OnWaitStatus(EventArgs.Empty);
            }
        }

        private void Process(PageInfo pageInfo, SiteHelper siteHelper, string threadDir, string imageDir, string thumbDir, Dictionary<string, DownloadInfo> completedImages, Dictionary<string, DownloadInfo> completedThumbs) {
            HTMLParser htmlParser = siteHelper.GetHTMLParser();
            for (int i = 0; i < pageInfo.ReplaceList.Count; i++) {
                ReplaceInfo replace = pageInfo.ReplaceList[i];
                DownloadInfo downloadInfo = null;
                ThreadWatcher watcher;
                Func<string, string> getRelativeDownloadPath = (fileDownloadDir) => {
                    return General.GetRelativeFilePath(Path.Combine(fileDownloadDir, downloadInfo.Path),
                        threadDir).Replace(Path.DirectorySeparatorChar, '/');
                };
                if (replace.Type == ReplaceType.ImageLinkHref && completedImages.TryGetValue(replace.Tag, out downloadInfo)) {
                    replace.Value = "href=\"" + General.HtmlAttributeEncode(getRelativeDownloadPath(imageDir), false) + "\"";
                }
                if (replace.Type == ReplaceType.ImageSrc && completedThumbs.TryGetValue(replace.Tag, out downloadInfo)) {
                    replace.Value = "src=\"" + General.HtmlAttributeEncode(getRelativeDownloadPath(thumbDir), false) + "\"";
                }
                if (RootThread.DescendantThreads.TryGetValue(replace.Tag, out watcher)) {
                    if (watcher._hasInitialized) {
                        switch (replace.Type) {
                            case ReplaceType.QuoteLinkHref:
                                replace.Value = "href=\"" + HttpUtility.HtmlAttributeEncode(General.GetRelativeFilePath(Path.Combine(watcher.ThreadDownloadDirectory, General.CleanFileName(watcher.ThreadName) + ".html"), _threadDownloadDirectory)) + "\"";
                                break;
                            case ReplaceType.DeadLink:
                                string[] tagSplit = replace.Tag.Split('/');
                                string innerHTML = String.Format(">>{0}{1}", siteHelper.GetBoardName() != tagSplit[1] ? ">/" + tagSplit[1] + "/" : String.Empty, tagSplit[2]);
                                replace.Value = "<a class=\"quotelink\" href=\"" + HttpUtility.HtmlAttributeEncode(General.GetRelativeFilePath(Path.Combine(watcher.ThreadDownloadDirectory, General.CleanFileName(watcher.ThreadName) + ".html"), _threadDownloadDirectory)) + "\">" + innerHTML + "</a>";
                                break;
                        }
                    }
                    else {
                        pageInfo.ReplaceList.RemoveAt(i--);
                    }
                }
            }
            General.AddOtherReplaces(htmlParser, pageInfo.URL, pageInfo.ReplaceList);
            using (StreamWriter sw = new StreamWriter(pageInfo.Path, false, pageInfo.Encoding)) {
                General.WriteReplacedString(htmlParser.PreprocessedHTML, pageInfo.ReplaceList, sw);
            }
            if (htmlParser.FindEndTag("html") != null && File.Exists(pageInfo.Path + ".bak")) {
                try { File.Delete(pageInfo.Path + ".bak"); }
                catch { }
            }
        }

        private void TryRenameThreadDownloadDirectory(bool calledFromCheck) {
            bool renamedDir = false;
            lock (_settingsSync) {
                if ((!calledFromCheck && !_checkFinishedEvent.WaitOne(0, false)) ||
                    !_reparseFinishedEvent.WaitOne(0, false) ||
                    String.IsNullOrEmpty(_threadDownloadDirectory) ||
                    String.IsNullOrEmpty(_description) ||
                    (IsStopping && (StopReason == StopReason.IOError || StopReason == StopReason.Exiting)))
                {
                    return;
                }
                try {
                    if (DoNotRename) return;
                    string destDir = Path.Combine(
                        Path.Combine(_mainDownloadDirectory, Settings.RenameDownloadFolderWithCategory == true ? General.CleanFileName(_category) : String.Empty),
                        General.CleanFileName(_description + ParentThreadFormattedDescription));
                    if (String.Equals(destDir, _threadDownloadDirectory, StringComparison.Ordinal)) return;
                    if (String.Equals(destDir, _threadDownloadDirectory, StringComparison.OrdinalIgnoreCase)) {
                        Directory.Move(_threadDownloadDirectory, destDir + " Temp");
                        _threadDownloadDirectory = destDir + " Temp";
                        renamedDir = true;
                    }
                    if (!Directory.Exists(General.RemoveLastDirectory(destDir))) Directory.CreateDirectory(General.RemoveLastDirectory(destDir));
                    Directory.Move(_threadDownloadDirectory, destDir);
                    string categoryPath = General.RemoveLastDirectory(_threadDownloadDirectory);
                    if (categoryPath != MainDownloadDirectory && Directory.GetFiles(categoryPath).Length == 0 && Directory.GetDirectories(categoryPath).Length == 0) {
                        try { Directory.Delete(categoryPath); }
                        catch { }
                    }
                    _threadDownloadDirectory = destDir;
                    renamedDir = true;
                }
                catch (Exception ex) {
                    Logger.Log(ex.ToString());
                }
            }
            if (renamedDir) {
                OnThreadDownloadDirectoryRename(EventArgs.Empty);
            }
        }

        private void DownloadPageAsync(string path, string url, string auth, DateTime? cacheLastModifiedTime, DownloadPageEndCallback onDownloadEnd) {
            ConnectionManager connectionManager = ConnectionManager.GetInstance(url);
            string connectionGroupName = connectionManager.ObtainConnectionGroupName();

            string backupPath = path + ".bak";
            int tryNumber = 0;
            long? prevDownloadedFileSize = null;

            Action tryDownload = null;
            tryDownload = () => {
                string httpContentType = null;
                DateTime? lastModifiedTime = null;
                Encoding encoding = null;
                string content = null;

                Action<DownloadResult> endTryDownload = (result) => {
                    connectionManager.ReleaseConnectionGroupName(connectionGroupName);
                    onDownloadEnd(result, content, lastModifiedTime, encoding);
                };

                tryNumber++;
                if (IsStopping || tryNumber > _maxDownloadTries) {
                    endTryDownload(DownloadResult.RetryLater);
                    return;
                }

                long downloadID = (long)(General.BytesTo64BitXor(Guid.NewGuid().ToByteArray()) & 0x7FFFFFFFFFFFFFFFUL);
                FileStream fileStream = null;
                long? totalFileSize = null;
                long downloadedFileSize = 0;
                bool createdFile = false;
                MemoryStream memoryStream = null;
                bool removedDownloadAborter = false;
                Action<bool> cleanup = (successful) => {
                    if (fileStream != null)
                        try { fileStream.Close(); }
                        catch { }
                    if (memoryStream != null)
                        try { memoryStream.Close(); }
                        catch { }
                    if (!successful && createdFile) {
                        try { File.Delete(path); }
                        catch { }
                        if (File.Exists(backupPath)) {
                            try { File.Move(backupPath, path); }
                            catch { }
                        }
                    }
                    lock (_downloadAborters) {
                        _downloadAborters.Remove(downloadID);
                        removedDownloadAborter = true;
                    }
                };

                Action abortDownload = General.DownloadAsync(url, auth, null, connectionGroupName, cacheLastModifiedTime,
                    (response) => {
                        if (File.Exists(path)) {
                            if (File.Exists(backupPath)) {
                                try { File.Delete(backupPath); }
                                catch { }
                            }
                            try { File.Move(path, backupPath); }
                            catch { }
                        }
                        fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                        if (response.ContentLength != -1) {
                            totalFileSize = response.ContentLength;
                            fileStream.SetLength(totalFileSize.Value);
                        }
                        createdFile = true;
                        memoryStream = new MemoryStream();
                        httpContentType = response.ContentType;
                        lastModifiedTime = General.GetResponseLastModifiedTime(response);
                        OnDownloadStart(new DownloadStartEventArgs(downloadID, url, tryNumber, totalFileSize));
                    },
                    (data, dataLength) => {
                        fileStream.Write(data, 0, dataLength);
                        memoryStream.Write(data, 0, dataLength);
                        downloadedFileSize += dataLength;
                        OnDownloadProgress(new DownloadProgressEventArgs(downloadID, downloadedFileSize));
                    },
                    () => {
                        byte[] pageBytes = memoryStream.ToArray();
                        if (totalFileSize != null && downloadedFileSize != totalFileSize) {
                            fileStream.SetLength(downloadedFileSize);
                        }
                        bool incompleteDownload = totalFileSize != null && downloadedFileSize != totalFileSize &&
                                                  (prevDownloadedFileSize == null || downloadedFileSize != prevDownloadedFileSize);
                        if (incompleteDownload) {
                            // Corrupt download, retry
                            prevDownloadedFileSize = downloadedFileSize;
                            throw new Exception("Download is corrupt.");
                        }
                        cleanup(true);
                        OnDownloadEnd(new DownloadEndEventArgs(downloadID, downloadedFileSize, true));
                        encoding = General.DetectHTMLEncoding(pageBytes, httpContentType);
                        content = encoding.GetString(pageBytes);
                        endTryDownload(DownloadResult.Completed);
                    },
                    (ex) => {
                        cleanup(false);
                        OnDownloadEnd(new DownloadEndEventArgs(downloadID, downloadedFileSize, false));
                        if (ex is HTTP304Exception) {
                            // Page not modified, skip
                            endTryDownload(DownloadResult.Skipped);
                        }
                        else if (ex is HTTP404Exception) {
                            // Page not found, stop
                            Stop(StopReason.PageNotFound);
                            endTryDownload(DownloadResult.Skipped);
                        }
                        else if (ex is IOException || ex is UnauthorizedAccessException) {
                            // Fatal IO error, stop
                            Stop(StopReason.IOError);
                            endTryDownload(DownloadResult.Skipped);
                        }
                        else {
                            // Other error, retry
                            connectionGroupName = connectionManager.SwapForFreshConnection(connectionGroupName, url);
                            tryDownload();
                        }
                    });

                lock (_downloadAborters) {
                    if (!removedDownloadAborter) {
                        _downloadAborters[downloadID] = abortDownload;
                    }
                }
            };

            tryDownload();
        }

        private void DownloadFileAsync(string path, string url, string auth, string referer, HashType hashType, byte[] correctHash, DownloadFileEndCallback onDownloadEnd) {
            ConnectionManager connectionManager = ConnectionManager.GetInstance(url);
            string connectionGroupName = connectionManager.ObtainConnectionGroupName();

            int tryNumber = 0;
            byte[] prevHash = null;
            long? prevDownloadedFileSize = null;

            Action tryDownload = null;
            tryDownload = () => {
                Action<DownloadResult> endTryDownload = (result) => {
                    connectionManager.ReleaseConnectionGroupName(connectionGroupName);
                    onDownloadEnd(result);
                };

                tryNumber++;
                if (IsStopping || tryNumber > _maxDownloadTries) {
                    endTryDownload(DownloadResult.RetryLater);
                    return;
                }

                long downloadID = (long)(General.BytesTo64BitXor(Guid.NewGuid().ToByteArray()) & 0x7FFFFFFFFFFFFFFFUL);
                FileStream fileStream = null;
                long? totalFileSize = null;
                long downloadedFileSize = 0;
                bool createdFile = false;
                HashGeneratorStream hashStream = null;
                bool removedDownloadAborter = false;
                Action<bool> cleanup = (successful) => {
                    if (fileStream != null)
                        try { fileStream.Close(); }
                        catch { }
                    if (hashStream != null)
                        try { hashStream.Close(); }
                        catch { }
                    if (!successful && createdFile) {
                        try { File.Delete(path); }
                        catch { }
                    }
                    lock (_downloadAborters) {
                        _downloadAborters.Remove(downloadID);
                        removedDownloadAborter = true;
                    }
                };

                Action abortDownload = General.DownloadAsync(url, auth, referer, connectionGroupName, null,
                    (response) => {
                        fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                        if (response.ContentLength != -1) {
                            totalFileSize = response.ContentLength;
                            fileStream.SetLength(totalFileSize.Value);
                        }
                        createdFile = true;
                        if (hashType != HashType.None) {
                            hashStream = new HashGeneratorStream(hashType);
                        }
                        OnDownloadStart(new DownloadStartEventArgs(downloadID, url, tryNumber, totalFileSize));
                    },
                    (data, dataLength) => {
                        fileStream.Write(data, 0, dataLength);
                        if (hashStream != null) hashStream.Write(data, 0, dataLength);
                        downloadedFileSize += dataLength;
                        OnDownloadProgress(new DownloadProgressEventArgs(downloadID, downloadedFileSize));
                    },
                    () => {
                        byte[] hash = (hashType != HashType.None) ? hashStream.GetDataHash() : null;
                        if (totalFileSize != null && downloadedFileSize != totalFileSize) {
                            fileStream.SetLength(downloadedFileSize);
                        }
                        bool incorrectHash = hashType != HashType.None && !General.ArraysAreEqual(hash, correctHash) &&
                                             (prevHash == null || !General.ArraysAreEqual(hash, prevHash));
                        bool incompleteDownload = totalFileSize != null && downloadedFileSize != totalFileSize &&
                                                  (prevDownloadedFileSize == null || downloadedFileSize != prevDownloadedFileSize);
                        if (incorrectHash || incompleteDownload) {
                            // Corrupt download, retry
                            prevHash = hash;
                            prevDownloadedFileSize = downloadedFileSize;
                            throw new Exception("Download is corrupt.");
                        }
                        cleanup(true);
                        OnDownloadEnd(new DownloadEndEventArgs(downloadID, downloadedFileSize, true));
                        endTryDownload(DownloadResult.Completed);
                    },
                    (ex) => {
                        cleanup(false);
                        OnDownloadEnd(new DownloadEndEventArgs(downloadID, downloadedFileSize, false));
                        if (ex is DirectoryNotFoundException || ex is UnauthorizedAccessException) {
                            // Fatal IO error, stop
                            Stop(StopReason.IOError);
                            endTryDownload(DownloadResult.Skipped);
                        }
                        else if (ex is HTTP404Exception || ex is IOException) {
                            // Fatal problem with this file, skip
                            endTryDownload(DownloadResult.Skipped);
                        }
                        else {
                            // Other error, retry
                            connectionGroupName = connectionManager.SwapForFreshConnection(connectionGroupName, url);
                            tryDownload();
                        }
                    });

                lock (_downloadAborters) {
                    if (!removedDownloadAborter) {
                        _downloadAborters[downloadID] = abortDownload;
                    }
                }
            };

            tryDownload();
        }
    }
}
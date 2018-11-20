using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace JDP {
    public class ReplaceInfo {
        public int Offset { get; set; }
        public int Length { get; set; }
        public string Value { get; set; }
        public ReplaceType Type { get; set; }
        public string Tag { get; set; }
    }

    public class PageInfo {
        public string URL { get; set; }
        public DateTime? CacheTime { get; set; }
        public bool IsFresh { get; set; }
        public string Path { get; set; }
        public Encoding Encoding { get; set; }
        public List<ReplaceInfo> ReplaceList { get; set; }
    }

    public class ImageInfo {
        public string URL { get; set; }
        public string Referer { get; set; }
        public string OriginalFileName { get; set; }
        public HashType HashType { get; set; }
        public byte[] Hash { get; set; }
        public string Poster { get; set; }

        public string FileName {
            get { return General.CleanFileName(General.URLFileName(URL)); }
        }
    }

    public class DownloadInfo {
        public string Folder { get; set; }
        public string FileName { get; set; }
        public bool Skipped { get; set; }

        public string Path {
            get { return System.IO.Path.Combine(Folder ?? String.Empty, FileName); }
        }
    }

    public class ThumbnailInfo {
        public string URL { get; set; }
        public string Referer { get; set; }

        public string FileName {
            get { return General.CleanFileName(General.URLFileName(URL)); }
        }
    }

    public class ThreadInfo {
        public string URL { get; set; }
        public string PageAuth { get; set; }
        public string ImageAuth { get; set; }
        public int CheckIntervalSeconds { get; set; }
        public bool OneTimeDownload { get; set; }
        public string SaveDir { get; set; }
        public string Description { get; set; }
        public StopReason? StopReason { get; set; }
        public WatcherExtraData ExtraData { get; set; }
        public string Category { get; set; }
        public bool AutoFollow { get; set; }
    }

    public class MonitoringInfo {
        public int TotalThreads { get; set; }
        public int RunningThreads { get; set; }
        public int DeadThreads { get; set; }
        public int StoppedThreads { get; set; }
    }

    public class HTTP404Exception : Exception { }

    public class HTTP304Exception : Exception { }

    public static class TickCount {
        private static object _sync = new object();
        private static int _lastTickCount;
        private static long _correction;

        public static long Now {
            get {
                lock (_sync) {
                    int tickCount = Environment.TickCount;
                    if ((tickCount < 0) && (_lastTickCount >= 0)) {
                        _correction += 0x100000000L;
                    }
                    _lastTickCount = tickCount;
                    return tickCount + _correction;
                }
            }
        }
    }

    public class ConnectionManager {
        private const int _maxConnectionsPerHost = 4;

        private static Dictionary<string, ConnectionManager> _connectionManagers = new Dictionary<string, ConnectionManager>(StringComparer.OrdinalIgnoreCase);

        private FIFOSemaphore _semaphore = new FIFOSemaphore(_maxConnectionsPerHost, _maxConnectionsPerHost);
        private Stack<string> _groupNames = new Stack<string>();

        public static ConnectionManager GetInstance(string url) {
            string host = (new Uri(url)).Host;
            ConnectionManager manager;
            lock (_connectionManagers) {
                if (!_connectionManagers.TryGetValue(host, out manager)) {
                    manager = new ConnectionManager();
                    _connectionManagers[host] = manager;
                }
            }
            return manager;
        }

        public string ObtainConnectionGroupName() {
            _semaphore.WaitOne();
            return GetConnectionGroupName();
        }

        public void ReleaseConnectionGroupName(string name) {
            lock (_groupNames) {
                _groupNames.Push(name);
            }
            _semaphore.Release();
        }

        public string SwapForFreshConnection(string name, string url) {
            ServicePoint servicePoint = ServicePointManager.FindServicePoint(new Uri(url));
            try {
                servicePoint.CloseConnectionGroup(name);
            }
            catch (NotImplementedException) {
                // Workaround for Mono
            }
            return GetConnectionGroupName();
        }

        private string GetConnectionGroupName() {
            lock (_groupNames) {
                return _groupNames.Count != 0 ? _groupNames.Pop() : Guid.NewGuid().ToString();
            }
        }
    }

    public class FIFOSemaphore {
        private int _currentCount;
        private int _maximumCount;
        private object _mainSync = new object();
        private Queue<QueueSync> _queueSyncs = new Queue<QueueSync>();

        public FIFOSemaphore(int initialCount, int maximumCount) {
            if (initialCount > maximumCount) {
                throw new ArgumentException();
            }
            if (initialCount < 0 || maximumCount < 1) {
                throw new ArgumentOutOfRangeException();
            }
            _currentCount = initialCount;
            _maximumCount = maximumCount;
        }

        public void WaitOne() {
            WaitOne(Timeout.Infinite);
        }

        public bool WaitOne(int timeout) {
            QueueSync queueSync;
            lock (_mainSync) {
                if (_currentCount > 0) {
                    _currentCount--;
                    return true;
                }
                else {
                    queueSync = new QueueSync();
                    _queueSyncs.Enqueue(queueSync);
                }
            }
            lock (queueSync) {
                if (queueSync.IsSignaled || Monitor.Wait(queueSync, timeout)) {
                    return true;
                }
                else {
                    queueSync.IsAbandoned = true;
                    return false;
                }
            }
        }

        public void Release() {
            lock (_mainSync) {
                if (_currentCount >= _maximumCount) { // Workaround for Mono
                    Type semaphoreException = Type.GetType("System.Threading.SemaphoreFullException");
                    object exception = Activator.CreateInstance(semaphoreException);
                    throw (SystemException)exception;
                }
                CheckQueue:
                if (_queueSyncs.Count == 0) {
                    _currentCount++;
                }
                else {
                    QueueSync queueSync = _queueSyncs.Dequeue();
                    lock (queueSync) {
                        if (queueSync.IsAbandoned) {
                            goto CheckQueue;
                        }
                        // Backup signal in case we acquired the lock before the waiter
                        queueSync.IsSignaled = true;
                        Monitor.Pulse(queueSync);
                    }
                }
            }
        }

        private class QueueSync {
            public bool IsSignaled { get; set; }
            public bool IsAbandoned { get; set; }
        }
    }

    public class WorkScheduler {
        private const int _maxThreadIdleTime = 15000;

        private object _sync = new object();
        private LinkedList<WorkItem> _workItems = new LinkedList<WorkItem>();
        private ManualResetEvent _scheduleChanged = new ManualResetEvent(false);
        private Thread _schedulerThread;

        public WorkItem AddItem(long runAtTicks, Action action) {
            return AddItem(runAtTicks, action, String.Empty);
        }

        public WorkItem AddItem(long runAtTicks, Action action, string group) {
            WorkItem item = new WorkItem(this, runAtTicks, action, group);
            AddItem(item);
            return item;
        }

        private void AddItem(WorkItem item) {
            lock (_sync) {
                LinkedListNode<WorkItem> nextNode = null;
                foreach (LinkedListNode<WorkItem> node in EnumerateNodes()) {
                    if (node.Value.RunAtTicks > item.RunAtTicks) {
                        nextNode = node;
                        break;
                    }
                }
                if (nextNode == null) {
                    _workItems.AddLast(item);
                }
                else {
                    _workItems.AddBefore(nextNode, item);
                }
                _scheduleChanged.Set();
                if (_schedulerThread == null) {
                    _schedulerThread = new Thread(SchedulerThread);
                    _schedulerThread.IsBackground = true;
                    _schedulerThread.Start();
                }
            }
        }

        public bool RemoveItem(WorkItem item) {
            lock (_sync) {
                if (_workItems.Remove(item)) {
                    _scheduleChanged.Set();
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        private void ReAddItem(WorkItem item) {
            lock (_sync) {
                if (RemoveItem(item)) {
                    AddItem(item);
                }
            }
        }

        private void SchedulerThread() {
            while (true) {
                int? firstWaitTime = null;

                lock (_sync) {
                    _scheduleChanged.Reset();
                    if (_workItems.Count != 0) {
                        firstWaitTime = (int)(_workItems.First.Value.RunAtTicks - TickCount.Now);
                    }
                }

                if (!(firstWaitTime <= 0)) {
                    if (_scheduleChanged.WaitOne(firstWaitTime ?? _maxThreadIdleTime, false)) {
                        continue;
                    }
                    if (firstWaitTime == null) {
                        lock (_sync) {
                            if (_workItems.Count != 0) {
                                continue;
                            }
                            else {
                                _schedulerThread = null;
                                return;
                            }
                        }
                    }
                }

                lock (_sync) {
                    while (_workItems.Count != 0 && _workItems.First.Value.RunAtTicks <= TickCount.Now) {
                        _workItems.First.Value.StartRunning();
                        _workItems.RemoveFirst();
                    }
                }
            }
        }

        private IEnumerable<LinkedListNode<WorkItem>> EnumerateNodes() {
            LinkedListNode<WorkItem> node = _workItems.First;
            while (node != null) {
                yield return node;
                node = node.Next;
            }
        }

        public class WorkItem {
            private bool _hasStarted;
            private WorkScheduler _scheduler;
            private long _runAtTicks;
            private Action _action;
            private string _group;

            public WorkItem(WorkScheduler scheduler, long runAtTicks, Action action, string group) {
                _scheduler = scheduler;
                _runAtTicks = runAtTicks;
                _action = action;
                _group = group;
            }

            public long RunAtTicks {
                get { lock (_scheduler._sync) { return _runAtTicks; } }
                set {
                    lock (_scheduler._sync) {
                        if (_hasStarted) return;
                        _runAtTicks = value;
                        _scheduler.ReAddItem(this);
                    }
                }
            }

            public void StartRunning() {
                lock (_scheduler._sync) {
                    if (_hasStarted) {
                        throw new Exception("Work item has already started.");
                    }
                    _hasStarted = true;
                    ThreadPoolManager.QueueWorkItem(_group, _action);
                }
            }
        }
    }

    public class ThreadPoolManager {
        private const int _minThreadCount = 4;
        private const int _threadCreationDelay = 500;
        private const int _maxThreadIdleTime = 15000;

        private static Dictionary<string, ThreadPoolManager> _threadPoolManagers = new Dictionary<string, ThreadPoolManager>(StringComparer.OrdinalIgnoreCase);

        private object _sync = new object();
        private FIFOSemaphore _semaphore = new FIFOSemaphore(0, Int32.MaxValue);
        private Stack<ThreadPoolThread> _idleThreads = new Stack<ThreadPoolThread>();
        private ThreadPoolThread _schedulerThread = new ThreadPoolThread(null);

        public ThreadPoolManager() {
            lock (_sync) {
                for (int i = 0; i < _minThreadCount; i++) {
                    _idleThreads.Push(new ThreadPoolThread(this));
                    _semaphore.Release();
                }
            }
        }

        public static void QueueWorkItem(string group, Action action) {
            ThreadPoolManager manager;
            lock (_threadPoolManagers) {
                if (!_threadPoolManagers.TryGetValue(group, out manager)) {
                    manager = new ThreadPoolManager();
                    _threadPoolManagers[group] = manager;
                }
            }
            manager.QueueWorkItem(action);
        }

        public void QueueWorkItem(Action action) {
            _schedulerThread.QueueWorkItem(() => {
                ThreadPoolThread thread;
                if (_semaphore.WaitOne(_threadCreationDelay)) {
                    lock (_sync) {
                        thread = _idleThreads.Pop();
                    }
                }
                else {
                    thread = new ThreadPoolThread(this);
                }
                thread.QueueWorkItem(action);
                thread.QueueWorkItem(() => {
                    lock (_sync) {
                        _idleThreads.Push(thread);
                        _semaphore.Release();
                    }
                });
            });
        }

        private void OnThreadPoolThreadExit(ThreadPoolThread exitedThread) {
            lock (_sync) {
                if (_idleThreads.Count <= _minThreadCount) return;
                Stack<ThreadPoolThread> threads = new Stack<ThreadPoolThread>();
                while (_idleThreads.Count != 0) {
                    ThreadPoolThread thread = _idleThreads.Pop();
                    if (thread == exitedThread) {
                        if (!_semaphore.WaitOne(0)) {
                            throw new Exception("Semaphore count is invalid.");
                        }
                        break;
                    }
                    threads.Push(thread);
                }
                while (threads.Count != 0) {
                    _idleThreads.Push(threads.Pop());
                }
            }
        }

        private class ThreadPoolThread {
            private object _sync = new object();
            private ThreadPoolManager _manager;
            private Thread _thread;
            private ManualResetEvent _newWorkItem;
            private Queue<Action> _workItems = new Queue<Action>();

            internal ThreadPoolThread(ThreadPoolManager manager) {
                _manager = manager;
            }

            internal void QueueWorkItem(Action action) {
                lock (_sync) {
                    if (_thread == null) {
                        _newWorkItem = new ManualResetEvent(false);
                        _thread = new Thread(WorkThread);
                        _thread.IsBackground = true;
                        _thread.Start();
                    }
                    _workItems.Enqueue(action);
                    _newWorkItem.Set();
                }
            }

            private void WorkThread() {
                while (_newWorkItem.WaitOne(_maxThreadIdleTime, false) || !ReleaseThread()) {
                    Action workItem = null;
                    lock (_sync) {
                        if (_workItems.Count != 0) {
                            workItem = _workItems.Dequeue();
                        }
                        else {
                            _newWorkItem.Reset();
                        }
                    }
                    if (workItem != null) {
                        Thread.MemoryBarrier();
                        workItem();
                        Thread.MemoryBarrier();
                    }
                }
                if (_manager != null) {
                    _manager.OnThreadPoolThreadExit(this);
                }
            }

            private bool ReleaseThread() {
                lock (_sync) {
                    if (_workItems.Count == 0) {
                        _newWorkItem.Close();
                        _newWorkItem = null;
                        _thread = null;
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
        }
    }

    public class HashSet<T> : IEnumerable<T> {
        private Dictionary<T, int> _dict;

        public HashSet() {
            _dict = new Dictionary<T, int>();
        }

        public HashSet(IEqualityComparer<T> comparer) {
            _dict = new Dictionary<T, int>(comparer);
        }

        public HashSet(IEnumerable<T> collection) :
            this()
        {
            AddRange(collection);
        }

        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) :
            this(comparer)
        {
            AddRange(collection);
        }

        public int Count {
            get { return _dict.Count; }
        }

        public bool Add(T item) {
            if (!_dict.ContainsKey(item)) {
                _dict[item] = 0;
                return true;
            }
            return false;
        }

        private void AddRange(IEnumerable<T> collection) {
            foreach (T item in collection) {
                Add(item);
            }
        }

        public bool Remove(T item) {
            return _dict.Remove(item);
        }

        public void Clear() {
            _dict.Clear();
        }

        public bool Contains(T item) {
            return _dict.ContainsKey(item);
        }

        public IEnumerator<T> GetEnumerator() {
            foreach (KeyValuePair<T, int> item in _dict) {
                yield return item.Key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    public class HashGeneratorStream : Stream {
        private HashAlgorithm _hashAlgo;
        private byte[] _dataHash;

        public HashGeneratorStream(HashType hashType) {
            switch (hashType) {
                case HashType.MD5:
                    _hashAlgo = new MD5CryptoServiceProvider();
                    break;
                default:
                    throw new Exception("Unsupported hash type.");
            }
        }

        public override bool CanRead {
            get { return false; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (_hashAlgo == null) {
                throw new Exception("Cannot write after hash has been finalized.");
            }
            _hashAlgo.TransformBlock(buffer, offset, count, null, 0);
        }

        public override void Flush() { }

        public byte[] GetDataHash() {
            if (_hashAlgo != null) {
                _hashAlgo.TransformFinalBlock(new byte[0], 0, 0);
                _dataHash = _hashAlgo.Hash;
                _hashAlgo = null;
            }
            return _dataHash;
        }

        public override long Length {
            get { throw new NotSupportedException(); }
        }

        public override long Position {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }
    }
    
    public class ThrottledStream : Stream {
        public const long Infinite = 0;

        private Stream _baseStream;
        private long _maximumBytesPerSecond;
        private long _byteCount;
        private long _start;

        protected long CurrentMilliseconds {
            get { return Environment.TickCount; }
        }

        public long MaximumBytesPerSecond {
            get { return _maximumBytesPerSecond; }
            set {
                if (MaximumBytesPerSecond != value) {
                    _maximumBytesPerSecond = value;
                    Reset();
                }
            }
        }

        public override bool CanRead {
            get { return _baseStream.CanRead; }
        }

        public override bool CanSeek {
            get { return _baseStream.CanSeek; }
        }

        public override bool CanWrite {
            get { return _baseStream.CanWrite; }
        }

        public override long Length {
            get { return _baseStream.Length; }
        }

        public override long Position {
            get { return _baseStream.Position; }
            set { _baseStream.Position = value; }
        }

        public ThrottledStream(Stream baseStream, long maximumBytesPerSecond = Infinite) {
            if (baseStream == null) {
                throw new ArgumentNullException("baseStream");
            }

            if (maximumBytesPerSecond < 0) {
                throw new ArgumentOutOfRangeException("maximumBytesPerSecond", maximumBytesPerSecond, "The maximum number of bytes per second can't be negative.");
            }

            _baseStream = baseStream;
            _maximumBytesPerSecond = maximumBytesPerSecond;
            _start = CurrentMilliseconds;
            _byteCount = 0;
        }

        public override void Flush() {
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            Throttle(count);
            return _baseStream.Read(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Throttle(count);
            return _baseStream.BeginRead(buffer, offset, count, callback, state);
        }
        
        public override int EndRead(IAsyncResult asyncResult)
        {
            return _baseStream.EndRead(asyncResult);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            _baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            Throttle(count);
            _baseStream.Write(buffer, offset, count);
        }

        public override string ToString() {
            return _baseStream.ToString();
        }

        protected void Throttle(int bufferSizeInBytes) {
            if (_maximumBytesPerSecond <= 0 || bufferSizeInBytes <= 0) {
                return;
            }
            MaximumBytesPerSecond = Settings.MaximumBytesPerSecond ?? Infinite;

            _byteCount += bufferSizeInBytes;
            long weightedMaximumBytesPerSecond = _maximumBytesPerSecond / frmChanThreadWatch.ConcurrentDownloads;
            long elapsedMilliseconds = CurrentMilliseconds - _start;

            if (elapsedMilliseconds > 0) {
                long bps = _byteCount * 1000L / elapsedMilliseconds;

                if (bps > weightedMaximumBytesPerSecond) {
                    long wakeElapsed = _byteCount * 1000L / weightedMaximumBytesPerSecond;
                    int toSleep = (int)(wakeElapsed - elapsedMilliseconds);

                    if (toSleep > 1) {
                        try { Thread.Sleep(toSleep); }
                        catch (ThreadAbortException) { }
                        Reset();
                    }
                }
            }
        }

        protected void Reset() {
            long difference = CurrentMilliseconds - _start;
            if (difference > 1000) {
                _byteCount = 0;
                _start = CurrentMilliseconds;
            }
        }
    }

    public static class Enumerable {
        public static IEnumerable<TSource> Where<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (TSource item in source) {
                if (predicate(item)) {
                    yield return item;
                }
            }
        }

        public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source) {
            foreach (TSource item in source) {
                return item;
            }
            return default(TSource);
        }

        public static bool Any<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (TSource item in source) {
                if (predicate(item)) {
                    return true;
                }
            }
            return false;
        }

        public static bool All<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (TSource item in source) {
                if (!predicate(item)) {
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            foreach (TSource item in source) {
                yield return selector(item);
            }
        }
    }

    public class DownloadStatusEventArgs : EventArgs {
        public DownloadType DownloadType { get; private set; }
        public int CompleteCount { get; private set; }
        public int TotalCount { get; private set; }

        public DownloadStatusEventArgs(DownloadType downloadType, int completeCount, int totalCount) {
            DownloadType = downloadType;
            CompleteCount = completeCount;
            TotalCount = totalCount;
        }
    }

    public class StopStatusEventArgs : EventArgs {
        public StopReason StopReason { get; private set; }

        public StopStatusEventArgs(StopReason stopReason) {
            StopReason = stopReason;
        }
    }

    public class ReparseStatusEventArgs : EventArgs {
        public ReparseType ReparseType { get; private set; }
        public int CompleteCount { get; private set; }
        public int TotalCount { get; private set; }

        public ReparseStatusEventArgs(ReparseType reparseType, int completeCount, int totalCount) {
            ReparseType = reparseType;
            CompleteCount = completeCount;
            TotalCount = totalCount;
        }
    }

    public class DownloadStartEventArgs : EventArgs {
        public long DownloadID { get; private set; }
        public string URL { get; private set; }
        public int TryNumber { get; private set; }
        public long? TotalSize { get; private set; }

        public DownloadStartEventArgs(long downloadID, string url, int tryNumber, long? totalSize) {
            DownloadID = downloadID;
            URL = url;
            TryNumber = tryNumber;
            TotalSize = totalSize;
        }
    }

    public class DownloadProgressEventArgs : EventArgs {
        public long DownloadID { get; private set; }
        public long DownloadedSize { get; private set; }

        public DownloadProgressEventArgs(long downloadID, long downloadedSize) {
            DownloadID = downloadID;
            DownloadedSize = downloadedSize;
        }
    }

    public class DownloadEndEventArgs : EventArgs {
        public long DownloadID { get; private set; }
        public long DownloadedSize { get; private set; }
        public bool IsSuccessful { get; private set; }

        public DownloadEndEventArgs(long downloadID, long downloadedSize, bool isSuccessful) {
            DownloadID = downloadID;
            DownloadedSize = downloadedSize;
            IsSuccessful = isSuccessful;
        }
    }

    public class AddThreadEventArgs : EventArgs {
        public string PageURL { get; private set; }

        public AddThreadEventArgs(string pageURL) {
            PageURL = pageURL;
        }
    }

    public delegate void EventHandler<TSender, TArgs>(TSender sender, TArgs e) where TArgs : EventArgs;

    public delegate void DownloadFileEndCallback(DownloadResult result);

    public delegate void DownloadPageEndCallback(DownloadResult result, string content, DateTime? lastModifiedTime, Encoding encoding);

    public delegate void Action();

    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);

    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);

    public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public delegate TResult Func<TResult>();

    public delegate TResult Func<T, TResult>(T arg);

    public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);

    public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);

    public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public enum ThreadDoubleClickAction {
        OpenFolder = 1,
        OpenURL = 2,
        Edit = 3
    }

    public enum HashType {
        None = 0,
        MD5 = 1
    }

    public enum ReplaceType {
        Other = 0,
        ImageLinkHref = 1,
        ImageSrc = 2,
        QuoteLinkHref = 3,
        DeadLink = 4,
        DeadPost = 5
    }

    public enum DownloadType {
        Page = 1,
        Image = 2,
        Thumbnail = 3
    }

    public enum DownloadResult {
        Completed = 1,
        Skipped = 2,
        RetryLater = 3
    }

    public enum StopReason {
        Other = 0,
        UserRequest = 1,
        Exiting = 2,
        PageNotFound = 3,
        DownloadComplete = 4,
        IOError = 5
    }

    public enum ReparseType {
        Page = 1,
        Image = 2
    }

    public enum BOMType {
        None = 0,
        UTF8 = 1,
        UTF16LE = 2,
        UTF16BE = 3
    }

    public enum SlugType {
        First = 0,
        Last = 1,
        Only = 2
    }

    public enum WindowTitleMacro {
        ApplicationName = 0,
        TotalThreads = 1,
        RunningThreads = 2,
        DeadThreads = 3,
        StoppedThreads = 4
    }
}
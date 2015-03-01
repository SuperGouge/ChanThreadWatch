using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using JDP.Properties;

namespace JDP {
    public partial class frmChanThreadWatch : Form {
        private Dictionary<long, DownloadProgressInfo> _downloadProgresses = new Dictionary<long, DownloadProgressInfo>();
        private frmDownloads _downloadForm;
        private object _startupPromptSync = new object();
        private bool _isExiting;
        private bool _saveThreadList;
        private int _itemAreaY;
        private int[] _columnWidths;
        private object _cboCheckEveryLastValue;
        private bool _isLoadingThreadsFromFile;
        private static Dictionary<string, int> _categories = new Dictionary<string, int>();
        private static Dictionary<string, ThreadWatcher> _watchers = new Dictionary<string, ThreadWatcher>();
        private static HashSet<string> _blacklist = new HashSet<string>();

        public static int ConcurrentDownloads { get; set; }

        // ReleaseDate property and version in AssemblyInfo.cs should be updated for each release.

        public frmChanThreadWatch() {
            InitializeComponent();
            Icon = Resources.ChanThreadWatchIcon;
            niTrayIcon.Icon = Resources.ChanThreadWatchIcon;
            Settings.Load();
            string logPath = Path.Combine(Settings.GetSettingsDirectory(), Settings.LogFileName);
            if (!File.Exists(logPath)) {
                try { File.Create(logPath); }
                catch { }
            }
            int initialWidth = ClientSize.Width;
            GUI.SetFontAndScaling(this);
            float scaleFactorX = (float)ClientSize.Width / initialWidth;
            if (Settings.ClientSize != null) {
                Size newSize = Settings.ClientSize.Value + Size - ClientSize;
                if (newSize.Width >= MinimumSize.Width && newSize.Height >= MinimumSize.Height) {
                    ClientSize = Settings.ClientSize.Value;
                }
            }
            _columnWidths = new int[lvThreads.Columns.Count];
            for (int iColumn = 0; iColumn < lvThreads.Columns.Count; iColumn++) {
                ColumnHeader column = lvThreads.Columns[iColumn];
                if (iColumn < Settings.ColumnWidths.Length) {
                    column.Width = Settings.ColumnWidths[iColumn] > 0 ? Settings.ColumnWidths[iColumn] : 0;
                }
                else {
                    column.Width = Convert.ToInt32(column.Width * scaleFactorX);
                }
                _columnWidths[iColumn] = column.Width != 0 ? column.Width : Settings.DefaultColumnWidths[iColumn];
                if (iColumn < Settings.ColumnIndices.Length && Settings.ColumnIndices[iColumn] > 0 && Settings.ColumnIndices[iColumn] < lvThreads.Columns.Count) {
                    column.DisplayIndex = Settings.ColumnIndices[iColumn];
                }
            }
            GUI.EnableDoubleBuffering(lvThreads);

            BindCheckEveryList();
            BuildCheckEverySubMenu();
            BuildColumnHeaderMenu();

            if ((Settings.DownloadFolder == null) || !Directory.Exists(Settings.AbsoluteDownloadDirectory)) {
                Settings.DownloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Watched Threads");
                Settings.DownloadFolderIsRelative = false;
            }
            if (Settings.CheckEvery == 1) {
                Settings.CheckEvery = 0;
            }

            chkPageAuth.Checked = Settings.UsePageAuth ?? false;
            txtPageAuth.Text = Settings.PageAuth ?? String.Empty;
            chkImageAuth.Checked = Settings.UseImageAuth ?? false;
            txtImageAuth.Text = Settings.ImageAuth ?? String.Empty;
            chkOneTime.Checked = Settings.OneTimeDownload ?? false;
            chkAutoFollow.Checked = Settings.AutoFollow ?? false;
            if (Settings.CheckEvery != null) {
                foreach (ListItemInt32 item in cboCheckEvery.Items) {
                    if (item.Value != Settings.CheckEvery) continue;
                    cboCheckEvery.SelectedValue = Settings.CheckEvery;
                    break;
                }
                if ((int)cboCheckEvery.SelectedValue != Settings.CheckEvery) txtCheckEvery.Text = Settings.CheckEvery.ToString();
            }
            else {
                cboCheckEvery.SelectedValue = 3;
            }
            OnThreadDoubleClick = Settings.OnThreadDoubleClick ?? ThreadDoubleClickAction.OpenFolder;

            if ((Settings.CheckForUpdates == true) && (Settings.LastUpdateCheck ?? DateTime.MinValue) < DateTime.Now.Date) {
                CheckForUpdates();
            }
            niTrayIcon.Visible = Settings.MinimizeToTray ?? false;
        }

        public Dictionary<long, DownloadProgressInfo> DownloadProgresses {
            get { return _downloadProgresses; }
        }

        private ThreadDoubleClickAction OnThreadDoubleClick {
            get {
                if (rbEdit.Checked)
                    return ThreadDoubleClickAction.Edit;
                else if (rbOpenURL.Checked)
                    return ThreadDoubleClickAction.OpenURL;
                else
                    return ThreadDoubleClickAction.OpenFolder;
            }
            set {
                if (value == ThreadDoubleClickAction.Edit)
                    rbEdit.Checked = true;
                else if (value == ThreadDoubleClickAction.OpenURL)
                    rbOpenURL.Checked = true;
                else
                    rbOpenFolder.Checked = true;
            }
        }

        private void frmChanThreadWatch_Shown(object sender, EventArgs e) {
            UseWaitCursor = true;
            btnAdd.Enabled = false;
            btnAddFromClipboard.Enabled = false;
            btnRemoveCompleted.Enabled = false;
            btnDownloads.Enabled = false;
            btnSettings.Enabled = false;
            btnAbout.Enabled = false;
            btnHelp.Enabled = false;
            lvThreads.Enabled = false;
            Application.DoEvents();

            lvThreads.Items.Add(new ListViewItem());
            _itemAreaY = lvThreads.GetItemRect(0).Y;
            lvThreads.Items.RemoveAt(0);

            LoadThreadList();
            LoadBlacklist();

            UseWaitCursor = false;
            btnAdd.Enabled = true;
            btnAddFromClipboard.Enabled = true;
            btnRemoveCompleted.Enabled = true;
            btnDownloads.Enabled = true;
            btnSettings.Enabled = true;
            btnAbout.Enabled = true;
            btnHelp.Enabled = true;
            lvThreads.Enabled = true;

            lvThreads.ListViewItemSorter = new ListViewItemSorter(Settings.SortColumn ?? 3) { Ascending = Settings.SortAscending ?? true };
            lvThreads.Sort();
            FocusLastThread();
        }

        private void frmChanThreadWatch_FormClosed(object sender, FormClosedEventArgs e) {
            if (IsDisposed) return;
            Settings.UsePageAuth = chkPageAuth.Checked;
            Settings.PageAuth = txtPageAuth.Text;
            Settings.UseImageAuth = chkImageAuth.Checked;
            Settings.ImageAuth = txtImageAuth.Text;
            Settings.OneTimeDownload = chkOneTime.Checked;
            Settings.AutoFollow = chkAutoFollow.Checked;
            Settings.CheckEvery = pnlCheckEvery.Enabled ? (cboCheckEvery.Enabled ? (int)cboCheckEvery.SelectedValue : Int32.Parse(txtCheckEvery.Text)) : 0;
            Settings.OnThreadDoubleClick = OnThreadDoubleClick;
            Settings.ClientSize = ClientSize;

            int[] columnWidths = new int[lvThreads.Columns.Count];
            int[] columnIndices = new int[lvThreads.Columns.Count];
            for (int i = 0; i < lvThreads.Columns.Count; i++) {
                columnWidths[i] = lvThreads.Columns[i].Width;
                columnIndices[i] = lvThreads.Columns[i].DisplayIndex;
            }
            Settings.ColumnWidths = columnWidths;
            Settings.ColumnIndices = columnIndices;

            ListViewItemSorter sorter = (ListViewItemSorter)lvThreads.ListViewItemSorter;
            if (sorter != null) {
                Settings.SortColumn = sorter.Column;
                Settings.SortAscending = sorter.Ascending;
            }

            Settings.Save();

            foreach (ThreadWatcher watcher in ThreadWatchers) {
                watcher.Stop(StopReason.Exiting);
            }

            // Save before waiting in addition to after in case the wait hangs or is interrupted
            SaveThreadList();

            _isExiting = true;
            foreach (ThreadWatcher watcher in ThreadWatchers) {
                while (!watcher.WaitUntilStopped(10)) {
                    Application.DoEvents();
                }
            }

            SaveThreadList();

            Program.ReleaseMutex();
        }

        private void frmChanThreadWatch_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("UniformResourceLocatorW") ||
                e.Data.GetDataPresent("UniformResourceLocator"))
            {
                if ((e.AllowedEffect & DragDropEffects.Copy) != 0) {
                    e.Effect = DragDropEffects.Copy;
                }
                else if ((e.AllowedEffect & DragDropEffects.Link) != 0) {
                    e.Effect = DragDropEffects.Link;
                }
            }
        }

        private void frmChanThreadWatch_DragDrop(object sender, DragEventArgs e) {
            if (_isExiting) return;
            string url = null;
            if (e.Data.GetDataPresent("UniformResourceLocatorW")) {
                byte[] data = ((MemoryStream)e.Data.GetData("UniformResourceLocatorW")).ToArray();
                url = Encoding.Unicode.GetString(data, 0, General.StrLenW(data) * 2);
            }
            else if (e.Data.GetDataPresent("UniformResourceLocator")) {
                byte[] data = ((MemoryStream)e.Data.GetData("UniformResourceLocator")).ToArray();
                url = Encoding.Default.GetString(data, 0, General.StrLen(data));
            }
            url = General.CleanPageURL(url);
            if (url != null) {
                AddThread(url);
                FocusThread(url);
                _saveThreadList = true;
            }
        }

        private void frmChanThreadWatch_Resize(object sender, EventArgs e) {
            if (Settings.MinimizeToTray != true) return;
            if (WindowState == FormWindowState.Minimized) {
                Hide();
            }
        }

        private void txtPageURL_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                btnAdd_Click(txtPageURL, null);
                e.SuppressKeyPress = true;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            if (txtPageURL.Text.Trim().Length == 0) return;
            string pageURL = General.CleanPageURL(txtPageURL.Text);
            if (pageURL == null) {
                MessageBox.Show(this, "The specified URL is invalid.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!AddThread(pageURL)) {
                MessageBox.Show(this, "The same thread is already being watched, downloaded or has been blacklisted.", "Cannot Add Thread", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPageURL.Clear();
                FocusThread(pageURL);
                return;
            }
            FocusThread(pageURL);
            txtPageURL.Clear();
            txtPageURL.Focus();
            _saveThreadList = true;
        }

        private void btnAddFromClipboard_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            string text;
            try {
                text = Clipboard.GetText();
            }
            catch {
                return;
            }
            string[] urls = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (urls.Length > 0) {
                lvThreads.SelectedItems.Clear();
                lvThreads.Select();
            }
            for (int iURL = 0; iURL < urls.Length; iURL++) {
                string url = General.CleanPageURL(urls[iURL]);
                if (url == null) continue;
                AddThread(url);
                if (urls.Length == 1) {
                    FocusThread(url);
                }
                else {
                    SiteHelper siteHelper = SiteHelper.GetInstance((new Uri(url)).Host);
                    siteHelper.SetURL(url);
                    ThreadWatcher watcher;
                    if (_watchers.TryGetValue(siteHelper.GetPageID(), out watcher)) {
                        (((WatcherExtraData)watcher.Tag).ListViewItem).Selected = true;
                    }
                }
            }
            _saveThreadList = true;
        }

        private void btnRemoveCompleted_Click(object sender, EventArgs e) {
            RemoveThreads(true, false);
        }

        private void miStop_Click(object sender, EventArgs e) {
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                watcher.Stop(StopReason.UserRequest);
            }
            _saveThreadList = true;
        }

        private void miStart_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                if (!watcher.IsRunning) {
                    watcher.Start();
                }
            }
            _saveThreadList = true;
        }

        private void miEdit_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                using (frmThreadEdit editForm = new frmThreadEdit(watcher, _categories)) {
                    if (editForm.ShowDialog(this) == DialogResult.OK) {
                        watcher.Description = editForm.Description;
                        if (watcher.Category != editForm.Category) {
                            UpdateCategories(watcher.Category, true);
                            UpdateCategories(editForm.Category);
                        }
                        watcher.Category = editForm.Category;
                        if (!watcher.IsRunning) {
                            watcher.PageAuth = editForm.PageAuth;
                            watcher.ImageAuth = editForm.ImageAuth;
                            watcher.CheckIntervalSeconds = editForm.CheckIntervalSeconds;
                            watcher.OneTimeDownload = editForm.OneTimeDownload;
                            watcher.AutoFollow = editForm.AutoFollow;
                        }
                        DisplayData(watcher);
                        _saveThreadList = true;
                    }
                }
                break;
            }
        }

        private void miOpenFolder_Click(object sender, EventArgs e) {
            int selectedCount = lvThreads.SelectedItems.Count;
            if (selectedCount > 5 && MessageBox.Show(this, "Do you want to open the folders of all " + selectedCount + " selected items?",
                "Open Folders", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                string dir = watcher.ThreadDownloadDirectory;
                ThreadWatcher tmpWatcher = watcher;
                ThreadPool.QueueUserWorkItem((s) => {
                    try {
                        if (!Directory.Exists(dir)) {
                            tmpWatcher.Stop(StopReason.Other);
                            BeginInvoke(() => {
                                MessageBox.Show(this, "The folder " + dir + " does not exists. The watcher has been stopped to let you fix this, in case of an unwanted deletion or rename. If the thread file cannot be found for the next check, it won't include possible deleted posts.",
                                    "Folder Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            });
                        }
                        else {
                            Process.Start(dir);
                        }
                    }
                    catch (Exception ex) {
                        Logger.Log(ex.ToString());
                    }
                });
            }
        }

        private void miOpenURL_Click(object sender, EventArgs e) {
            int selectedCount = lvThreads.SelectedItems.Count;
            if (selectedCount > 5 && MessageBox.Show(this, "Do you want to open the URLs of all " + selectedCount + " selected items?",
                "Open URLs", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                string url = watcher.PageURL;
                ThreadPool.QueueUserWorkItem((s) => {
                    try {
                        Process.Start(url);
                    }
                    catch (Exception ex) {
                        Logger.Log(ex.ToString());
                    }
                });
            }
        }

        private void miCopyURL_Click(object sender, EventArgs e) {
            StringBuilder sb = new StringBuilder();
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                if (sb.Length != 0) sb.Append(Environment.NewLine);
                sb.Append(watcher.PageURL);
            }
            try {
                Clipboard.Clear();
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex) {
                MessageBox.Show(this, "Unable to copy to clipboard: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void miRemove_Click(object sender, EventArgs e) {
            RemoveThreads(false, true);
        }

        private void miRemoveAndDeleteFolder_Click(object sender, EventArgs e) {
            if (MessageBox.Show(this, "Are you sure you want to delete the selected threads and all associated files from disk?",
                "Delete From Disk", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            RemoveThreads(false, true,
                (watcher) => {
                    if (Directory.Exists(watcher.ThreadDownloadDirectory)) Directory.Delete(watcher.ThreadDownloadDirectory, true);
                    string categoryPath = General.RemoveLastDirectory(watcher.ThreadDownloadDirectory);
                    if (categoryPath != watcher.MainDownloadDirectory && Directory.GetFiles(categoryPath).Length == 0 && Directory.GetDirectories(categoryPath).Length == 0) {
                        Directory.Delete(categoryPath);
                    }
                });
        }

        private void miBlacklist_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            List<string> lines = new List<string>();
            foreach (string rule in _blacklist) {
                lines.Add(rule);
            }
            HashSet<string> blacklist = new HashSet<string>();
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                if (!_blacklist.Contains(watcher.PageID) && blacklist.Add(watcher.PageID)) {
                    lines.Add(watcher.PageID);
                }
            }
            try {
                string path = Path.Combine(Settings.GetSettingsDirectory(), Settings.BlacklistFileName);
                File.WriteAllLines(path, lines.ToArray());
                foreach (string pageID in blacklist) {
                    _blacklist.Add(pageID);
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        private void miCheckNow_Click(object sender, EventArgs e) {
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                watcher.MillisecondsUntilNextCheck = 0;
            }
        }

        private void miCheckEvery_Click(object sender, EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                int checkIntervalSeconds = Convert.ToInt32(menuItem.Tag) * 60;
                foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                    watcher.CheckIntervalSeconds = checkIntervalSeconds;
                }
                UpdateWaitingWatcherStatuses();
            }
            _saveThreadList = true;
        }

        private void miReparse_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                if (!watcher.IsRunning && Settings.SaveThumbnails != false) {
                    //TODO: Prevent program closing when reparsing
                    watcher.BeginReparse();
                }
            }
            _saveThreadList = true;
        }

        private void btnDownloads_Click(object sender, EventArgs e) {
            if (_downloadForm != null && !_downloadForm.IsDisposed) {
                _downloadForm.Activate();
            }
            else {
                _downloadForm = new frmDownloads(this);
                GUI.CenterChildForm(this, _downloadForm);
                _downloadForm.Show(this);
            }
        }

        private void btnSettings_Click(object sender, EventArgs e) {
            if (_isExiting) return;
            using (frmSettings settingsForm = new frmSettings()) {
                GUI.CenterChildForm(this, settingsForm);
                settingsForm.ShowDialog(this);
            }
            niTrayIcon.Visible = Settings.MinimizeToTray ?? false;
            tmrBackupThreadList.Interval = (Settings.BackupEvery ?? 1) * 60 * 1000;
        }

        private void btnAbout_Click(object sender, EventArgs e) {
            MessageBox.Show(this, String.Format("Chan Thread Watch{0}Version {1} ({2}){0}{0}Original Author: JDP (jart1126@yahoo.com){0}http://sites.google.com/site/chanthreadwatch/" +
                                                "{0}{0}Maintained by: SuperGouge (https://github.com/SuperGouge){0}{3}",
                Environment.NewLine, General.Version, General.ReleaseDate, General.ProgramURL), "About",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void btnHelp_Click(object sender, EventArgs e) {
            Process.Start(General.WikiURL);
        }

        private void lvThreads_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Delete) {
                RemoveThreads(false, true);
            }
            else if (e.Control && e.KeyCode == Keys.A) {
                foreach (ListViewItem item in lvThreads.Items) {
                    item.Selected = true;
                }
            }
            else if (e.Control && e.KeyCode == Keys.I) {
                foreach (ListViewItem item in lvThreads.Items) {
                    item.Selected = !item.Selected;
                }
            }
        }

        private void lvThreads_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                int selectedCount = lvThreads.SelectedItems.Count;
                if (selectedCount != 0) {
                    bool anyRunning = false;
                    bool anyStopped = false;
                    foreach (ThreadWatcher watcher in SelectedThreadWatchers) {
                        bool isRunning = watcher.IsRunning;
                        anyRunning |= isRunning;
                        anyStopped |= !isRunning;
                    }
                    miEdit.Visible = selectedCount == 1;
                    miStop.Visible = anyRunning;
                    miStart.Visible = anyStopped;
                    miCheckNow.Visible = anyRunning;
                    miCheckEvery.Visible = anyRunning;
                    miRemove.Visible = anyStopped;
                    miRemoveAndDeleteFolder.Visible = anyStopped;
                    miReparse.Visible = anyStopped;
                    cmThreads.Show(lvThreads, e.Location);
                }
            }
        }

        private void lvThreads_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (OnThreadDoubleClick == ThreadDoubleClickAction.Edit) {
                miEdit_Click(null, null);
            }
            else if (OnThreadDoubleClick == ThreadDoubleClickAction.OpenFolder) {
                miOpenFolder_Click(null, null);
            }
            else {
                miOpenURL_Click(null, null);
            }
        }

        private void lvThreads_ColumnClick(object sender, ColumnClickEventArgs e) {
            ListViewItemSorter sorter = (ListViewItemSorter)lvThreads.ListViewItemSorter;
            if (sorter == null) {
                sorter = new ListViewItemSorter(e.Column);
                lvThreads.ListViewItemSorter = sorter;
            }
            else if (e.Column != sorter.Column) {
                sorter.Column = e.Column;
                sorter.Ascending = true;
            }
            else {
                sorter.Ascending = !sorter.Ascending;
            }
            lvThreads.Sort();
        }

        private void chkOneTime_CheckedChanged(object sender, EventArgs e) {
            pnlCheckEvery.Enabled = !chkOneTime.Checked;
        }

        private void chkPageAuth_CheckedChanged(object sender, EventArgs e) {
            txtPageAuth.Enabled = chkPageAuth.Checked;
        }

        private void chkImageAuth_CheckedChanged(object sender, EventArgs e) {
            txtImageAuth.Enabled = chkImageAuth.Checked;
        }

        private void txtCheckEvery_TextChanged(object sender, EventArgs e) {
            int checkEvery;
            if (Int32.TryParse(txtCheckEvery.Text, out checkEvery)) {
                cboCheckEvery.SelectedIndex = -1;
                cboCheckEvery.Enabled = false;
            }
            else {
                if (cboCheckEvery.SelectedIndex == -1) cboCheckEvery.SelectedValue = _cboCheckEveryLastValue;
                cboCheckEvery.Enabled = true;
            }
        }

        private void cboCheckEvery_SelectedIndexChanged(object sender, EventArgs e) {
            if (cboCheckEvery.SelectedIndex == -1) return;
            if (cboCheckEvery.Focused) txtCheckEvery.Clear();
            if (_cboCheckEveryLastValue == null && (int)cboCheckEvery.SelectedValue == 0 && (int)cboCheckEvery.SelectedValue != Settings.CheckEvery) {
                _cboCheckEveryLastValue = 3;
            }
            else {
                _cboCheckEveryLastValue = cboCheckEvery.SelectedValue;
            }
        }

        private void tmrSaveThreadList_Tick(object sender, EventArgs e) {
            if (_saveThreadList && !_isExiting) {
                SaveThreadList();
                _saveThreadList = false;
            }
        }

        private void tmrUpdateWaitStatus_Tick(object sender, EventArgs e) {
            UpdateWaitingWatcherStatuses();
        }

        private void tmrMaintenance_Tick(object sender, EventArgs e) {
            lock (_downloadProgresses) {
                if (_downloadProgresses.Count == 0) return;
                List<long> oldDownloadIDs = new List<long>();
                long ticksNow = TickCount.Now;
                foreach (DownloadProgressInfo info in _downloadProgresses.Values) {
                    if (info.EndTicks != null && ticksNow - info.EndTicks.Value > 5000) {
                        oldDownloadIDs.Add(info.DownloadID);
                    }
                }
                foreach (long downloadID in oldDownloadIDs) {
                    _downloadProgresses.Remove(downloadID);
                }
            }
        }
        
        private void tmrMonitor_Tick(object sender, EventArgs e) {
            int running = 0;
            int dead = 0;
            int stopped = 0;
            foreach (ThreadWatcher watcher in ThreadWatchers) {
                if (watcher.IsRunning || watcher.IsWaiting) {
                    running++;
                }
                else if (watcher.StopReason == StopReason.PageNotFound) {
                    dead++;
                }
                else {
                    stopped++;
                }
            }
            miMonitorTotal.Text = String.Format("Watching {0} thread{1}", _watchers.Count, _watchers.Count != 1 ? "s" : String.Empty);
            miMonitorRunning.Text = String.Format("    {0} running", running);
            miMonitorDead.Text = String.Format("    {0} dead", dead);
            miMonitorStopped.Text = String.Format("    {0} stopped", stopped);
        }

        private void tmrBackupThreadList_Tick(object sender, EventArgs e) {
            if (Settings.BackupThreadList == true) {
                General.BackupThreadList(Settings.BackupCheckSize ?? false);
            }
        }

        private void niTrayIcon_Click(object sender, EventArgs e) {
            // Nothing for now
        }

        private void niTrayIcon_DoubleClick(object sender, EventArgs e) {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void miExit_Click(object sender, EventArgs e) {
            Close();
        }

        private void ThreadWatcher_DownloadStatus(ThreadWatcher watcher, DownloadStatusEventArgs args) {
            WatcherExtraData extraData = (WatcherExtraData)watcher.Tag;
            bool isInitialPageDownload = false;
            bool isFirstImageUpdate = false;
            if (args.DownloadType == DownloadType.Page) {
                if (!extraData.HasDownloadedPage) {
                    extraData.HasDownloadedPage = true;
                    isInitialPageDownload = true;
                }
                extraData.PreviousDownloadWasPage = true;
            }
            if (args.DownloadType == DownloadType.Image && extraData.PreviousDownloadWasPage) {
                extraData.LastImageOn = DateTime.Now;
                extraData.PreviousDownloadWasPage = false;
                isFirstImageUpdate = true;
            }
            BeginInvoke(() => {
                SetDownloadStatus(watcher, args.DownloadType, args.CompleteCount, args.TotalCount);
                if (isInitialPageDownload) {
                    DisplayDescription(watcher);
                    _saveThreadList = true;
                }
                if (isFirstImageUpdate) {
                    DisplayLastImageOn(watcher);
                    _saveThreadList = true;
                }
                SetupWaitTimer();
            });
        }

        private void ThreadWatcher_WaitStatus(ThreadWatcher watcher, EventArgs args) {
            BeginInvoke(() => {
                SetWaitStatus(watcher);
                SetupWaitTimer();
            });
        }

        private void ThreadWatcher_StopStatus(ThreadWatcher watcher, StopStatusEventArgs args) {
            BeginInvoke(() => {
                SetStopStatus(watcher, args.StopReason);
                SetupWaitTimer();
                if (args.StopReason != StopReason.UserRequest && args.StopReason != StopReason.Exiting) {
                    _saveThreadList = true;
                }
            });
        }

        private void ThreadWatcher_ReparseStatus(ThreadWatcher watcher, ReparseStatusEventArgs args) {
            BeginInvoke(() => {
                SetReparseStatus(watcher, args.ReparseType, args.CompleteCount, args.TotalCount);
                SetupWaitTimer();
            });
        }

        private void ThreadWatcher_ThreadDownloadDirectoryRename(ThreadWatcher watcher, EventArgs args) {
            BeginInvoke(() => {
                _saveThreadList = true;
            });
        }

        private void ThreadWatcher_DownloadStart(ThreadWatcher watcher, DownloadStartEventArgs args) {
            DownloadProgressInfo info = new DownloadProgressInfo();
            info.DownloadID = args.DownloadID;
            info.URL = args.URL;
            info.TryNumber = args.TryNumber;
            info.StartTicks = TickCount.Now;
            info.TotalSize = args.TotalSize;
            lock (_downloadProgresses) {
                _downloadProgresses[args.DownloadID] = info;
                ConcurrentDownloads += 1;
            }
        }

        private void ThreadWatcher_DownloadProgress(ThreadWatcher watcher, DownloadProgressEventArgs args) {
            lock (_downloadProgresses) {
                DownloadProgressInfo info;
                if (!_downloadProgresses.TryGetValue(args.DownloadID, out info)) return;
                info.DownloadedSize = args.DownloadedSize;
                _downloadProgresses[args.DownloadID] = info;
            }
        }

        private void ThreadWatcher_DownloadEnd(ThreadWatcher watcher, DownloadEndEventArgs args) {
            lock (_downloadProgresses) {
                DownloadProgressInfo info;
                if (!_downloadProgresses.TryGetValue(args.DownloadID, out info)) return;
                info.EndTicks = TickCount.Now;
                info.DownloadedSize = args.DownloadedSize;
                info.TotalSize = args.DownloadedSize;
                _downloadProgresses[args.DownloadID] = info;
                ConcurrentDownloads -= 1;
            }
        }

        private void ThreadWatcher_AddThread(ThreadWatcher watcher, AddThreadEventArgs args) {
            BeginInvoke(() => {
                ThreadInfo thread = new ThreadInfo {
                    URL = args.PageURL,
                    PageAuth = watcher.PageAuth,
                    ImageAuth = watcher.ImageAuth,
                    CheckIntervalSeconds = watcher.CheckIntervalSeconds,
                    OneTimeDownload = watcher.OneTimeDownload,
                    SaveDir = null,
                    Description = String.Empty,
                    StopReason = null,
                    ExtraData = new WatcherExtraData {
                        AddedOn = DateTime.Now,
                        AddedFrom = watcher.PageID
                    },
                    Category = watcher.Category,
                    AutoFollow = Settings.RecursiveAutoFollow != false
                };
                SiteHelper siteHelper = SiteHelper.GetInstance((new Uri(thread.URL)).Host);
                siteHelper.SetURL(thread.URL);
                if (_watchers.ContainsKey(siteHelper.GetPageID())) return;
                if (AddThread(thread)) {
                    _saveThreadList = true;
                }
            });
        }

        private bool AddThread(string pageURL) {
            ThreadInfo thread = new ThreadInfo {
                URL = pageURL,
                PageAuth = (chkPageAuth.Checked && (txtPageAuth.Text.IndexOf(':') != -1)) ? txtPageAuth.Text : String.Empty,
                ImageAuth = (chkImageAuth.Checked && (txtImageAuth.Text.IndexOf(':') != -1)) ? txtImageAuth.Text : String.Empty,
                CheckIntervalSeconds = pnlCheckEvery.Enabled ? (cboCheckEvery.Enabled ? (int)cboCheckEvery.SelectedValue * 60 : Int32.Parse(txtCheckEvery.Text) * 60) : 0,
                OneTimeDownload = chkOneTime.Checked,
                SaveDir = null,
                Description = String.Empty,
                StopReason = null,
                ExtraData = null,
                Category = cboCategory.Text,
                AutoFollow = chkAutoFollow.Checked
            };
            return AddThread(thread);
        }

        private bool AddThread(ThreadInfo thread) {
            ThreadWatcher watcher = null;
            ThreadWatcher parentThread = null;
            ListViewItem newListViewItem = null;
            SiteHelper siteHelper = SiteHelper.GetInstance((new Uri(thread.URL)).Host);
            siteHelper.SetURL(thread.URL);
            string pageID = siteHelper.GetPageID();
            if (IsBlacklisted(pageID)) return false;

            if (_watchers.ContainsKey(pageID)) {
                watcher = _watchers[pageID];
                if (watcher.IsRunning) return false;
            }

            if (watcher == null) {
                watcher = new ThreadWatcher(thread.URL);
                watcher.ThreadDownloadDirectory = thread.SaveDir;
                watcher.Description = thread.Description;
                if (_isLoadingThreadsFromFile) watcher.DoNotRename = true;
                watcher.Category = thread.Category;
                watcher.DoNotRename = false;
                if (thread.ExtraData != null && !String.IsNullOrEmpty(thread.ExtraData.AddedFrom)) {
                    _watchers.TryGetValue(thread.ExtraData.AddedFrom, out parentThread);
                    watcher.ParentThread = parentThread;
                }
                watcher.DownloadStatus += ThreadWatcher_DownloadStatus;
                watcher.WaitStatus += ThreadWatcher_WaitStatus;
                watcher.StopStatus += ThreadWatcher_StopStatus;
                watcher.ReparseStatus += ThreadWatcher_ReparseStatus;
                watcher.ThreadDownloadDirectoryRename += ThreadWatcher_ThreadDownloadDirectoryRename;
                watcher.DownloadStart += ThreadWatcher_DownloadStart;
                watcher.DownloadProgress += ThreadWatcher_DownloadProgress;
                watcher.DownloadEnd += ThreadWatcher_DownloadEnd;
                watcher.AddThread += ThreadWatcher_AddThread;

                newListViewItem = new ListViewItem(String.Empty);
                for (int i = 1; i < lvThreads.Columns.Count; i++) {
                    newListViewItem.SubItems.Add(String.Empty);
                }
                newListViewItem.Tag = watcher;
                lvThreads.Items.Add(newListViewItem);
                lvThreads.Sort();
                UpdateCategories(watcher.Category);
            }

            watcher.PageAuth = thread.PageAuth;
            watcher.ImageAuth = thread.ImageAuth;
            watcher.CheckIntervalSeconds = thread.CheckIntervalSeconds;
            watcher.OneTimeDownload = thread.OneTimeDownload;
            watcher.AutoFollow = thread.AutoFollow;

            if (thread.ExtraData == null) {
                thread.ExtraData = watcher.Tag as WatcherExtraData ?? new WatcherExtraData { AddedOn = DateTime.Now };
            }
            if (newListViewItem != null) {
                thread.ExtraData.ListViewItem = newListViewItem;
            }
            watcher.Tag = thread.ExtraData;

            if (parentThread != null) parentThread.ChildThreads.Add(watcher.PageID, watcher);
            if (!_watchers.ContainsKey(watcher.PageID)) {
                _watchers.Add(watcher.PageID, watcher);
            }
            else {
                _watchers[watcher.PageID] = watcher;
            }
            DisplayData(watcher);

            if (thread.StopReason == null && !_isLoadingThreadsFromFile) {
                watcher.Start();
            }
            else if (thread.StopReason != null) {
                watcher.Stop(thread.StopReason.Value);
            }
            return true;
        }

        private void RemoveThreads(bool removeCompleted, bool removeSelected) {
            RemoveThreads(removeCompleted, removeSelected, null);
        }

        private void RemoveThreads(bool removeCompleted, bool removeSelected, Action<ThreadWatcher> preRemoveAction) {
            int i = 0;
            while (i < lvThreads.Items.Count) {
                ThreadWatcher watcher = (ThreadWatcher)lvThreads.Items[i].Tag;
                if ((removeCompleted || (removeSelected && lvThreads.Items[i].Selected)) && !watcher.IsRunning) {
                    if (preRemoveAction != null) {
                        try { preRemoveAction(watcher); }
                        catch (Exception ex) {
                            Logger.Log(ex.ToString());
                        }
                    }
                    UpdateCategories(watcher.Category, true);
                    lvThreads.Items.RemoveAt(i);
                    _watchers.Remove(watcher.PageID);
                }
                else {
                    i++;
                }
            }
            _saveThreadList = true;
        }

        private void BindCheckEveryList() {
            cboCheckEvery.ValueMember = "Value";
            cboCheckEvery.DisplayMember = "Text";
            cboCheckEvery.DataSource = new[] {
                new ListItemInt32(0, "1 or <"),
                new ListItemInt32(2, "2"),
                new ListItemInt32(3, "3"),
                new ListItemInt32(5, "5"),
                new ListItemInt32(10, "10"),
                new ListItemInt32(60, "60")
            };
        }

        private void BuildCheckEverySubMenu() {
            for (int i = 0; i < cboCheckEvery.Items.Count; i++) {
                int minutes = ((ListItemInt32)cboCheckEvery.Items[i]).Value;
                MenuItem menuItem = new MenuItem {
                    Index = i,
                    Tag = minutes,
                    Text = minutes > 0 ? minutes + " Minutes" : "1 Minute or <"
                };
                menuItem.Click += miCheckEvery_Click;
                miCheckEvery.MenuItems.Add(menuItem);
            }
        }

        private void BuildColumnHeaderMenu() {
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Popup += (s, e) => {
                for (int i = 0; i < lvThreads.Columns.Count; i++) {
                    contextMenu.MenuItems[i].Checked = lvThreads.Columns[i].Width != 0;
                }
            };
            for (int i = 0; i < lvThreads.Columns.Count; i++) {
                MenuItem menuItem = new MenuItem {
                    Index = i,
                    Tag = i,
                    Text = lvThreads.Columns[i].Text
                };
                menuItem.Click += (s, e) => {
                    int iColumn = (int)((MenuItem)s).Tag;
                    ColumnHeader column = lvThreads.Columns[iColumn];
                    if (column.Width != 0) {
                        _columnWidths[iColumn] = column.Width;
                        column.Width = 0;
                    }
                    else {
                        column.Width = _columnWidths[iColumn];
                    }
                };
                contextMenu.MenuItems.Add(menuItem);
            }
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Opening += (s, e) => {
                e.Cancel = true;
                Point pos = lvThreads.PointToClient(Control.MousePosition);
                if (pos.Y >= _itemAreaY) return;
                contextMenu.Show(lvThreads, pos);
            };
            lvThreads.ContextMenuStrip = contextMenuStrip;
        }

        private void SetupWaitTimer() {
            bool anyWaiting = false;
            foreach (ThreadWatcher watcher in ThreadWatchers) {
                if (watcher.IsWaiting) {
                    anyWaiting = true;
                    break;
                }
            }
            if (!tmrUpdateWaitStatus.Enabled && anyWaiting) {
                tmrUpdateWaitStatus.Start();
            }
            else if (tmrUpdateWaitStatus.Enabled && !anyWaiting) {
                tmrUpdateWaitStatus.Stop();
            }
        }

        private void UpdateWaitingWatcherStatuses() {
            foreach (ThreadWatcher watcher in ThreadWatchers) {
                if (watcher.IsWaiting) {
                    SetWaitStatus(watcher);
                }
            }
        }

        private void SetSubItemText(ThreadWatcher watcher, ColumnIndex columnIndex, string text) {
            ListViewItem item = ((WatcherExtraData)watcher.Tag).ListViewItem;
            var subItem = item.SubItems[(int)columnIndex];
            if (subItem.Text != text) {
                subItem.Text = text;
                lvThreads.Sort();
            }
        }

        private void DisplayDescription(ThreadWatcher watcher) {
            SetSubItemText(watcher, ColumnIndex.Description, watcher.Description);
        }

        private void DisplayStatus(ThreadWatcher watcher, string status) {
            SetSubItemText(watcher, ColumnIndex.Status, status);
        }

        private void DisplayAddedOn(ThreadWatcher watcher) {
            DateTime time = ((WatcherExtraData)watcher.Tag).AddedOn;
            SetSubItemText(watcher, ColumnIndex.AddedOn, time.ToString("yyyy/MM/dd HH:mm:ss"));
        }

        private void DisplayLastImageOn(ThreadWatcher watcher) {
            DateTime? time = ((WatcherExtraData)watcher.Tag).LastImageOn;
            SetSubItemText(watcher, ColumnIndex.LastImageOn, time != null ? time.Value.ToString("yyyy/MM/dd HH:mm:ss") : String.Empty);
        }

        private void DisplayAddedFrom(ThreadWatcher watcher) {
            ThreadWatcher fromWatcher;
            _watchers.TryGetValue(((WatcherExtraData)watcher.Tag).AddedFrom ?? String.Empty, out fromWatcher);
            SetSubItemText(watcher, ColumnIndex.AddedFrom, fromWatcher != null ? fromWatcher.Description : String.Empty);
        }

        private void DisplayCategory(ThreadWatcher watcher) {
            SetSubItemText(watcher, ColumnIndex.Category, watcher.Category);
        }

        private void DisplayData(ThreadWatcher watcher) {
            DisplayDescription(watcher);
            DisplayAddedOn(watcher);
            DisplayLastImageOn(watcher);
            if (!_isLoadingThreadsFromFile) DisplayAddedFrom(watcher);
            DisplayCategory(watcher);
        }

        private void SetDownloadStatus(ThreadWatcher watcher, DownloadType downloadType, int completeCount, int totalCount) {
            string type;
            bool hideDetail = false;
            switch (downloadType) {
                case DownloadType.Page:
                    type = totalCount == 1 ? "page" : "pages";
                    hideDetail = totalCount == 1;
                    break;
                case DownloadType.Image:
                    type = "images";
                    break;
                case DownloadType.Thumbnail:
                    type = "thumbnails";
                    break;
                default:
                    return;
            }
            string status = hideDetail ? "Downloading " + type :
                String.Format("Downloading {0}: {1} of {2} completed", type, completeCount, totalCount);
            DisplayStatus(watcher, status);
        }

        private void SetWaitStatus(ThreadWatcher watcher) {
            int remainingSeconds = (watcher.MillisecondsUntilNextCheck + 999) / 1000;
            DisplayStatus(watcher, String.Format("Waiting {0} seconds", remainingSeconds));
        }

        private void SetStopStatus(ThreadWatcher watcher, StopReason stopReason) {
            string status = "Stopped: ";
            switch (stopReason) {
                case StopReason.UserRequest:
                    status += "User requested";
                    break;
                case StopReason.Exiting:
                    status += "Exiting";
                    break;
                case StopReason.PageNotFound:
                    status += "Page not found";
                    break;
                case StopReason.DownloadComplete:
                    status += "Download complete";
                    break;
                case StopReason.IOError:
                    status += "Error writing to disk";
                    break;
                default:
                    status += "Unknown error";
                    break;
            }
            DisplayStatus(watcher, status);
        }

        private void SetReparseStatus(ThreadWatcher watcher, ReparseType reparseType, int completeCount, int totalCount) {
            string type;
            bool hideDetail = false;
            switch (reparseType) {
                case ReparseType.Page:
                    type = totalCount == 1 ? "page" : "pages";
                    hideDetail = totalCount == 1;
                    break;
                case ReparseType.Image:
                    type = "images";
                    break;
                default:
                    return;
            }
            string status = hideDetail ? "Reparsing " + type :
                String.Format("Reparsing {0}: {1} of {2} completed", type, completeCount, totalCount);
            DisplayStatus(watcher, status);
            //TODO: Find alternative to Application.DoEvents() (throws StackOverflowException when reparsing a large number of images)
            //Application.DoEvents();
        }

        private void SaveThreadList() {
            try {
                // Prepare lines before writing file so that an exception can't result
                // in a partially written file.
                List<string> lines = new List<string>();
                lines.Add("4"); // File version
                foreach (ThreadWatcher watcher in ThreadWatchers) {
                    WatcherExtraData extraData = (WatcherExtraData)watcher.Tag;
                    lines.Add(watcher.PageURL);
                    lines.Add(watcher.PageAuth);
                    lines.Add(watcher.ImageAuth);
                    lines.Add(watcher.CheckIntervalSeconds.ToString());
                    lines.Add(watcher.OneTimeDownload ? "1" : "0");
                    lines.Add(watcher.ThreadDownloadDirectory != null ? General.GetRelativeDirectoryPath(watcher.ThreadDownloadDirectory, watcher.MainDownloadDirectory) : String.Empty);
                    lines.Add((watcher.IsStopping && watcher.StopReason != StopReason.Exiting) ? ((int)watcher.StopReason).ToString() : String.Empty);
                    lines.Add(watcher.Description);
                    lines.Add(extraData.AddedOn.ToUniversalTime().Ticks.ToString());
                    lines.Add(extraData.LastImageOn != null ? extraData.LastImageOn.Value.ToUniversalTime().Ticks.ToString() : String.Empty);
                    lines.Add(extraData.AddedFrom);
                    lines.Add(watcher.Category);
                    lines.Add(watcher.AutoFollow ? "1" : "0");
                }
                string path = Path.Combine(Settings.GetSettingsDirectory(), Settings.ThreadsFileName);
                File.WriteAllLines(path, lines.ToArray());
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        private void LoadThreadList() {
            try {
                string path = Path.Combine(Settings.GetSettingsDirectory(), Settings.ThreadsFileName);
                if (!File.Exists(path)) return;
                string[] lines = File.ReadAllLines(path);
                if (lines.Length < 1) return;
                int fileVersion = Int32.Parse(lines[0]);
                int linesPerThread;
                switch (fileVersion) {
                    case 1: linesPerThread = 6; break;
                    case 2: linesPerThread = 7; break;
                    case 3: linesPerThread = 10; break;
                    case 4: linesPerThread = 13; break;
                    default: return;
                }
                if (lines.Length < (1 + linesPerThread)) return;
                _isLoadingThreadsFromFile = true;
                UpdateCategories(String.Empty);
                int i = 1;
                while (i <= lines.Length - linesPerThread) {
                    ThreadInfo thread = new ThreadInfo { ExtraData = new WatcherExtraData() };
                    thread.URL = lines[i++];
                    thread.PageAuth = lines[i++];
                    thread.ImageAuth = lines[i++];
                    thread.CheckIntervalSeconds = Int32.Parse(lines[i++]);
                    thread.OneTimeDownload = lines[i++] == "1";
                    thread.SaveDir = lines[i++];
                    thread.SaveDir = thread.SaveDir.Length != 0 ? General.GetAbsoluteDirectoryPath(thread.SaveDir, Settings.AbsoluteDownloadDirectory) : null;
                    if (fileVersion >= 2) {
                        string stopReasonLine = lines[i++];
                        if (stopReasonLine.Length != 0) {
                            thread.StopReason = (StopReason)Int32.Parse(stopReasonLine);
                        }
                    }
                    if (fileVersion >= 3) {
                        thread.Description = lines[i++];
                        thread.ExtraData.AddedOn = new DateTime(Int64.Parse(lines[i++]), DateTimeKind.Utc).ToLocalTime();
                        string lastImageOn = lines[i++];
                        if (lastImageOn.Length != 0) {
                            thread.ExtraData.LastImageOn = new DateTime(Int64.Parse(lastImageOn), DateTimeKind.Utc).ToLocalTime();
                        }
                    }
                    else {
                        thread.Description = String.Empty;
                        thread.ExtraData.AddedOn = DateTime.Now;
                    }
                    if (fileVersion >= 4) {
                        thread.ExtraData.AddedFrom = lines[i++];
                        thread.Category = lines[i++];
                        thread.AutoFollow = lines[i++] == "1";
                    }
                    else {
                        thread.ExtraData.AddedFrom = String.Empty;
                        thread.Category = String.Empty;
                    }
                    AddThread(thread);
                }
                _isLoadingThreadsFromFile = false;
                foreach (ThreadWatcher threadWatcher in ThreadWatchers) {
                    ThreadWatcher parentThread;
                    _watchers.TryGetValue(((WatcherExtraData)threadWatcher.Tag).AddedFrom, out parentThread);
                    threadWatcher.ParentThread = parentThread;
                    if (parentThread != null && !parentThread.ChildThreads.ContainsKey(threadWatcher.PageID) && !parentThread.ChildThreads.ContainsKey(parentThread.PageID)) {
                        parentThread.ChildThreads.Add(threadWatcher.PageID, threadWatcher);
                    }
                    DisplayAddedFrom(threadWatcher);
                    if (Settings.ChildThreadsAreNewFormat == true && threadWatcher.StopReason != StopReason.PageNotFound && threadWatcher.StopReason != StopReason.UserRequest) {
                        threadWatcher.Start();
                    }
                }
                if (Settings.ChildThreadsAreNewFormat != true) {
                    foreach (ThreadWatcher threadWatcher in ThreadWatchers) {
                        if (threadWatcher.ChildThreads.Count == 0 || threadWatcher.ParentThread != null) continue;
                        foreach (ThreadWatcher descendantThread in threadWatcher.DescendantThreads.Values) {
                            descendantThread.DoNotRename = true;
                            string sourceDir = descendantThread.ThreadDownloadDirectory;
                            string destDir;
                            if (General.RemoveLastDirectory(sourceDir) == descendantThread.MainDownloadDirectory) {
                                destDir = Path.Combine(descendantThread.MainDownloadDirectory, General.RemoveLastDirectory(sourceDir));
                            }
                            else {
                                destDir = Path.Combine(General.RemoveLastDirectory(threadWatcher.ThreadDownloadDirectory),
                                    General.GetRelativeDirectoryPath(descendantThread.ThreadDownloadDirectory, threadWatcher.ThreadDownloadDirectory));
                            }
                            if (String.Equals(destDir, sourceDir, StringComparison.Ordinal) || !Directory.Exists(sourceDir)) continue;
                            try {
                                if (String.Equals(destDir, sourceDir, StringComparison.OrdinalIgnoreCase)) {
                                    Directory.Move(sourceDir, destDir + " Temp");
                                    sourceDir = destDir + " Temp";
                                }
                                if (!Directory.Exists(General.RemoveLastDirectory(destDir))) Directory.CreateDirectory(General.RemoveLastDirectory(destDir));
                                Directory.Move(sourceDir, destDir);
                                descendantThread.ThreadDownloadDirectory = destDir;
                            }
                            catch (Exception ex) {
                                Logger.Log(ex.ToString());
                            }
                            descendantThread.DoNotRename = false;
                        }
                    }
                    Settings.ChildThreadsAreNewFormat = true;
                    Settings.Save();

                    foreach (ThreadWatcher threadWatcher in ThreadWatchers) {
                        if (threadWatcher.StopReason != StopReason.PageNotFound && threadWatcher.StopReason != StopReason.UserRequest) threadWatcher.Start();
                    }
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        private void LoadBlacklist() {
            try {
                string path = Path.Combine(Settings.GetSettingsDirectory(), Settings.BlacklistFileName);
                if (!File.Exists(path)) return;
                string[] lines = File.ReadAllLines(path);
                if (lines.Length < 1) return;
                for (int i = 0; i < lines.Length; i++) {
                    string rule = lines[i];
                    if (rule.Split('/').Length == 3) {
                        _blacklist.Add(rule);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        private void CheckForUpdates() {
            Thread thread = new Thread(CheckForUpdateThread);
            thread.IsBackground = true;
            thread.Start();
        }

        private void CheckForUpdateThread() {
            string html;
            try {
                html = General.DownloadPageToString(General.ProgramURL);
            }
            catch {
                return;
            }
            Settings.LastUpdateCheck = DateTime.Now.Date;
            var htmlParser = new HTMLParser(html);
            HTMLTagRange labelLatestDivTagRange = htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                htmlParser.FindStartTags("div"), t => HTMLParser.ClassAttributeValueHas(t, "label-latest"))));
            if (labelLatestDivTagRange == null) return;
            HTMLTagRange versionSpanTagRange = htmlParser.CreateTagRange(Enumerable.FirstOrDefault(Enumerable.Where(
                htmlParser.FindStartTags(labelLatestDivTagRange, "span"), t => HTMLParser.ClassAttributeValueHas(t, "css-truncate-target"))));
            if (versionSpanTagRange == null) return;
            string latestStr = htmlParser.GetInnerHTML(versionSpanTagRange).Replace("v", "");
            int latest = ParseVersionNumber(latestStr);
            if (latest == -1) return;
            int current = ParseVersionNumber(General.Version);
            if (!String.IsNullOrEmpty(Settings.LatestUpdateVersion)) {
                current = Math.Max(current, ParseVersionNumber(Settings.LatestUpdateVersion));
            }
            if (latest > current) {
                lock (_startupPromptSync) {
                    if (IsDisposed) return;
                    Settings.LatestUpdateVersion = latestStr;
                    Invoke(() => {
                        if (MessageBox.Show(this, "A newer version of Chan Thread Watch is available.  Would you like to open the Chan Thread Watch website?",
                            "Newer Version Found", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            Process.Start(General.ProgramURL);
                        }
                    });
                }
            }
        }

        private int ParseVersionNumber(string str) {
            string[] split = str.Split('.');
            int num = 0;
            try {
                if (split.Length >= 1) num |= (Int32.Parse(split[0]) & 0x7F) << 24;
                if (split.Length >= 2) num |= (Int32.Parse(split[1]) & 0xFF) << 16;
                if (split.Length >= 3) num |= (Int32.Parse(split[2]) & 0xFF) << 8;
                if (split.Length >= 4) num |= (Int32.Parse(split[3]) & 0xFF);
                return num;
            }
            catch {
                return -1;
            }
        }

        private IAsyncResult BeginInvoke(MethodInvoker method) {
            return BeginInvoke((Delegate)method);
        }

        private object Invoke(MethodInvoker method) {
            return Invoke((Delegate)method);
        }

        private IEnumerable<ThreadWatcher> ThreadWatchers {
            get {
                foreach (ListViewItem item in lvThreads.Items) {
                    yield return (ThreadWatcher)item.Tag;
                }
            }
        }

        private IEnumerable<ThreadWatcher> SelectedThreadWatchers {
            get {
                foreach (ListViewItem item in lvThreads.SelectedItems) {
                    yield return (ThreadWatcher)item.Tag;
                }
            }
        }

        private enum ColumnIndex {
            Description = 0,
            Status = 1,
            LastImageOn = 2,
            AddedOn = 3,
            AddedFrom = 4,
            Category = 5
        }

        private void UpdateCategories(string key, bool remove = false) {
            key = key ?? String.Empty;
            if (remove && (_categories.ContainsKey(key) && (--_categories[key] < 1 && !String.IsNullOrEmpty(key)))) {
                _categories.Remove(key);
                cboCategory.Items.Remove(key);
            }
            else {
                if (_categories.ContainsKey(key)) {
                    ++_categories[key];
                }
                else {
                    _categories.Add(key, 1);
                    cboCategory.Items.Add(key);
                }
            }
        }
        
        private void FocusThread(string pageURL) {
            SiteHelper siteHelper = SiteHelper.GetInstance((new Uri(pageURL)).Host);
            siteHelper.SetURL(pageURL);
            ThreadWatcher watcher;
            if (_watchers.TryGetValue(siteHelper.GetPageID(), out watcher)) {
                FocusThread(watcher);
            }
        }

        private void FocusThread(ThreadWatcher watcher) {
            ListViewItem item = ((WatcherExtraData)watcher.Tag).ListViewItem;
            lvThreads.SelectedItems.Clear();
            lvThreads.Select();
            item.Selected = true;
            item.EnsureVisible();
        }

        private void FocusLastThread() {
            if (lvThreads.Items.Count > 0) {
                FocusThread((ThreadWatcher)lvThreads.Items[lvThreads.Items.Count - 1].Tag);
            }
        }

        private bool IsBlacklisted(string pageID) {
            if (_blacklist.Contains(pageID)) return true;
            if (Settings.BlacklistWildcards != true) return false;
            string[] pageIDSplit = pageID.Split('/');
            if (pageIDSplit.Length != 3) return false;
            foreach (string rule in _blacklist) {
                string[] ruleSplit = rule.Split('/');
                if (ruleSplit.Length != 3) continue;
                if (ruleSplit[0] != "*" && ruleSplit[0] != pageIDSplit[0]) continue;
                if (ruleSplit[1] != "*" && ruleSplit[1] != pageIDSplit[1]) continue;
                if (ruleSplit[2] != "*" && ruleSplit[2] != pageIDSplit[2]) continue;
                return true;
            }
            return false;
        }
    }
}
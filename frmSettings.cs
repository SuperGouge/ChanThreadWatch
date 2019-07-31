using System;
using System.IO;
using System.Windows.Forms;

namespace JDP {
    public partial class frmSettings : Form {
        public frmSettings() {
            InitializeComponent();
            GUI.SetFontAndScaling(this);
        }

        private void frmSettings_Load(object sender, EventArgs e) {
            txtDownloadFolder.Text = Settings.DownloadFolder;
            chkDownloadFolderRelative.Checked = Settings.DownloadFolderIsRelative ?? false;
            chkCompletedFolder.Checked = Settings.MoveToCompletedFolder ?? false;
            txtCompletedFolder.Enabled = btnCompletedFolder.Enabled = chkCompletedFolderRelative.Enabled = chkCompletedFolder.Checked;
            txtCompletedFolder.Text = Settings.CompletedFolder;
            chkCompletedFolderRelative.Checked = Settings.CompletedFolderIsRelative ?? false;
            chkCustomUserAgent.Checked = Settings.UseCustomUserAgent ?? false;
            txtCustomUserAgent.Text = Settings.CustomUserAgent ?? String.Empty;
            chkSaveThumbnails.Checked = Settings.SaveThumbnails ?? true;
            chkRenameDownloadFolderWithDescription.Checked = Settings.RenameDownloadFolderWithDescription ?? false;
            chkRenameDownloadFolderWithCategory.Checked = Settings.RenameDownloadFolderWithCategory ?? false;
            chkRenameDownloadFolderWithParentThreadDescription.Checked = Settings.RenameDownloadFolderWithParentThreadDescription ?? false;
            pnlParentThreadDescriptionFormat.Enabled = chkRenameDownloadFolderWithParentThreadDescription.Checked;
            txtParentThreadDescriptionFormat.Text = Settings.ParentThreadDescriptionFormat ?? " ({Parent})";
            chkSortImagesByPoster.Checked = Settings.SortImagesByPoster ?? false;
            chkRecursiveAutoFollow.Checked = Settings.RecursiveAutoFollow ?? true;
            chkInterBoardAutoFollow.Checked = Settings.InterBoardAutoFollow ?? true;
            chkUseOriginalFileNames.Checked = Settings.UseOriginalFileNames ?? false;
            chkVerifyImageHashes.Checked = Settings.VerifyImageHashes ?? true;
            chkUseSlug.Checked = Settings.UseSlug ?? false;
            pnlSlug.Enabled = chkUseSlug.Checked;
            rbSlugFirst.Checked = Settings.SlugType == SlugType.First;
            rbSlugLast.Checked = Settings.SlugType == SlugType.Last;
            rbSlugOnly.Checked = Settings.SlugType == SlugType.Only;
            chkCheckForUpdates.Checked = Settings.CheckForUpdates ?? false;
            chkBlacklistWildcards.Checked = Settings.BlacklistWildcards ?? false;
            chkMinimizeToTray.Checked = Settings.MinimizeToTray ?? false;
            chkBackupThreadList.Checked = Settings.BackupThreadList ?? false;
            pnlBackupEvery.Enabled = chkBackupThreadList.Checked;
            txtBackupEvery.Text = (Settings.BackupEvery ?? 1).ToString();
            chkThreadStatus.CheckState = Settings.ThreadStatusSimple == true ? CheckState.Checked : CheckState.Unchecked;
            txtThreadStatusBoxThreshold.Enabled = Settings.ThreadStatusSimple == true;
            txtThreadStatusBoxThreshold.Text = (Settings.ThreadStatusThreshold ?? 10).ToString();
            chkBackupCheckSize.Enabled = chkBackupThreadList.Checked;
            chkBackupCheckSize.Checked = Settings.BackupCheckSize ?? false;
            txtMaximumKilobytesPerSecond.Text = ((Settings.MaximumBytesPerSecond ?? 0) / 1024).ToString();
            txtWindowTitle.Text = Settings.WindowTitle ?? String.Format("{{{0}}}", WindowTitleMacro.ApplicationName);
            txtWindowTitle.SelectionStart = txtWindowTitle.Text.Length;
            cboWindowTitle.DataSource = Enum.GetValues(typeof(WindowTitleMacro));
            if (Settings.UseExeDirectoryForSettings == true) {
                rbSettingsInExeFolder.Checked = true;
            }
            else {
                rbSettingsInAppDataFolder.Checked = true;
            }
        }

        private void btnOK_Click(object sender, EventArgs e) {
            try {
                string downloadFolder = txtDownloadFolder.Text.Trim();

                if (downloadFolder.Length == 0) {
                    throw new Exception("You must enter a download folder.");
                }
                if (!Directory.Exists(downloadFolder)) {
                    try {
                        Directory.CreateDirectory(downloadFolder);
                    }
                    catch {
                        throw new Exception("Unable to create the download folder.");
                    }
                }

                string completedFolder = txtCompletedFolder.Text.Trim();

                if (chkCompletedFolder.Checked) {
                    if (completedFolder.Length == 0) {
                        throw new Exception("You must enter a completed folder.");
                    }
                    if (!Directory.Exists(completedFolder)) {
                        try {
                            Directory.CreateDirectory(completedFolder);
                        }
                        catch {
                            throw new Exception("Unable to create the completed folder.");
                        }
                    }
                }

                string oldSettingsFolder = Settings.GetSettingsDirectory();
                string newSettingsFolder = Settings.GetSettingsDirectory(rbSettingsInExeFolder.Checked);
                if (!String.Equals(newSettingsFolder, oldSettingsFolder, StringComparison.OrdinalIgnoreCase)) {
                    if (!Program.ObtainMutex(newSettingsFolder)) {
                        MessageBox.Show(this, "Another instance of this program is using the same settings folder.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    try {
                        foreach (string fileName in new[] { Settings.SettingsFileName, Settings.ThreadsFileName }) {
                            string oldPath = Path.Combine(oldSettingsFolder, fileName);
                            string newPath = Path.Combine(newSettingsFolder, fileName);
                            if (!File.Exists(oldPath)) continue;
                            byte[] contents = File.ReadAllBytes(oldPath);
                            File.WriteAllBytes(newPath, contents);
                            try { File.Delete(oldPath); }
                            catch { }
                        }
                    }
                    catch {
                        MessageBox.Show(this, "Unable to move the settings files.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                string oldAbsoluteDownloadFolder = Settings.AbsoluteDownloadDirectory;

                Settings.DownloadFolder = downloadFolder;
                Settings.DownloadFolderIsRelative = chkDownloadFolderRelative.Checked;
                Settings.MoveToCompletedFolder = chkCompletedFolder.Checked;
                Settings.CompletedFolder = completedFolder;
                Settings.CompletedFolderIsRelative = chkCompletedFolderRelative.Checked;
                Settings.UseCustomUserAgent = chkCustomUserAgent.Checked;
                Settings.CustomUserAgent = txtCustomUserAgent.Text;
                Settings.SaveThumbnails = chkSaveThumbnails.Checked;
                Settings.RenameDownloadFolderWithDescription = chkRenameDownloadFolderWithDescription.Checked;
                Settings.RenameDownloadFolderWithCategory = chkRenameDownloadFolderWithCategory.Checked;
                Settings.RenameDownloadFolderWithParentThreadDescription = chkRenameDownloadFolderWithParentThreadDescription.Checked;
                Settings.ParentThreadDescriptionFormat = txtParentThreadDescriptionFormat.Text;
                Settings.SortImagesByPoster = chkSortImagesByPoster.Checked;
                Settings.RecursiveAutoFollow = chkRecursiveAutoFollow.Checked;
                Settings.InterBoardAutoFollow = chkInterBoardAutoFollow.Checked;
                Settings.UseOriginalFileNames = chkUseOriginalFileNames.Checked;
                Settings.VerifyImageHashes = chkVerifyImageHashes.Checked;
                Settings.UseSlug = chkUseSlug.Checked;
                if (rbSlugFirst.Checked) {
                    Settings.SlugType = SlugType.First;
                }
                else if (rbSlugOnly.Checked) {
                    Settings.SlugType = SlugType.Only;
                }
                else {
                    Settings.SlugType = SlugType.Last;
                }
                Settings.CheckForUpdates = chkCheckForUpdates.Checked;
                Settings.BlacklistWildcards = chkBlacklistWildcards.Checked;
                Settings.MinimizeToTray = chkMinimizeToTray.Checked;
                Settings.BackupThreadList = chkBackupThreadList.Checked;
                Settings.BackupEvery = Int32.Parse(txtBackupEvery.Text);
                Settings.ThreadStatusSimple = txtThreadStatusBoxThreshold.Enabled;
                Int32.TryParse(txtThreadStatusBoxThreshold.Text, out int tmpThreadStatusThreshold);
                Settings.ThreadStatusThreshold = tmpThreadStatusThreshold;
                Settings.BackupCheckSize = chkBackupCheckSize.Checked;
                Settings.MaximumBytesPerSecond = Int64.Parse(txtMaximumKilobytesPerSecond.Text) * 1024;
                Settings.WindowTitle = txtWindowTitle.Text;
                Settings.UseExeDirectoryForSettings = rbSettingsInExeFolder.Checked;

                Settings.Save();

                if (!String.Equals(Settings.AbsoluteDownloadDirectory, oldAbsoluteDownloadFolder, StringComparison.OrdinalIgnoreCase)) {
                    MessageBox.Show(this, "The new download folder will not affect threads currently being watched until the program is restarted.  " +
                                          "If you are still watching the threads at next run, make sure you have moved their download folders into the new download folder.",
                        "Download Folder Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDownloadFolder_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.Description = "Select the download location.";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    SetDownloadFolderTextBox(dialog.SelectedPath);
                }
            }
        }

        private void btnCompletedFolder_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.Description = "Select the download location.";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    SetCompletedFolderTextBox(dialog.SelectedPath);
                }
            }
        }

        private void btnBackupThreadList_Click(object sender, EventArgs e) {
            General.BackupThreadList();
        }

        private void chkDownloadFolderRelative_CheckedChanged(object sender, EventArgs e) {
            SetDownloadFolderTextBox(txtDownloadFolder.Text.Trim());
        }

        private void chkCompletedFolderRelative_CheckedChanged(object sender, EventArgs e) {
            SetCompletedFolderTextBox(txtCompletedFolder.Text.Trim());
        }

        private void chkCompletedFolder_CheckedChanged(object sender, EventArgs e) {
            txtCompletedFolder.Enabled = btnCompletedFolder.Enabled = chkCompletedFolderRelative.Enabled = chkCompletedFolder.Checked;
        }

        private void chkCustomUserAgent_CheckedChanged(object sender, EventArgs e) {
            txtCustomUserAgent.Enabled = chkCustomUserAgent.Checked;
        }

        private void chkUseSlug_CheckedChanged(object sender, EventArgs e) {
            pnlSlug.Enabled = chkUseSlug.Checked;
        }

        private void chkRenameDownloadFolderWithParentThreadDescription_CheckedChanged(object sender, EventArgs e) {
            pnlParentThreadDescriptionFormat.Enabled = chkRenameDownloadFolderWithParentThreadDescription.Checked;
        }

        private void chkBackupThreadList_CheckedChanged(object sender, EventArgs e) {
            pnlBackupEvery.Enabled = chkBackupThreadList.Checked;
            chkBackupCheckSize.Enabled = chkBackupThreadList.Checked;
        }

        private void txtBackupEvery_Leave(object sender, EventArgs e) {
            int minutes;
            if (!Int32.TryParse(txtBackupEvery.Text, out minutes) || minutes < 1) {
                txtBackupEvery.Text = "1";
            }
        }

        private void txtMaximumKilobytesPerSecond_Leave(object sender, EventArgs e) {
            long kbps;
            if (!Int64.TryParse(txtMaximumKilobytesPerSecond.Text, out kbps) || kbps < 0 || kbps > Int64.MaxValue / 1024) {
                txtMaximumKilobytesPerSecond.Text = "0";
            }
        }

        private void btnWindowTitle_Click(object sender, EventArgs e) {
            int selectionStart = txtWindowTitle.SelectionStart;
            string macro = String.Format("{{{0}}}", cboWindowTitle.SelectedValue);
            txtWindowTitle.Text = txtWindowTitle.Text.Insert(selectionStart, macro);
            txtWindowTitle.SelectionStart = selectionStart + macro.Length;
        }

        private void SetDownloadFolderTextBox(string path) {
            txtDownloadFolder.Text = chkDownloadFolderRelative.Checked ?
                General.GetRelativeDirectoryPath(path, Settings.ExeDirectory) :
                General.GetAbsoluteDirectoryPath(path, Settings.ExeDirectory);
        }

        private void SetCompletedFolderTextBox(string path) {
            txtCompletedFolder.Text = chkCompletedFolderRelative.Checked ?
                General.GetRelativeDirectoryPath(path, Settings.ExeDirectory) :
                General.GetAbsoluteDirectoryPath(path, Settings.ExeDirectory);
        }

        private void txtThreadStatusBoxThreshold_Leave(object sender, EventArgs e) {
            if (!Int32.TryParse(txtThreadStatusBoxThreshold.Text, out int tmpThreadStatusThreshold) || tmpThreadStatusThreshold < 0) {
                txtThreadStatusBoxThreshold.Text = Settings.ThreadStatusThreshold.ToString();
            }
        }

        private void chkThreadStatus_CheckedChanged(object sender, EventArgs e) {
            if (chkThreadStatus.Checked) {
                txtThreadStatusBoxThreshold.Enabled = true;
            }
            else {
                txtThreadStatusBoxThreshold.Enabled = false;
            }
        }
    }
}

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
            chkRelativePath.Checked = Settings.DownloadFolderIsRelative ?? false;
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
                Settings.DownloadFolderIsRelative = chkRelativePath.Checked;
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
                Settings.UseExeDirectoryForSettings = rbSettingsInExeFolder.Checked;

                try {
                    Settings.Save();
                }
                catch (Exception ex) {
                    Logger.Log(ex.ToString());
                }

                if (!String.Equals(Settings.AbsoluteDownloadDirectory, oldAbsoluteDownloadFolder, StringComparison.OrdinalIgnoreCase)) {
                    MessageBox.Show(this, "The new download folder will not affect threads currently being watched until the program is restared.  " +
                                          "If you are still watching the threads at next run, make sure you have moved their download folders into the new download folder.",
                        "Download Folder Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.Description = "Select the download location.";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    SetDownloadFolderTextBox(dialog.SelectedPath);
                }
            }
        }

        private void chkRelativePath_CheckedChanged(object sender, EventArgs e) {
            SetDownloadFolderTextBox(txtDownloadFolder.Text.Trim());
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

        private void SetDownloadFolderTextBox(string path) {
            txtDownloadFolder.Text = chkRelativePath.Checked ?
                General.GetRelativeDirectoryPath(path, Settings.ExeDirectory) :
                General.GetAbsoluteDirectoryPath(path, Settings.ExeDirectory);
        }
    }
}
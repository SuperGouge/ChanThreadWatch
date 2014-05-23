namespace JDP {
    partial class frmSettings {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.txtDownloadFolder = new System.Windows.Forms.TextBox();
            this.lblDownloadFolder = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkCustomUserAgent = new System.Windows.Forms.CheckBox();
            this.txtCustomUserAgent = new System.Windows.Forms.TextBox();
            this.chkRelativePath = new System.Windows.Forms.CheckBox();
            this.lblSettingsLocation = new System.Windows.Forms.Label();
            this.rbSettingsInAppDataFolder = new System.Windows.Forms.RadioButton();
            this.rbSettingsInExeFolder = new System.Windows.Forms.RadioButton();
            this.chkUseOriginalFileNames = new System.Windows.Forms.CheckBox();
            this.chkVerifyImageHashes = new System.Windows.Forms.CheckBox();
            this.chkCheckForUpdates = new System.Windows.Forms.CheckBox();
            this.chkSaveThumbnails = new System.Windows.Forms.CheckBox();
            this.chkRenameDownloadFolderWithDescription = new System.Windows.Forms.CheckBox();
            this.chkUseSlug = new System.Windows.Forms.CheckBox();
            this.pnlSlug = new System.Windows.Forms.Panel();
            this.rbSlugOnly = new System.Windows.Forms.RadioButton();
            this.rbSlugLast = new System.Windows.Forms.RadioButton();
            this.rbSlugFirst = new System.Windows.Forms.RadioButton();
            this.chkRenameDownloadFolderWithCategory = new System.Windows.Forms.CheckBox();
            this.chkRenameDownloadFolderWithParentThreadDescription = new System.Windows.Forms.CheckBox();
            this.lblParentThreadDescriptionFormat = new System.Windows.Forms.Label();
            this.txtParentThreadDescriptionFormat = new System.Windows.Forms.TextBox();
            this.chkSortImagesByPoster = new System.Windows.Forms.CheckBox();
            this.grpThreadFolderNaming = new System.Windows.Forms.GroupBox();
            this.grpNamingStructure = new System.Windows.Forms.GroupBox();
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.chkRecursiveAutoFollow = new System.Windows.Forms.CheckBox();
            this.chkInterBoardAutoFollow = new System.Windows.Forms.CheckBox();
            this.lblSlugType = new System.Windows.Forms.Label();
            this.pnlParentThreadDescriptionFormat = new System.Windows.Forms.Panel();
            this.pnlSlug.SuspendLayout();
            this.grpThreadFolderNaming.SuspendLayout();
            this.grpNamingStructure.SuspendLayout();
            this.grpGeneral.SuspendLayout();
            this.pnlParentThreadDescriptionFormat.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDownloadFolder
            // 
            this.txtDownloadFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDownloadFolder.Location = new System.Drawing.Point(108, 8);
            this.txtDownloadFolder.Name = "txtDownloadFolder";
            this.txtDownloadFolder.Size = new System.Drawing.Size(506, 20);
            this.txtDownloadFolder.TabIndex = 1;
            // 
            // lblDownloadFolder
            // 
            this.lblDownloadFolder.AutoSize = true;
            this.lblDownloadFolder.Location = new System.Drawing.Point(8, 12);
            this.lblDownloadFolder.Name = "lblDownloadFolder";
            this.lblDownloadFolder.Size = new System.Drawing.Size(87, 13);
            this.lblDownloadFolder.TabIndex = 0;
            this.lblDownloadFolder.Text = "Download folder:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(622, 8);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(80, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(662, 271);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(60, 23);
            this.btnOK.TabIndex = 14;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(730, 271);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(68, 23);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // chkCustomUserAgent
            // 
            this.chkCustomUserAgent.AutoSize = true;
            this.chkCustomUserAgent.Location = new System.Drawing.Point(10, 42);
            this.chkCustomUserAgent.Name = "chkCustomUserAgent";
            this.chkCustomUserAgent.Size = new System.Drawing.Size(117, 17);
            this.chkCustomUserAgent.TabIndex = 4;
            this.chkCustomUserAgent.Text = "Custom user agent:";
            this.chkCustomUserAgent.UseVisualStyleBackColor = true;
            this.chkCustomUserAgent.CheckedChanged += new System.EventHandler(this.chkCustomUserAgent_CheckedChanged);
            // 
            // txtCustomUserAgent
            // 
            this.txtCustomUserAgent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCustomUserAgent.Enabled = false;
            this.txtCustomUserAgent.Location = new System.Drawing.Point(140, 40);
            this.txtCustomUserAgent.Name = "txtCustomUserAgent";
            this.txtCustomUserAgent.Size = new System.Drawing.Size(656, 20);
            this.txtCustomUserAgent.TabIndex = 5;
            // 
            // chkRelativePath
            // 
            this.chkRelativePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkRelativePath.AutoSize = true;
            this.chkRelativePath.Location = new System.Drawing.Point(710, 12);
            this.chkRelativePath.Name = "chkRelativePath";
            this.chkRelativePath.Size = new System.Drawing.Size(89, 17);
            this.chkRelativePath.TabIndex = 3;
            this.chkRelativePath.Text = "Relative path";
            this.chkRelativePath.UseVisualStyleBackColor = true;
            this.chkRelativePath.CheckedChanged += new System.EventHandler(this.chkRelativePath_CheckedChanged);
            // 
            // lblSettingsLocation
            // 
            this.lblSettingsLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSettingsLocation.AutoSize = true;
            this.lblSettingsLocation.Location = new System.Drawing.Point(8, 279);
            this.lblSettingsLocation.Name = "lblSettingsLocation";
            this.lblSettingsLocation.Size = new System.Drawing.Size(85, 13);
            this.lblSettingsLocation.TabIndex = 11;
            this.lblSettingsLocation.Text = "Save settings in:";
            // 
            // rbSettingsInAppDataFolder
            // 
            this.rbSettingsInAppDataFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbSettingsInAppDataFolder.AutoSize = true;
            this.rbSettingsInAppDataFolder.Location = new System.Drawing.Point(108, 277);
            this.rbSettingsInAppDataFolder.Name = "rbSettingsInAppDataFolder";
            this.rbSettingsInAppDataFolder.Size = new System.Drawing.Size(130, 17);
            this.rbSettingsInAppDataFolder.TabIndex = 12;
            this.rbSettingsInAppDataFolder.TabStop = true;
            this.rbSettingsInAppDataFolder.Text = "Application data folder";
            this.rbSettingsInAppDataFolder.UseVisualStyleBackColor = true;
            // 
            // rbSettingsInExeFolder
            // 
            this.rbSettingsInExeFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbSettingsInExeFolder.AutoSize = true;
            this.rbSettingsInExeFolder.Location = new System.Drawing.Point(252, 277);
            this.rbSettingsInExeFolder.Name = "rbSettingsInExeFolder";
            this.rbSettingsInExeFolder.Size = new System.Drawing.Size(107, 17);
            this.rbSettingsInExeFolder.TabIndex = 13;
            this.rbSettingsInExeFolder.TabStop = true;
            this.rbSettingsInExeFolder.Text = "Executable folder";
            this.rbSettingsInExeFolder.UseVisualStyleBackColor = true;
            // 
            // chkUseOriginalFileNames
            // 
            this.chkUseOriginalFileNames.AutoSize = true;
            this.chkUseOriginalFileNames.Location = new System.Drawing.Point(6, 115);
            this.chkUseOriginalFileNames.Name = "chkUseOriginalFileNames";
            this.chkUseOriginalFileNames.Size = new System.Drawing.Size(273, 17);
            this.chkUseOriginalFileNames.TabIndex = 8;
            this.chkUseOriginalFileNames.Text = "Use original filenames (only supported for some sites)";
            this.chkUseOriginalFileNames.UseVisualStyleBackColor = true;
            // 
            // chkVerifyImageHashes
            // 
            this.chkVerifyImageHashes.AutoSize = true;
            this.chkVerifyImageHashes.Location = new System.Drawing.Point(6, 42);
            this.chkVerifyImageHashes.Name = "chkVerifyImageHashes";
            this.chkVerifyImageHashes.Size = new System.Drawing.Size(265, 17);
            this.chkVerifyImageHashes.TabIndex = 9;
            this.chkVerifyImageHashes.Text = "Verify image hashes (only supported for some sites)";
            this.chkVerifyImageHashes.UseVisualStyleBackColor = true;
            // 
            // chkCheckForUpdates
            // 
            this.chkCheckForUpdates.AutoSize = true;
            this.chkCheckForUpdates.Location = new System.Drawing.Point(6, 109);
            this.chkCheckForUpdates.Name = "chkCheckForUpdates";
            this.chkCheckForUpdates.Size = new System.Drawing.Size(353, 17);
            this.chkCheckForUpdates.TabIndex = 10;
            this.chkCheckForUpdates.Text = "Automatically check for and notify of updated versions of this program";
            this.chkCheckForUpdates.UseVisualStyleBackColor = true;
            // 
            // chkSaveThumbnails
            // 
            this.chkSaveThumbnails.AutoSize = true;
            this.chkSaveThumbnails.Location = new System.Drawing.Point(6, 19);
            this.chkSaveThumbnails.Name = "chkSaveThumbnails";
            this.chkSaveThumbnails.Size = new System.Drawing.Size(221, 17);
            this.chkSaveThumbnails.TabIndex = 6;
            this.chkSaveThumbnails.Text = "Save thumbnails and post-process HTML";
            this.chkSaveThumbnails.UseVisualStyleBackColor = true;
            // 
            // chkRenameDownloadFolderWithDescription
            // 
            this.chkRenameDownloadFolderWithDescription.AutoSize = true;
            this.chkRenameDownloadFolderWithDescription.Location = new System.Drawing.Point(6, 19);
            this.chkRenameDownloadFolderWithDescription.Name = "chkRenameDownloadFolderWithDescription";
            this.chkRenameDownloadFolderWithDescription.Size = new System.Drawing.Size(79, 17);
            this.chkRenameDownloadFolderWithDescription.TabIndex = 7;
            this.chkRenameDownloadFolderWithDescription.Text = "Description";
            this.chkRenameDownloadFolderWithDescription.UseVisualStyleBackColor = true;
            // 
            // chkUseSlug
            // 
            this.chkUseSlug.AutoSize = true;
            this.chkUseSlug.Location = new System.Drawing.Point(6, 138);
            this.chkUseSlug.Name = "chkUseSlug";
            this.chkUseSlug.Size = new System.Drawing.Size(285, 17);
            this.chkUseSlug.TabIndex = 16;
            this.chkUseSlug.Text = "Use slug in thread name (only supported for some sites)";
            this.chkUseSlug.UseVisualStyleBackColor = true;
            this.chkUseSlug.CheckedChanged += new System.EventHandler(this.chkUseSlug_CheckedChanged);
            // 
            // pnlSlug
            // 
            this.pnlSlug.Controls.Add(this.lblSlugType);
            this.pnlSlug.Controls.Add(this.rbSlugOnly);
            this.pnlSlug.Controls.Add(this.rbSlugLast);
            this.pnlSlug.Controls.Add(this.rbSlugFirst);
            this.pnlSlug.Location = new System.Drawing.Point(6, 154);
            this.pnlSlug.Name = "pnlSlug";
            this.pnlSlug.Size = new System.Drawing.Size(308, 26);
            this.pnlSlug.TabIndex = 17;
            // 
            // rbSlugOnly
            // 
            this.rbSlugOnly.AutoSize = true;
            this.rbSlugOnly.Location = new System.Drawing.Point(232, 6);
            this.rbSlugOnly.Name = "rbSlugOnly";
            this.rbSlugOnly.Size = new System.Drawing.Size(68, 17);
            this.rbSlugOnly.TabIndex = 20;
            this.rbSlugOnly.TabStop = true;
            this.rbSlugOnly.Text = "Slug only";
            this.rbSlugOnly.UseVisualStyleBackColor = true;
            // 
            // rbSlugLast
            // 
            this.rbSlugLast.AutoSize = true;
            this.rbSlugLast.Location = new System.Drawing.Point(134, 6);
            this.rbSlugLast.Name = "rbSlugLast";
            this.rbSlugLast.Size = new System.Drawing.Size(92, 17);
            this.rbSlugLast.TabIndex = 19;
            this.rbSlugLast.TabStop = true;
            this.rbSlugLast.Text = "Thread ID first";
            this.rbSlugLast.UseVisualStyleBackColor = true;
            // 
            // rbSlugFirst
            // 
            this.rbSlugFirst.AutoSize = true;
            this.rbSlugFirst.Location = new System.Drawing.Point(63, 6);
            this.rbSlugFirst.Name = "rbSlugFirst";
            this.rbSlugFirst.Size = new System.Drawing.Size(65, 17);
            this.rbSlugFirst.TabIndex = 18;
            this.rbSlugFirst.TabStop = true;
            this.rbSlugFirst.Text = "Slug first";
            this.rbSlugFirst.UseVisualStyleBackColor = true;
            // 
            // chkRenameDownloadFolderWithCategory
            // 
            this.chkRenameDownloadFolderWithCategory.AutoSize = true;
            this.chkRenameDownloadFolderWithCategory.Location = new System.Drawing.Point(91, 19);
            this.chkRenameDownloadFolderWithCategory.Name = "chkRenameDownloadFolderWithCategory";
            this.chkRenameDownloadFolderWithCategory.Size = new System.Drawing.Size(68, 17);
            this.chkRenameDownloadFolderWithCategory.TabIndex = 18;
            this.chkRenameDownloadFolderWithCategory.Text = "Category";
            this.chkRenameDownloadFolderWithCategory.UseVisualStyleBackColor = true;
            // 
            // chkRenameDownloadFolderWithParentThreadDescription
            // 
            this.chkRenameDownloadFolderWithParentThreadDescription.AutoSize = true;
            this.chkRenameDownloadFolderWithParentThreadDescription.Location = new System.Drawing.Point(6, 42);
            this.chkRenameDownloadFolderWithParentThreadDescription.Name = "chkRenameDownloadFolderWithParentThreadDescription";
            this.chkRenameDownloadFolderWithParentThreadDescription.Size = new System.Drawing.Size(144, 17);
            this.chkRenameDownloadFolderWithParentThreadDescription.TabIndex = 20;
            this.chkRenameDownloadFolderWithParentThreadDescription.Text = "Parent thread description";
            this.chkRenameDownloadFolderWithParentThreadDescription.UseVisualStyleBackColor = true;
            this.chkRenameDownloadFolderWithParentThreadDescription.CheckedChanged += new System.EventHandler(this.chkRenameDownloadFolderWithParentThreadDescription_CheckedChanged);
            // 
            // lblParentThreadDescriptionFormat
            // 
            this.lblParentThreadDescriptionFormat.AutoSize = true;
            this.lblParentThreadDescriptionFormat.Location = new System.Drawing.Point(3, 7);
            this.lblParentThreadDescriptionFormat.Name = "lblParentThreadDescriptionFormat";
            this.lblParentThreadDescriptionFormat.Size = new System.Drawing.Size(42, 13);
            this.lblParentThreadDescriptionFormat.TabIndex = 21;
            this.lblParentThreadDescriptionFormat.Text = "Format:";
            // 
            // txtParentThreadDescriptionFormat
            // 
            this.txtParentThreadDescriptionFormat.Location = new System.Drawing.Point(51, 3);
            this.txtParentThreadDescriptionFormat.Name = "txtParentThreadDescriptionFormat";
            this.txtParentThreadDescriptionFormat.Size = new System.Drawing.Size(150, 20);
            this.txtParentThreadDescriptionFormat.TabIndex = 22;
            // 
            // chkSortImagesByPoster
            // 
            this.chkSortImagesByPoster.AutoSize = true;
            this.chkSortImagesByPoster.Location = new System.Drawing.Point(6, 92);
            this.chkSortImagesByPoster.Name = "chkSortImagesByPoster";
            this.chkSortImagesByPoster.Size = new System.Drawing.Size(272, 17);
            this.chkSortImagesByPoster.TabIndex = 23;
            this.chkSortImagesByPoster.Text = "Sort images by poster (only supported for some sites)";
            this.chkSortImagesByPoster.UseVisualStyleBackColor = true;
            // 
            // grpThreadFolderNaming
            // 
            this.grpThreadFolderNaming.Controls.Add(this.pnlParentThreadDescriptionFormat);
            this.grpThreadFolderNaming.Controls.Add(this.chkRenameDownloadFolderWithDescription);
            this.grpThreadFolderNaming.Controls.Add(this.chkRenameDownloadFolderWithCategory);
            this.grpThreadFolderNaming.Controls.Add(this.chkRenameDownloadFolderWithParentThreadDescription);
            this.grpThreadFolderNaming.Location = new System.Drawing.Point(6, 19);
            this.grpThreadFolderNaming.Name = "grpThreadFolderNaming";
            this.grpThreadFolderNaming.Size = new System.Drawing.Size(365, 67);
            this.grpThreadFolderNaming.TabIndex = 24;
            this.grpThreadFolderNaming.TabStop = false;
            this.grpThreadFolderNaming.Text = "Thread Folder Naming";
            // 
            // grpNamingStructure
            // 
            this.grpNamingStructure.Controls.Add(this.grpThreadFolderNaming);
            this.grpNamingStructure.Controls.Add(this.pnlSlug);
            this.grpNamingStructure.Controls.Add(this.chkSortImagesByPoster);
            this.grpNamingStructure.Controls.Add(this.chkUseSlug);
            this.grpNamingStructure.Controls.Add(this.chkUseOriginalFileNames);
            this.grpNamingStructure.Location = new System.Drawing.Point(12, 66);
            this.grpNamingStructure.Name = "grpNamingStructure";
            this.grpNamingStructure.Size = new System.Drawing.Size(379, 194);
            this.grpNamingStructure.TabIndex = 25;
            this.grpNamingStructure.TabStop = false;
            this.grpNamingStructure.Text = "Naming and Structure";
            // 
            // grpGeneral
            // 
            this.grpGeneral.Controls.Add(this.chkInterBoardAutoFollow);
            this.grpGeneral.Controls.Add(this.chkRecursiveAutoFollow);
            this.grpGeneral.Controls.Add(this.chkSaveThumbnails);
            this.grpGeneral.Controls.Add(this.chkVerifyImageHashes);
            this.grpGeneral.Controls.Add(this.chkCheckForUpdates);
            this.grpGeneral.Location = new System.Drawing.Point(397, 66);
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Size = new System.Drawing.Size(399, 132);
            this.grpGeneral.TabIndex = 26;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "General";
            // 
            // chkRecursiveAutoFollow
            // 
            this.chkRecursiveAutoFollow.AutoSize = true;
            this.chkRecursiveAutoFollow.Location = new System.Drawing.Point(6, 65);
            this.chkRecursiveAutoFollow.Name = "chkRecursiveAutoFollow";
            this.chkRecursiveAutoFollow.Size = new System.Drawing.Size(343, 17);
            this.chkRecursiveAutoFollow.TabIndex = 11;
            this.chkRecursiveAutoFollow.Text = "Recursively auto-follow child threads (only supported for some sites)";
            this.chkRecursiveAutoFollow.UseVisualStyleBackColor = true;
            // 
            // chkInterBoardAutoFollow
            // 
            this.chkInterBoardAutoFollow.AutoSize = true;
            this.chkInterBoardAutoFollow.Location = new System.Drawing.Point(6, 86);
            this.chkInterBoardAutoFollow.Name = "chkInterBoardAutoFollow";
            this.chkInterBoardAutoFollow.Size = new System.Drawing.Size(372, 17);
            this.chkInterBoardAutoFollow.TabIndex = 12;
            this.chkInterBoardAutoFollow.Text = "Follow threads outside of the current board (only supported for some sites)";
            this.chkInterBoardAutoFollow.UseVisualStyleBackColor = true;
            // 
            // lblSlugType
            // 
            this.lblSlugType.AutoSize = true;
            this.lblSlugType.Location = new System.Drawing.Point(3, 8);
            this.lblSlugType.Name = "lblSlugType";
            this.lblSlugType.Size = new System.Drawing.Size(54, 13);
            this.lblSlugType.TabIndex = 21;
            this.lblSlugType.Text = "Slug type:";
            // 
            // pnlParentThreadDescriptionFormat
            // 
            this.pnlParentThreadDescriptionFormat.Controls.Add(this.lblParentThreadDescriptionFormat);
            this.pnlParentThreadDescriptionFormat.Controls.Add(this.txtParentThreadDescriptionFormat);
            this.pnlParentThreadDescriptionFormat.Location = new System.Drawing.Point(156, 36);
            this.pnlParentThreadDescriptionFormat.Name = "pnlParentThreadDescriptionFormat";
            this.pnlParentThreadDescriptionFormat.Size = new System.Drawing.Size(208, 25);
            this.pnlParentThreadDescriptionFormat.TabIndex = 25;
            // 
            // frmSettings
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(808, 304);
            this.Controls.Add(this.grpGeneral);
            this.Controls.Add(this.grpNamingStructure);
            this.Controls.Add(this.rbSettingsInExeFolder);
            this.Controls.Add(this.rbSettingsInAppDataFolder);
            this.Controls.Add(this.lblSettingsLocation);
            this.Controls.Add(this.chkRelativePath);
            this.Controls.Add(this.txtCustomUserAgent);
            this.Controls.Add(this.chkCustomUserAgent);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblDownloadFolder);
            this.Controls.Add(this.txtDownloadFolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.frmSettings_Load);
            this.pnlSlug.ResumeLayout(false);
            this.pnlSlug.PerformLayout();
            this.grpThreadFolderNaming.ResumeLayout(false);
            this.grpThreadFolderNaming.PerformLayout();
            this.grpNamingStructure.ResumeLayout(false);
            this.grpNamingStructure.PerformLayout();
            this.grpGeneral.ResumeLayout(false);
            this.grpGeneral.PerformLayout();
            this.pnlParentThreadDescriptionFormat.ResumeLayout(false);
            this.pnlParentThreadDescriptionFormat.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDownloadFolder;
        private System.Windows.Forms.Label lblDownloadFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkCustomUserAgent;
        private System.Windows.Forms.TextBox txtCustomUserAgent;
        private System.Windows.Forms.CheckBox chkRelativePath;
        private System.Windows.Forms.Label lblSettingsLocation;
        private System.Windows.Forms.RadioButton rbSettingsInAppDataFolder;
        private System.Windows.Forms.RadioButton rbSettingsInExeFolder;
        private System.Windows.Forms.CheckBox chkUseOriginalFileNames;
        private System.Windows.Forms.CheckBox chkVerifyImageHashes;
        private System.Windows.Forms.CheckBox chkCheckForUpdates;
        private System.Windows.Forms.CheckBox chkSaveThumbnails;
        private System.Windows.Forms.CheckBox chkRenameDownloadFolderWithDescription;
        private System.Windows.Forms.CheckBox chkUseSlug;
        private System.Windows.Forms.Panel pnlSlug;
        private System.Windows.Forms.RadioButton rbSlugOnly;
        private System.Windows.Forms.RadioButton rbSlugLast;
        private System.Windows.Forms.RadioButton rbSlugFirst;
        private System.Windows.Forms.CheckBox chkRenameDownloadFolderWithCategory;
        private System.Windows.Forms.CheckBox chkRenameDownloadFolderWithParentThreadDescription;
        private System.Windows.Forms.Label lblParentThreadDescriptionFormat;
        private System.Windows.Forms.TextBox txtParentThreadDescriptionFormat;
        private System.Windows.Forms.CheckBox chkSortImagesByPoster;
        private System.Windows.Forms.GroupBox grpThreadFolderNaming;
        private System.Windows.Forms.GroupBox grpNamingStructure;
        private System.Windows.Forms.GroupBox grpGeneral;
        private System.Windows.Forms.CheckBox chkRecursiveAutoFollow;
        private System.Windows.Forms.CheckBox chkInterBoardAutoFollow;
        private System.Windows.Forms.Label lblSlugType;
        private System.Windows.Forms.Panel pnlParentThreadDescriptionFormat;
    }
}
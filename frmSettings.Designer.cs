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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSettings));
            this.txtDownloadFolder = new System.Windows.Forms.TextBox();
            this.lblDownloadFolder = new System.Windows.Forms.Label();
            this.btnDownloadFolder = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkCustomUserAgent = new System.Windows.Forms.CheckBox();
            this.txtCustomUserAgent = new System.Windows.Forms.TextBox();
            this.chkDownloadFolderRelative = new System.Windows.Forms.CheckBox();
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
            this.lblSlugType = new System.Windows.Forms.Label();
            this.rbSlugOnly = new System.Windows.Forms.RadioButton();
            this.rbSlugLast = new System.Windows.Forms.RadioButton();
            this.rbSlugFirst = new System.Windows.Forms.RadioButton();
            this.chkRenameDownloadFolderWithCategory = new System.Windows.Forms.CheckBox();
            this.chkRenameDownloadFolderWithParentThreadDescription = new System.Windows.Forms.CheckBox();
            this.lblParentThreadDescriptionFormat = new System.Windows.Forms.Label();
            this.txtParentThreadDescriptionFormat = new System.Windows.Forms.TextBox();
            this.chkSortImagesByPoster = new System.Windows.Forms.CheckBox();
            this.grpThreadFolderNaming = new System.Windows.Forms.GroupBox();
            this.pnlParentThreadDescriptionFormat = new System.Windows.Forms.Panel();
            this.grpNamingStructure = new System.Windows.Forms.GroupBox();
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.grpThreadStatus = new System.Windows.Forms.GroupBox();
            this.chkThreadStatus = new System.Windows.Forms.CheckBox();
            this.txtThreadStatusBoxThreshold = new System.Windows.Forms.TextBox();
            this.txtMaximumKilobytesPerSecond = new System.Windows.Forms.TextBox();
            this.lblMaximumKilobytesPerSecond = new System.Windows.Forms.Label();
            this.chkBlacklistWildcards = new System.Windows.Forms.CheckBox();
            this.chkMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.chkInterBoardAutoFollow = new System.Windows.Forms.CheckBox();
            this.chkRecursiveAutoFollow = new System.Windows.Forms.CheckBox();
            this.lblSupportedSitesNote = new System.Windows.Forms.Label();
            this.cboWindowTitle = new System.Windows.Forms.ComboBox();
            this.txtWindowTitle = new System.Windows.Forms.TextBox();
            this.btnBackupThreadList = new System.Windows.Forms.Button();
            this.pnlBackupEvery = new System.Windows.Forms.Panel();
            this.lblBackupEvery = new System.Windows.Forms.Label();
            this.txtBackupEvery = new System.Windows.Forms.TextBox();
            this.chkBackupThreadList = new System.Windows.Forms.CheckBox();
            this.grpBackup = new System.Windows.Forms.GroupBox();
            this.chkBackupCheckSize = new System.Windows.Forms.CheckBox();
            this.grpWindow = new System.Windows.Forms.GroupBox();
            this.pnlWindowTitle = new System.Windows.Forms.Panel();
            this.btnWindowTitle = new System.Windows.Forms.Button();
            this.txtCompletedFolder = new System.Windows.Forms.TextBox();
            this.btnCompletedFolder = new System.Windows.Forms.Button();
            this.chkCompletedFolder = new System.Windows.Forms.CheckBox();
            this.chkCompletedFolderRelative = new System.Windows.Forms.CheckBox();
            this.frmSettingsToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.pnlSlug.SuspendLayout();
            this.grpThreadFolderNaming.SuspendLayout();
            this.pnlParentThreadDescriptionFormat.SuspendLayout();
            this.grpNamingStructure.SuspendLayout();
            this.grpGeneral.SuspendLayout();
            this.grpThreadStatus.SuspendLayout();
            this.pnlBackupEvery.SuspendLayout();
            this.grpBackup.SuspendLayout();
            this.grpWindow.SuspendLayout();
            this.pnlWindowTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDownloadFolder
            // 
            this.txtDownloadFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDownloadFolder.Location = new System.Drawing.Point(108, 8);
            this.txtDownloadFolder.Name = "txtDownloadFolder";
            this.txtDownloadFolder.Size = new System.Drawing.Size(544, 20);
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
            // btnDownloadFolder
            // 
            this.btnDownloadFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownloadFolder.Location = new System.Drawing.Point(658, 8);
            this.btnDownloadFolder.Name = "btnDownloadFolder";
            this.btnDownloadFolder.Size = new System.Drawing.Size(80, 23);
            this.btnDownloadFolder.TabIndex = 2;
            this.btnDownloadFolder.Text = "Browse...";
            this.btnDownloadFolder.UseVisualStyleBackColor = true;
            this.btnDownloadFolder.Click += new System.EventHandler(this.btnDownloadFolder_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(698, 385);
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
            this.btnCancel.Location = new System.Drawing.Point(766, 385);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(68, 23);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // chkCustomUserAgent
            // 
            this.chkCustomUserAgent.AutoSize = true;
            this.chkCustomUserAgent.Location = new System.Drawing.Point(10, 66);
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
            this.txtCustomUserAgent.Location = new System.Drawing.Point(140, 64);
            this.txtCustomUserAgent.Name = "txtCustomUserAgent";
            this.txtCustomUserAgent.Size = new System.Drawing.Size(692, 20);
            this.txtCustomUserAgent.TabIndex = 5;
            // 
            // chkDownloadFolderRelative
            // 
            this.chkDownloadFolderRelative.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDownloadFolderRelative.AutoSize = true;
            this.chkDownloadFolderRelative.Location = new System.Drawing.Point(746, 12);
            this.chkDownloadFolderRelative.Name = "chkDownloadFolderRelative";
            this.chkDownloadFolderRelative.Size = new System.Drawing.Size(89, 17);
            this.chkDownloadFolderRelative.TabIndex = 3;
            this.chkDownloadFolderRelative.Text = "Relative path";
            this.chkDownloadFolderRelative.UseVisualStyleBackColor = true;
            this.chkDownloadFolderRelative.CheckedChanged += new System.EventHandler(this.chkDownloadFolderRelative_CheckedChanged);
            // 
            // lblSettingsLocation
            // 
            this.lblSettingsLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSettingsLocation.AutoSize = true;
            this.lblSettingsLocation.Location = new System.Drawing.Point(8, 393);
            this.lblSettingsLocation.Name = "lblSettingsLocation";
            this.lblSettingsLocation.Size = new System.Drawing.Size(85, 13);
            this.lblSettingsLocation.TabIndex = 11;
            this.lblSettingsLocation.Text = "Save settings in:";
            // 
            // rbSettingsInAppDataFolder
            // 
            this.rbSettingsInAppDataFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbSettingsInAppDataFolder.AutoSize = true;
            this.rbSettingsInAppDataFolder.Location = new System.Drawing.Point(108, 391);
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
            this.rbSettingsInExeFolder.Location = new System.Drawing.Point(252, 391);
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
            this.chkUseOriginalFileNames.Size = new System.Drawing.Size(135, 17);
            this.chkUseOriginalFileNames.TabIndex = 8;
            this.chkUseOriginalFileNames.Text = "Use original filenames *";
            this.frmSettingsToolTip.SetToolTip(this.chkUseOriginalFileNames, "Only supported for some sites");
            this.chkUseOriginalFileNames.UseVisualStyleBackColor = true;
            // 
            // chkVerifyImageHashes
            // 
            this.chkVerifyImageHashes.AutoSize = true;
            this.chkVerifyImageHashes.Location = new System.Drawing.Point(6, 55);
            this.chkVerifyImageHashes.Name = "chkVerifyImageHashes";
            this.chkVerifyImageHashes.Size = new System.Drawing.Size(127, 17);
            this.chkVerifyImageHashes.TabIndex = 9;
            this.chkVerifyImageHashes.Text = "Verify image hashes *";
            this.frmSettingsToolTip.SetToolTip(this.chkVerifyImageHashes, "Only supported for some sites");
            this.chkVerifyImageHashes.UseVisualStyleBackColor = true;
            // 
            // chkCheckForUpdates
            // 
            this.chkCheckForUpdates.AutoSize = true;
            this.chkCheckForUpdates.Location = new System.Drawing.Point(206, 42);
            this.chkCheckForUpdates.Name = "chkCheckForUpdates";
            this.chkCheckForUpdates.Size = new System.Drawing.Size(197, 30);
            this.chkCheckForUpdates.TabIndex = 10;
            this.chkCheckForUpdates.Text = "Automatically check for and notify of\r\nupdated versions of this program";
            this.chkCheckForUpdates.UseVisualStyleBackColor = true;
            // 
            // chkSaveThumbnails
            // 
            this.chkSaveThumbnails.AutoSize = true;
            this.chkSaveThumbnails.Location = new System.Drawing.Point(6, 19);
            this.chkSaveThumbnails.Name = "chkSaveThumbnails";
            this.chkSaveThumbnails.Size = new System.Drawing.Size(190, 30);
            this.chkSaveThumbnails.TabIndex = 6;
            this.chkSaveThumbnails.Text = "Save thumbnails and Post-Process\r\nHTML";
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
            this.chkUseSlug.Size = new System.Drawing.Size(147, 17);
            this.chkUseSlug.TabIndex = 16;
            this.chkUseSlug.Text = "Use slug in thread name *";
            this.frmSettingsToolTip.SetToolTip(this.chkUseSlug, "Only supported for some sites");
            this.chkUseSlug.UseVisualStyleBackColor = true;
            this.chkUseSlug.CheckedChanged += new System.EventHandler(this.chkUseSlug_CheckedChanged);
            // 
            // pnlSlug
            // 
            this.pnlSlug.Controls.Add(this.lblSlugType);
            this.pnlSlug.Controls.Add(this.rbSlugOnly);
            this.pnlSlug.Controls.Add(this.rbSlugLast);
            this.pnlSlug.Controls.Add(this.rbSlugFirst);
            this.pnlSlug.Location = new System.Drawing.Point(6, 161);
            this.pnlSlug.Name = "pnlSlug";
            this.pnlSlug.Size = new System.Drawing.Size(308, 26);
            this.pnlSlug.TabIndex = 17;
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
            this.chkSortImagesByPoster.Size = new System.Drawing.Size(134, 17);
            this.chkSortImagesByPoster.TabIndex = 23;
            this.chkSortImagesByPoster.Text = "Sort images by poster *";
            this.frmSettingsToolTip.SetToolTip(this.chkSortImagesByPoster, "Only supported for some sites");
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
            // pnlParentThreadDescriptionFormat
            // 
            this.pnlParentThreadDescriptionFormat.Controls.Add(this.lblParentThreadDescriptionFormat);
            this.pnlParentThreadDescriptionFormat.Controls.Add(this.txtParentThreadDescriptionFormat);
            this.pnlParentThreadDescriptionFormat.Location = new System.Drawing.Point(156, 36);
            this.pnlParentThreadDescriptionFormat.Name = "pnlParentThreadDescriptionFormat";
            this.pnlParentThreadDescriptionFormat.Size = new System.Drawing.Size(208, 25);
            this.pnlParentThreadDescriptionFormat.TabIndex = 25;
            // 
            // grpNamingStructure
            // 
            this.grpNamingStructure.Controls.Add(this.grpThreadFolderNaming);
            this.grpNamingStructure.Controls.Add(this.pnlSlug);
            this.grpNamingStructure.Controls.Add(this.chkSortImagesByPoster);
            this.grpNamingStructure.Controls.Add(this.chkUseSlug);
            this.grpNamingStructure.Controls.Add(this.chkUseOriginalFileNames);
            this.grpNamingStructure.Location = new System.Drawing.Point(12, 90);
            this.grpNamingStructure.Name = "grpNamingStructure";
            this.grpNamingStructure.Size = new System.Drawing.Size(379, 194);
            this.grpNamingStructure.TabIndex = 25;
            this.grpNamingStructure.TabStop = false;
            this.grpNamingStructure.Text = "Naming and Structure";
            // 
            // grpGeneral
            // 
            this.grpGeneral.Controls.Add(this.grpThreadStatus);
            this.grpGeneral.Controls.Add(this.txtMaximumKilobytesPerSecond);
            this.grpGeneral.Controls.Add(this.lblMaximumKilobytesPerSecond);
            this.grpGeneral.Controls.Add(this.chkBlacklistWildcards);
            this.grpGeneral.Controls.Add(this.chkMinimizeToTray);
            this.grpGeneral.Controls.Add(this.chkInterBoardAutoFollow);
            this.grpGeneral.Controls.Add(this.chkRecursiveAutoFollow);
            this.grpGeneral.Controls.Add(this.chkSaveThumbnails);
            this.grpGeneral.Controls.Add(this.chkVerifyImageHashes);
            this.grpGeneral.Controls.Add(this.chkCheckForUpdates);
            this.grpGeneral.Location = new System.Drawing.Point(397, 90);
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Size = new System.Drawing.Size(437, 204);
            this.grpGeneral.TabIndex = 26;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "General";
            // 
            // grpThreadStatus
            // 
            this.grpThreadStatus.Controls.Add(this.chkThreadStatus);
            this.grpThreadStatus.Controls.Add(this.txtThreadStatusBoxThreshold);
            this.grpThreadStatus.Location = new System.Drawing.Point(6, 114);
            this.grpThreadStatus.Name = "grpThreadStatus";
            this.grpThreadStatus.Size = new System.Drawing.Size(172, 52);
            this.grpThreadStatus.TabIndex = 41;
            this.grpThreadStatus.TabStop = false;
            this.grpThreadStatus.Text = "Thread Status Format";
            this.frmSettingsToolTip.SetToolTip(this.grpThreadStatus, resources.GetString("grpThreadStatus.ToolTip"));
            // 
            // chkThreadStatus
            // 
            this.chkThreadStatus.AutoSize = true;
            this.chkThreadStatus.Location = new System.Drawing.Point(6, 19);
            this.chkThreadStatus.Name = "chkThreadStatus";
            this.chkThreadStatus.Size = new System.Drawing.Size(93, 17);
            this.chkThreadStatus.TabIndex = 42;
            this.chkThreadStatus.Text = "Show Minutes";
            this.frmSettingsToolTip.SetToolTip(this.chkThreadStatus, resources.GetString("chkThreadStatus.ToolTip"));
            this.chkThreadStatus.UseVisualStyleBackColor = true;
            this.chkThreadStatus.CheckedChanged += new System.EventHandler(this.chkThreadStatus_CheckedChanged);
            // 
            // txtThreadStatusBoxThreshold
            // 
            this.txtThreadStatusBoxThreshold.AccessibleDescription = "";
            this.txtThreadStatusBoxThreshold.Location = new System.Drawing.Point(105, 17);
            this.txtThreadStatusBoxThreshold.MaxLength = 4;
            this.txtThreadStatusBoxThreshold.Name = "txtThreadStatusBoxThreshold";
            this.txtThreadStatusBoxThreshold.Size = new System.Drawing.Size(61, 20);
            this.txtThreadStatusBoxThreshold.TabIndex = 38;
            this.frmSettingsToolTip.SetToolTip(this.txtThreadStatusBoxThreshold, resources.GetString("txtThreadStatusBoxThreshold.ToolTip"));
            this.txtThreadStatusBoxThreshold.Leave += new System.EventHandler(this.txtThreadStatusBoxThreshold_Leave);
            // 
            // txtMaximumKilobytesPerSecond
            // 
            this.txtMaximumKilobytesPerSecond.Location = new System.Drawing.Point(241, 178);
            this.txtMaximumKilobytesPerSecond.Name = "txtMaximumKilobytesPerSecond";
            this.txtMaximumKilobytesPerSecond.Size = new System.Drawing.Size(56, 20);
            this.txtMaximumKilobytesPerSecond.TabIndex = 32;
            this.txtMaximumKilobytesPerSecond.Leave += new System.EventHandler(this.txtMaximumKilobytesPerSecond_Leave);
            // 
            // lblMaximumKilobytesPerSecond
            // 
            this.lblMaximumKilobytesPerSecond.AutoSize = true;
            this.lblMaximumKilobytesPerSecond.Location = new System.Drawing.Point(3, 181);
            this.lblMaximumKilobytesPerSecond.Name = "lblMaximumKilobytesPerSecond";
            this.lblMaximumKilobytesPerSecond.Size = new System.Drawing.Size(232, 13);
            this.lblMaximumKilobytesPerSecond.TabIndex = 33;
            this.lblMaximumKilobytesPerSecond.Text = "Maximum download speed (kB/s, 0 = unlimited):";
            // 
            // chkBlacklistWildcards
            // 
            this.chkBlacklistWildcards.AutoSize = true;
            this.chkBlacklistWildcards.Location = new System.Drawing.Point(206, 78);
            this.chkBlacklistWildcards.Name = "chkBlacklistWildcards";
            this.chkBlacklistWildcards.Size = new System.Drawing.Size(147, 17);
            this.chkBlacklistWildcards.TabIndex = 28;
            this.chkBlacklistWildcards.Text = "Enable blacklist wildcards";
            this.chkBlacklistWildcards.UseVisualStyleBackColor = true;
            // 
            // chkMinimizeToTray
            // 
            this.chkMinimizeToTray.AutoSize = true;
            this.chkMinimizeToTray.Location = new System.Drawing.Point(333, 181);
            this.chkMinimizeToTray.Name = "chkMinimizeToTray";
            this.chkMinimizeToTray.Size = new System.Drawing.Size(98, 17);
            this.chkMinimizeToTray.TabIndex = 27;
            this.chkMinimizeToTray.Text = "Minimize to tray";
            this.chkMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // chkInterBoardAutoFollow
            // 
            this.chkInterBoardAutoFollow.AutoSize = true;
            this.chkInterBoardAutoFollow.Location = new System.Drawing.Point(6, 78);
            this.chkInterBoardAutoFollow.Name = "chkInterBoardAutoFollow";
            this.chkInterBoardAutoFollow.Size = new System.Drawing.Size(161, 30);
            this.chkInterBoardAutoFollow.TabIndex = 12;
            this.chkInterBoardAutoFollow.Text = "Follow threads outside of the\r\ncurrent board *";
            this.frmSettingsToolTip.SetToolTip(this.chkInterBoardAutoFollow, "Only supported for some sites");
            this.chkInterBoardAutoFollow.UseVisualStyleBackColor = true;
            // 
            // chkRecursiveAutoFollow
            // 
            this.chkRecursiveAutoFollow.AutoSize = true;
            this.chkRecursiveAutoFollow.Location = new System.Drawing.Point(206, 19);
            this.chkRecursiveAutoFollow.Name = "chkRecursiveAutoFollow";
            this.chkRecursiveAutoFollow.Size = new System.Drawing.Size(205, 17);
            this.chkRecursiveAutoFollow.TabIndex = 11;
            this.chkRecursiveAutoFollow.Text = "Recursively auto-follow child threads *";
            this.frmSettingsToolTip.SetToolTip(this.chkRecursiveAutoFollow, "Only supported for some sites");
            this.chkRecursiveAutoFollow.UseVisualStyleBackColor = true;
            // 
            // lblSupportedSitesNote
            // 
            this.lblSupportedSitesNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSupportedSitesNote.AutoSize = true;
            this.lblSupportedSitesNote.Location = new System.Drawing.Point(394, 390);
            this.lblSupportedSitesNote.Name = "lblSupportedSitesNote";
            this.lblSupportedSitesNote.Size = new System.Drawing.Size(152, 13);
            this.lblSupportedSitesNote.TabIndex = 40;
            this.lblSupportedSitesNote.Text = "* Only supported for some sites";
            // 
            // cboWindowTitle
            // 
            this.cboWindowTitle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWindowTitle.FormattingEnabled = true;
            this.cboWindowTitle.Location = new System.Drawing.Point(71, 29);
            this.cboWindowTitle.Name = "cboWindowTitle";
            this.cboWindowTitle.Size = new System.Drawing.Size(128, 21);
            this.cboWindowTitle.TabIndex = 36;
            // 
            // txtWindowTitle
            // 
            this.txtWindowTitle.Location = new System.Drawing.Point(71, 3);
            this.txtWindowTitle.Name = "txtWindowTitle";
            this.txtWindowTitle.Size = new System.Drawing.Size(351, 20);
            this.txtWindowTitle.TabIndex = 35;
            // 
            // btnBackupThreadList
            // 
            this.btnBackupThreadList.AutoSize = true;
            this.btnBackupThreadList.Location = new System.Drawing.Point(289, 47);
            this.btnBackupThreadList.Name = "btnBackupThreadList";
            this.btnBackupThreadList.Size = new System.Drawing.Size(84, 23);
            this.btnBackupThreadList.TabIndex = 27;
            this.btnBackupThreadList.Text = "Force Backup";
            this.btnBackupThreadList.UseVisualStyleBackColor = true;
            this.btnBackupThreadList.Click += new System.EventHandler(this.btnBackupThreadList_Click);
            // 
            // pnlBackupEvery
            // 
            this.pnlBackupEvery.Controls.Add(this.lblBackupEvery);
            this.pnlBackupEvery.Controls.Add(this.txtBackupEvery);
            this.pnlBackupEvery.Location = new System.Drawing.Point(196, 15);
            this.pnlBackupEvery.Name = "pnlBackupEvery";
            this.pnlBackupEvery.Size = new System.Drawing.Size(177, 26);
            this.pnlBackupEvery.TabIndex = 32;
            // 
            // lblBackupEvery
            // 
            this.lblBackupEvery.AutoSize = true;
            this.lblBackupEvery.Location = new System.Drawing.Point(3, 5);
            this.lblBackupEvery.Name = "lblBackupEvery";
            this.lblBackupEvery.Size = new System.Drawing.Size(121, 13);
            this.lblBackupEvery.TabIndex = 30;
            this.lblBackupEvery.Text = "Backup every (minutes):";
            // 
            // txtBackupEvery
            // 
            this.txtBackupEvery.Location = new System.Drawing.Point(130, 2);
            this.txtBackupEvery.Name = "txtBackupEvery";
            this.txtBackupEvery.Size = new System.Drawing.Size(40, 20);
            this.txtBackupEvery.TabIndex = 31;
            this.txtBackupEvery.Leave += new System.EventHandler(this.txtBackupEvery_Leave);
            // 
            // chkBackupThreadList
            // 
            this.chkBackupThreadList.AutoSize = true;
            this.chkBackupThreadList.Location = new System.Drawing.Point(6, 24);
            this.chkBackupThreadList.Name = "chkBackupThreadList";
            this.chkBackupThreadList.Size = new System.Drawing.Size(111, 17);
            this.chkBackupThreadList.TabIndex = 29;
            this.chkBackupThreadList.Text = "Backup thread list";
            this.chkBackupThreadList.UseVisualStyleBackColor = true;
            this.chkBackupThreadList.CheckedChanged += new System.EventHandler(this.chkBackupThreadList_CheckedChanged);
            // 
            // grpBackup
            // 
            this.grpBackup.Controls.Add(this.chkBackupCheckSize);
            this.grpBackup.Controls.Add(this.chkBackupThreadList);
            this.grpBackup.Controls.Add(this.btnBackupThreadList);
            this.grpBackup.Controls.Add(this.pnlBackupEvery);
            this.grpBackup.Location = new System.Drawing.Point(12, 290);
            this.grpBackup.Name = "grpBackup";
            this.grpBackup.Size = new System.Drawing.Size(379, 76);
            this.grpBackup.TabIndex = 26;
            this.grpBackup.TabStop = false;
            this.grpBackup.Text = "Backup";
            // 
            // chkBackupCheckSize
            // 
            this.chkBackupCheckSize.AutoSize = true;
            this.chkBackupCheckSize.Location = new System.Drawing.Point(6, 47);
            this.chkBackupCheckSize.Name = "chkBackupCheckSize";
            this.chkBackupCheckSize.Size = new System.Drawing.Size(207, 17);
            this.chkBackupCheckSize.TabIndex = 33;
            this.chkBackupCheckSize.Text = "Do not backup if current size is smaller";
            this.chkBackupCheckSize.UseVisualStyleBackColor = true;
            // 
            // grpWindow
            // 
            this.grpWindow.Controls.Add(this.pnlWindowTitle);
            this.grpWindow.Location = new System.Drawing.Point(397, 300);
            this.grpWindow.Name = "grpWindow";
            this.grpWindow.Size = new System.Drawing.Size(437, 77);
            this.grpWindow.TabIndex = 34;
            this.grpWindow.TabStop = false;
            this.grpWindow.Text = "Window";
            // 
            // pnlWindowTitle
            // 
            this.pnlWindowTitle.Controls.Add(this.btnWindowTitle);
            this.pnlWindowTitle.Controls.Add(this.cboWindowTitle);
            this.pnlWindowTitle.Controls.Add(this.txtWindowTitle);
            this.pnlWindowTitle.Location = new System.Drawing.Point(6, 16);
            this.pnlWindowTitle.Name = "pnlWindowTitle";
            this.pnlWindowTitle.Size = new System.Drawing.Size(425, 54);
            this.pnlWindowTitle.TabIndex = 32;
            // 
            // btnWindowTitle
            // 
            this.btnWindowTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWindowTitle.Location = new System.Drawing.Point(6, 28);
            this.btnWindowTitle.Name = "btnWindowTitle";
            this.btnWindowTitle.Size = new System.Drawing.Size(60, 23);
            this.btnWindowTitle.TabIndex = 35;
            this.btnWindowTitle.Text = "Insert";
            this.btnWindowTitle.UseVisualStyleBackColor = true;
            this.btnWindowTitle.Click += new System.EventHandler(this.btnWindowTitle_Click);
            // 
            // txtCompletedFolder
            // 
            this.txtCompletedFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCompletedFolder.Location = new System.Drawing.Point(180, 36);
            this.txtCompletedFolder.Name = "txtCompletedFolder";
            this.txtCompletedFolder.Size = new System.Drawing.Size(472, 20);
            this.txtCompletedFolder.TabIndex = 36;
            // 
            // btnCompletedFolder
            // 
            this.btnCompletedFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCompletedFolder.Location = new System.Drawing.Point(658, 34);
            this.btnCompletedFolder.Name = "btnCompletedFolder";
            this.btnCompletedFolder.Size = new System.Drawing.Size(80, 23);
            this.btnCompletedFolder.TabIndex = 37;
            this.btnCompletedFolder.Text = "Browse...";
            this.btnCompletedFolder.UseVisualStyleBackColor = true;
            this.btnCompletedFolder.Click += new System.EventHandler(this.btnCompletedFolder_Click);
            // 
            // chkCompletedFolder
            // 
            this.chkCompletedFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCompletedFolder.AutoSize = true;
            this.chkCompletedFolder.Location = new System.Drawing.Point(10, 39);
            this.chkCompletedFolder.Name = "chkCompletedFolder";
            this.chkCompletedFolder.Size = new System.Drawing.Size(164, 17);
            this.chkCompletedFolder.TabIndex = 38;
            this.chkCompletedFolder.Text = "Move on Remove Completed";
            this.chkCompletedFolder.UseVisualStyleBackColor = true;
            this.chkCompletedFolder.CheckedChanged += new System.EventHandler(this.chkCompletedFolder_CheckedChanged);
            // 
            // chkCompletedFolderRelative
            // 
            this.chkCompletedFolderRelative.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCompletedFolderRelative.AutoSize = true;
            this.chkCompletedFolderRelative.Location = new System.Drawing.Point(746, 38);
            this.chkCompletedFolderRelative.Name = "chkCompletedFolderRelative";
            this.chkCompletedFolderRelative.Size = new System.Drawing.Size(89, 17);
            this.chkCompletedFolderRelative.TabIndex = 39;
            this.chkCompletedFolderRelative.Text = "Relative path";
            this.chkCompletedFolderRelative.UseVisualStyleBackColor = true;
            this.chkCompletedFolderRelative.CheckedChanged += new System.EventHandler(this.chkCompletedFolderRelative_CheckedChanged);
            // 
            // frmSettings
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(844, 418);
            this.Controls.Add(this.lblSupportedSitesNote);
            this.Controls.Add(this.chkCompletedFolderRelative);
            this.Controls.Add(this.chkCompletedFolder);
            this.Controls.Add(this.btnCompletedFolder);
            this.Controls.Add(this.txtCompletedFolder);
            this.Controls.Add(this.grpWindow);
            this.Controls.Add(this.grpBackup);
            this.Controls.Add(this.grpGeneral);
            this.Controls.Add(this.grpNamingStructure);
            this.Controls.Add(this.rbSettingsInExeFolder);
            this.Controls.Add(this.rbSettingsInAppDataFolder);
            this.Controls.Add(this.lblSettingsLocation);
            this.Controls.Add(this.chkDownloadFolderRelative);
            this.Controls.Add(this.txtCustomUserAgent);
            this.Controls.Add(this.chkCustomUserAgent);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnDownloadFolder);
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
            this.pnlParentThreadDescriptionFormat.ResumeLayout(false);
            this.pnlParentThreadDescriptionFormat.PerformLayout();
            this.grpNamingStructure.ResumeLayout(false);
            this.grpNamingStructure.PerformLayout();
            this.grpGeneral.ResumeLayout(false);
            this.grpGeneral.PerformLayout();
            this.grpThreadStatus.ResumeLayout(false);
            this.grpThreadStatus.PerformLayout();
            this.pnlBackupEvery.ResumeLayout(false);
            this.pnlBackupEvery.PerformLayout();
            this.grpBackup.ResumeLayout(false);
            this.grpBackup.PerformLayout();
            this.grpWindow.ResumeLayout(false);
            this.pnlWindowTitle.ResumeLayout(false);
            this.pnlWindowTitle.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDownloadFolder;
        private System.Windows.Forms.Label lblDownloadFolder;
        private System.Windows.Forms.Button btnDownloadFolder;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkCustomUserAgent;
        private System.Windows.Forms.TextBox txtCustomUserAgent;
        private System.Windows.Forms.CheckBox chkDownloadFolderRelative;
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
        private System.Windows.Forms.CheckBox chkMinimizeToTray;
        private System.Windows.Forms.CheckBox chkBlacklistWildcards;
        private System.Windows.Forms.CheckBox chkBackupThreadList;
        private System.Windows.Forms.Label lblBackupEvery;
        private System.Windows.Forms.TextBox txtBackupEvery;
        private System.Windows.Forms.Panel pnlBackupEvery;
        private System.Windows.Forms.Button btnBackupThreadList;
        private System.Windows.Forms.Label lblMaximumKilobytesPerSecond;
        private System.Windows.Forms.TextBox txtMaximumKilobytesPerSecond;
        private System.Windows.Forms.GroupBox grpBackup;
        private System.Windows.Forms.CheckBox chkBackupCheckSize;
        private System.Windows.Forms.TextBox txtWindowTitle;
        private System.Windows.Forms.ComboBox cboWindowTitle;
        private System.Windows.Forms.GroupBox grpWindow;
        private System.Windows.Forms.Panel pnlWindowTitle;
        private System.Windows.Forms.Button btnWindowTitle;
        private System.Windows.Forms.TextBox txtCompletedFolder;
        private System.Windows.Forms.Button btnCompletedFolder;
        private System.Windows.Forms.CheckBox chkCompletedFolder;
        private System.Windows.Forms.CheckBox chkCompletedFolderRelative;
        private System.Windows.Forms.Label lblSupportedSitesNote;
        private System.Windows.Forms.TextBox txtThreadStatusBoxThreshold;
        private System.Windows.Forms.ToolTip frmSettingsToolTip;
        private System.Windows.Forms.GroupBox grpThreadStatus;
        private System.Windows.Forms.CheckBox chkThreadStatus;
    }
}
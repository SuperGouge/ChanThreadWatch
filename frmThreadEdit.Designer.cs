namespace JDP {
    partial class frmThreadEdit {
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.pnlDescription = new System.Windows.Forms.Panel();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.pnlCategory = new System.Windows.Forms.Panel();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.chkAutoFollow = new System.Windows.Forms.CheckBox();
            this.pnlCheckEvery = new System.Windows.Forms.Panel();
            this.txtCheckEvery = new System.Windows.Forms.TextBox();
            this.cboCheckEvery = new System.Windows.Forms.ComboBox();
            this.lblCheckEvery = new System.Windows.Forms.Label();
            this.txtImageAuth = new System.Windows.Forms.TextBox();
            this.txtPageAuth = new System.Windows.Forms.TextBox();
            this.chkImageAuth = new System.Windows.Forms.CheckBox();
            this.chkPageAuth = new System.Windows.Forms.CheckBox();
            this.chkOneTime = new System.Windows.Forms.CheckBox();
            this.pnlEdit.SuspendLayout();
            this.pnlDescription.SuspendLayout();
            this.pnlCategory.SuspendLayout();
            this.pnlCheckEvery.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(187, 209);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(68, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.Location = new System.Drawing.Point(119, 209);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(60, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // pnlEdit
            // 
            this.pnlEdit.Controls.Add(this.pnlDescription);
            this.pnlEdit.Controls.Add(this.pnlCategory);
            this.pnlEdit.Controls.Add(this.chkAutoFollow);
            this.pnlEdit.Controls.Add(this.pnlCheckEvery);
            this.pnlEdit.Controls.Add(this.txtImageAuth);
            this.pnlEdit.Controls.Add(this.txtPageAuth);
            this.pnlEdit.Controls.Add(this.chkImageAuth);
            this.pnlEdit.Controls.Add(this.chkPageAuth);
            this.pnlEdit.Controls.Add(this.chkOneTime);
            this.pnlEdit.Location = new System.Drawing.Point(12, 12);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(350, 182);
            this.pnlEdit.TabIndex = 26;
            // 
            // pnlDescription
            // 
            this.pnlDescription.Controls.Add(this.txtDescription);
            this.pnlDescription.Controls.Add(this.lblDescription);
            this.pnlDescription.Location = new System.Drawing.Point(3, 3);
            this.pnlDescription.Name = "pnlDescription";
            this.pnlDescription.Size = new System.Drawing.Size(200, 29);
            this.pnlDescription.TabIndex = 39;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(72, 3);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(118, 20);
            this.txtDescription.TabIndex = 40;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(3, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(63, 13);
            this.lblDescription.TabIndex = 37;
            this.lblDescription.Text = "Description:";
            // 
            // pnlCategory
            // 
            this.pnlCategory.Controls.Add(this.lblCategory);
            this.pnlCategory.Controls.Add(this.cboCategory);
            this.pnlCategory.Location = new System.Drawing.Point(3, 38);
            this.pnlCategory.Name = "pnlCategory";
            this.pnlCategory.Size = new System.Drawing.Size(200, 29);
            this.pnlCategory.TabIndex = 38;
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 8);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 37;
            this.lblCategory.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(61, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(129, 21);
            this.cboCategory.Sorted = true;
            this.cboCategory.TabIndex = 36;
            // 
            // chkAutoFollow
            // 
            this.chkAutoFollow.AutoSize = true;
            this.chkAutoFollow.Location = new System.Drawing.Point(126, 129);
            this.chkAutoFollow.Name = "chkAutoFollow";
            this.chkAutoFollow.Size = new System.Drawing.Size(78, 17);
            this.chkAutoFollow.TabIndex = 36;
            this.chkAutoFollow.Text = "Auto-follow";
            this.chkAutoFollow.UseVisualStyleBackColor = true;
            // 
            // pnlCheckEvery
            // 
            this.pnlCheckEvery.Controls.Add(this.txtCheckEvery);
            this.pnlCheckEvery.Controls.Add(this.cboCheckEvery);
            this.pnlCheckEvery.Controls.Add(this.lblCheckEvery);
            this.pnlCheckEvery.Location = new System.Drawing.Point(3, 152);
            this.pnlCheckEvery.Name = "pnlCheckEvery";
            this.pnlCheckEvery.Size = new System.Drawing.Size(236, 25);
            this.pnlCheckEvery.TabIndex = 33;
            // 
            // txtCheckEvery
            // 
            this.txtCheckEvery.Location = new System.Drawing.Point(185, 0);
            this.txtCheckEvery.Name = "txtCheckEvery";
            this.txtCheckEvery.Size = new System.Drawing.Size(40, 20);
            this.txtCheckEvery.TabIndex = 10;
            // 
            // cboCheckEvery
            // 
            this.cboCheckEvery.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCheckEvery.FormattingEnabled = true;
            this.cboCheckEvery.Location = new System.Drawing.Point(124, 0);
            this.cboCheckEvery.Name = "cboCheckEvery";
            this.cboCheckEvery.Size = new System.Drawing.Size(55, 21);
            this.cboCheckEvery.TabIndex = 8;
            // 
            // lblCheckEvery
            // 
            this.lblCheckEvery.AutoSize = true;
            this.lblCheckEvery.Location = new System.Drawing.Point(3, 4);
            this.lblCheckEvery.Name = "lblCheckEvery";
            this.lblCheckEvery.Size = new System.Drawing.Size(115, 13);
            this.lblCheckEvery.TabIndex = 31;
            this.lblCheckEvery.Text = "Check every (minutes):";
            // 
            // txtImageAuth
            // 
            this.txtImageAuth.Enabled = false;
            this.txtImageAuth.Location = new System.Drawing.Point(155, 101);
            this.txtImageAuth.Name = "txtImageAuth";
            this.txtImageAuth.Size = new System.Drawing.Size(184, 20);
            this.txtImageAuth.TabIndex = 29;
            // 
            // txtPageAuth
            // 
            this.txtPageAuth.Enabled = false;
            this.txtPageAuth.Location = new System.Drawing.Point(155, 73);
            this.txtPageAuth.Name = "txtPageAuth";
            this.txtPageAuth.Size = new System.Drawing.Size(184, 20);
            this.txtPageAuth.TabIndex = 27;
            // 
            // chkImageAuth
            // 
            this.chkImageAuth.AutoSize = true;
            this.chkImageAuth.Location = new System.Drawing.Point(3, 103);
            this.chkImageAuth.Name = "chkImageAuth";
            this.chkImageAuth.Size = new System.Drawing.Size(136, 17);
            this.chkImageAuth.TabIndex = 28;
            this.chkImageAuth.Text = "Image auth (user:pass):";
            this.chkImageAuth.UseVisualStyleBackColor = true;
            this.chkImageAuth.CheckedChanged += new System.EventHandler(this.chkImageAuth_CheckedChanged);
            // 
            // chkPageAuth
            // 
            this.chkPageAuth.AutoSize = true;
            this.chkPageAuth.Location = new System.Drawing.Point(3, 75);
            this.chkPageAuth.Name = "chkPageAuth";
            this.chkPageAuth.Size = new System.Drawing.Size(132, 17);
            this.chkPageAuth.TabIndex = 26;
            this.chkPageAuth.Text = "Page auth (user:pass):";
            this.chkPageAuth.UseVisualStyleBackColor = true;
            this.chkPageAuth.CheckedChanged += new System.EventHandler(this.chkPageAuth_CheckedChanged);
            // 
            // chkOneTime
            // 
            this.chkOneTime.AutoSize = true;
            this.chkOneTime.Location = new System.Drawing.Point(3, 129);
            this.chkOneTime.Name = "chkOneTime";
            this.chkOneTime.Size = new System.Drawing.Size(117, 17);
            this.chkOneTime.TabIndex = 30;
            this.chkOneTime.Text = "One-time download";
            this.chkOneTime.UseVisualStyleBackColor = true;
            this.chkOneTime.CheckedChanged += new System.EventHandler(this.chkOneTime_CheckedChanged);
            // 
            // frmThreadEdit
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(374, 242);
            this.Controls.Add(this.pnlEdit);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmThreadEdit";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit";
            this.pnlEdit.ResumeLayout(false);
            this.pnlEdit.PerformLayout();
            this.pnlDescription.ResumeLayout(false);
            this.pnlDescription.PerformLayout();
            this.pnlCategory.ResumeLayout(false);
            this.pnlCategory.PerformLayout();
            this.pnlCheckEvery.ResumeLayout(false);
            this.pnlCheckEvery.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel pnlEdit;
        private System.Windows.Forms.CheckBox chkAutoFollow;
        private System.Windows.Forms.Panel pnlCheckEvery;
        private System.Windows.Forms.TextBox txtCheckEvery;
        private System.Windows.Forms.ComboBox cboCheckEvery;
        private System.Windows.Forms.Label lblCheckEvery;
        private System.Windows.Forms.TextBox txtImageAuth;
        private System.Windows.Forms.TextBox txtPageAuth;
        private System.Windows.Forms.CheckBox chkImageAuth;
        private System.Windows.Forms.CheckBox chkPageAuth;
        private System.Windows.Forms.CheckBox chkOneTime;
        private System.Windows.Forms.Panel pnlDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Panel pnlCategory;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cboCategory;
    }
}
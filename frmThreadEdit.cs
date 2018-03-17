using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace JDP {
    public sealed partial class frmThreadEdit : Form {
        private object _cboCheckEveryLastValue;
        private readonly IList<ThreadWatcher> _watchers;

        public frmThreadEdit(IList<ThreadWatcher> watchers, Dictionary<string, int> categories) {
            InitializeComponent();
            GUI.SetFontAndScaling(this);

            _watchers = watchers;

            if (watchers.Count > 1) {
                Text = $"Edit {watchers.Count} threads";
            }

            foreach (string key in categories.Keys) {
                cboCategory.Items.Add(key);
            }

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

            bool anyRunning = false;
            bool multiDescription = false;
            bool multiCategory = false;
            bool multiPageAuth = false;
            bool multiImageAuth = false;
            bool multiOneTime = false;
            bool multiAutoFollow = false;
            bool multiCheckEvery = false;

            for (int i = 0; i < watchers.Count; i++) {
                var watcher = watchers[i];
                if (i == 0) {
                    txtDescription.Text = watcher.Description;
                    cboCategory.Text = watcher.Category;

                    chkPageAuth.Checked = !String.IsNullOrEmpty(watcher.PageAuth);
                    txtPageAuth.Text = watcher.PageAuth;
                    chkPageAuth.Checked = !String.IsNullOrEmpty(watcher.ImageAuth);
                    txtImageAuth.Text = watcher.ImageAuth;

                    chkOneTime.Checked = watcher.OneTimeDownload;
                    chkAutoFollow.Checked = watcher.AutoFollow;

                    int checkIntervalMinutes = watcher.CheckIntervalSeconds / 60;
                    switch (checkIntervalMinutes) {
                        case 0:
                        case 2:
                        case 3:
                        case 5:
                        case 10:
                        case 60:
                            cboCheckEvery.SelectedValue = checkIntervalMinutes;
                            break;
                        case 1:
                            cboCheckEvery.SelectedValue = 0;
                            break;
                        default:
                            txtCheckEvery.Text = checkIntervalMinutes.ToString(CultureInfo.InvariantCulture);
                            break;
                    }
                }
                else {
                    if (!multiDescription && txtDescription.Text != watcher.Description) {
                        txtDescription.Text = String.Empty;
                        multiDescription = true;
                    }

                    if (!multiCategory && cboCategory.Text != watcher.Category) {
                        cboCategory.Text = String.Empty;
                        multiCategory = true;
                    }

                    if (!multiPageAuth && (chkPageAuth.Checked != !String.IsNullOrEmpty(watcher.PageAuth) || txtPageAuth.Text != watcher.PageAuth)) {
                        chkPageAuth.CheckState = CheckState.Indeterminate;
                        txtPageAuth.Text = String.Empty;
                        txtPageAuth.Enabled = false;
                        multiPageAuth = true;
                    }

                    if (!multiImageAuth && (chkImageAuth.Checked != !String.IsNullOrEmpty(watcher.ImageAuth) || txtImageAuth.Text != watcher.ImageAuth)) {
                        chkImageAuth.CheckState = CheckState.Indeterminate;
                        txtImageAuth.Text = String.Empty;
                        txtImageAuth.Enabled = false;
                        multiImageAuth = true;
                    }

                    if (!multiOneTime && chkOneTime.Checked != watcher.OneTimeDownload) {
                        chkOneTime.CheckState = CheckState.Indeterminate;
                        multiOneTime = true;
                    }

                    if (!multiAutoFollow && chkAutoFollow.Checked != watcher.AutoFollow) {
                        chkAutoFollow.CheckState = CheckState.Indeterminate;
                        multiAutoFollow = true;
                    }

                    if (!multiCheckEvery && (pnlCheckEvery.Enabled ? (cboCheckEvery.Enabled ? (int)cboCheckEvery.SelectedValue * 60 : Int32.Parse(txtCheckEvery.Text) * 60) : 0) != watcher.CheckIntervalSeconds) {
                        txtCheckEvery.Text = String.Empty;
                        cboCheckEvery.SelectedIndex = -1;
                        cboCheckEvery.Enabled = true;
                        _cboCheckEveryLastValue = 0;
                        multiCheckEvery = true;
                    }
                }

                anyRunning |= watcher.IsRunning;
            }

            if (anyRunning) {
                chkPageAuth.Enabled = false;
                txtPageAuth.Enabled = false;
                chkImageAuth.Enabled = false;
                txtImageAuth.Enabled = false;
                chkOneTime.Enabled = false;
                chkAutoFollow.Enabled = false;
            }

            Description = new EditField<string>(() => txtDescription.Text.Trim(), txtDescription);
            Category = new EditField<string>(() => cboCategory.Text.Trim(), cboCategory);
            PageAuth = new EditField<string>(() => chkPageAuth.Checked && txtPageAuth.Text.IndexOf(':') != -1 ? txtPageAuth.Text : String.Empty, chkPageAuth, txtPageAuth);
            ImageAuth = new EditField<string>(() => chkImageAuth.Checked && txtImageAuth.Text.IndexOf(':') != -1 ? txtImageAuth.Text : String.Empty, chkImageAuth, txtImageAuth);
            OneTimeDownload = new EditField<bool>(() => chkOneTime.Checked, chkOneTime);
            AutoFollow = new EditField<bool>(() => chkAutoFollow.Checked, chkAutoFollow);
            CheckIntervalSeconds = new EditField<int>(() => pnlCheckEvery.Enabled ? (cboCheckEvery.Enabled ? (int)cboCheckEvery.SelectedValue * 60 : Int32.Parse(txtCheckEvery.Text) * 60) : 0, cboCheckEvery, txtCheckEvery);
        }

        public EditField<string> Description { get; }

        public EditField<string> Category { get; }

        public EditField<string> PageAuth { get; }

        public EditField<string> ImageAuth { get; }

        public EditField<bool> OneTimeDownload { get; }

        public EditField<bool> AutoFollow { get; }

        public EditField<int> CheckIntervalSeconds { get; }

        public bool IsDirty => Description.IsDirty
                               || Category.IsDirty
                               || PageAuth.IsDirty
                               || ImageAuth.IsDirty
                               || OneTimeDownload.IsDirty
                               || AutoFollow.IsDirty
                               || CheckIntervalSeconds.IsDirty;

        private void btnOK_Click(object sender, EventArgs e) {
            if (_watchers.Count > 1 && IsDirty) {
                var result = MessageBox.Show(this, $"Changes made will be applied to {_watchers.Count} selected threads.", "Confirm changes", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel) {
                    return;
                }
            }

            if (Description.IsDirty && Description.Value.Length == 0) {
                MessageBox.Show(this, "Description cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
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
            _cboCheckEveryLastValue = cboCheckEvery.SelectedValue;
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
    }
}
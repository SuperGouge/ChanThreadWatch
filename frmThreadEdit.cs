using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace JDP {
    public partial class frmThreadEdit : Form {
        private object _cboCheckEveryLastValue;

        public frmThreadEdit(ThreadWatcher watcher, Dictionary<string, int> categories) {
            InitializeComponent();
            GUI.SetFontAndScaling(this);

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

            txtDescription.Text = watcher.Description;
            cboCategory.Text = watcher.Category;
            chkPageAuth.Checked = !String.IsNullOrEmpty(watcher.PageAuth);
            txtPageAuth.Text = watcher.PageAuth;
            chkPageAuth.Checked = !String.IsNullOrEmpty(watcher.ImageAuth);
            txtImageAuth.Text = watcher.ImageAuth;
            chkOneTime.Checked = watcher.OneTimeDownload;
            chkAutoFollow.Checked = watcher.AutoFollow;

            switch (watcher.CheckIntervalSeconds) {
                case 0: cboCheckEvery.SelectedIndex = 0; break;
                case 2: cboCheckEvery.SelectedIndex = 1; break;
                case 3: cboCheckEvery.SelectedIndex = 2; break;
                case 5: cboCheckEvery.SelectedIndex = 3; break;
                case 10: cboCheckEvery.SelectedIndex = 4; break;
                case 60: cboCheckEvery.SelectedIndex = 5; break;
                default:
                    txtCheckEvery.Text = watcher.CheckIntervalSeconds.ToString(CultureInfo.InvariantCulture);
                    cboCheckEvery.SelectedIndex = -1;
                    cboCheckEvery.Enabled = false;
                    break;
            }

            if (watcher.IsRunning) {
                chkPageAuth.Enabled = false;
                txtPageAuth.Enabled = false;
                chkImageAuth.Enabled = false;
                txtImageAuth.Enabled = false;
                chkOneTime.Enabled = false;
                chkAutoFollow.Enabled = false;
                pnlCheckEvery.Enabled = false;
            }
            cboCheckEvery.SelectedIndexChanged += cboCheckEvery_SelectedIndexChanged;
            txtCheckEvery.TextChanged += txtCheckEvery_TextChanged;
        }

        public string Description {
            get { return txtDescription.Text.Trim(); }
        }

        public string Category {
            get { return cboCategory.Text; }
        }

        public string PageAuth {
            get { return (chkPageAuth.Checked && (txtPageAuth.Text.IndexOf(':') != -1)) ? txtPageAuth.Text : String.Empty; }
        }

        public string ImageAuth {
            get { return (chkImageAuth.Checked && (txtImageAuth.Text.IndexOf(':') != -1)) ? txtImageAuth.Text : String.Empty; }
        }

        public bool OneTimeDownload {
            get { return chkOneTime.Checked; }
        }

        public bool AutoFollow {
            get { return chkAutoFollow.Checked; }
        }

        public int CheckIntervalSeconds {
            get { return cboCheckEvery.Enabled ? (int)cboCheckEvery.SelectedValue * 60 : Int32.Parse(txtCheckEvery.Text) * 60; }
        }

        private void btnOK_Click(object sender, EventArgs e) {
            if (txtDescription.Text.Trim().Length == 0) {
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
            if (_cboCheckEveryLastValue == null && (int)cboCheckEvery.SelectedValue == 0) {
                _cboCheckEveryLastValue = 3;
            }
            else {
                _cboCheckEveryLastValue = cboCheckEvery.SelectedValue;
            }
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
using System;
using System.Windows.Forms;

namespace JDP {
    public partial class frmThreadEdit : Form {
        public frmThreadEdit(ThreadEditType threadEditType) {
            InitializeComponent();
            GUI.SetFontAndScaling(this);
            Text = threadEditType.ToString();
            lblEdit.Text = threadEditType + ":";
        }

        public string Value {
            get { return txtEdit.Text.Trim(); }
            set {
                txtEdit.Text = value;
                txtEdit.Select(txtEdit.Text.Length, 0);
            }
        }

        private void btnOK_Click(object sender, EventArgs e) {
            if (txtEdit.Text.Trim().Length == 0) {
                MessageBox.Show(this, Text + " cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
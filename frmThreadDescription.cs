using System;
using System.Windows.Forms;

namespace JDP {
	public partial class frmThreadDescription : Form {
		public frmThreadDescription() {
			InitializeComponent();
			GUI.SetFontAndScaling(this);
		}

		public string Description {
			get {
				return txtDescription.Text.Trim();
			}
			set {
				txtDescription.Text = value;
				txtDescription.Select(txtDescription.Text.Length, 0);
			}
		}

		private void btnOK_Click(object sender, EventArgs e) {
			if (txtDescription.Text.Trim().Length == 0) {
				MessageBox.Show(this, "Description cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			DialogResult = DialogResult.OK;
		}
	}
}

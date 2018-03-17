using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace JDP {
    public class WatcherExtraData {
        public ListViewItem ListViewItem { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime? LastImageOn { get; set; }
        public bool HasDownloadedPage { get; set; }
        public bool PreviousDownloadWasPage { get; set; }
        public string AddedFrom { get; set; }
    }

    public struct DownloadProgressInfo {
        public long DownloadID { get; set; }
        public string URL { get; set; }
        public int TryNumber { get; set; }
        public long StartTicks { get; set; }
        public long? EndTicks { get; set; }
        public long? TotalSize { get; set; }
        public long DownloadedSize { get; set; }
    }

    public class DownloadedSizeSnapshot {
        public long Ticks { get; set; }
        public long DownloadedSize { get; set; }

        public DownloadedSizeSnapshot(long ticks, long downloadedSize) {
            Ticks = ticks;
            DownloadedSize = downloadedSize;
        }
    }

    public class ListItemInt32 {
        public int Value { get; private set; }
        public string Text { get; private set; }

        public ListItemInt32(int value, string text) {
            Value = value;
            Text = text;
        }
    }

    public class ListViewItemSorter : IComparer {
        public int Column { get; set; }
        public bool Ascending { get; set; }

        public ListViewItemSorter(int column) {
            Column = column;
            Ascending = true;
        }

        public int Compare(object x, object y) {
            int cmp = String.Compare(((ListViewItem)x).SubItems[Column].Text, ((ListViewItem)y).SubItems[Column].Text);
            return Ascending ? cmp : -cmp;
        }
    }

    public class EditField<TValue> {
        private readonly Func<TValue> _valueGetter;

        public EditField(Func<TValue> valueGetter, params Control[] controls) {
            _valueGetter = valueGetter ?? throw new ArgumentNullException(nameof(valueGetter));

            if (controls == null) {
                throw new ArgumentNullException(nameof(controls));
            }

            foreach (var control in controls) {
                switch (control) {
                    case CheckBox chk:
                        chk.CheckedChanged += (sender, args) => IsDirty = true;
                        break;
                    case ComboBox cbo:
                        cbo.SelectedValueChanged += (sender, args) => IsDirty = true;
                        if (cbo.DropDownStyle != ComboBoxStyle.DropDownList) {
                            cbo.TextChanged += (sender, args) => IsDirty = true;
                        }
                        break;
                    case null:
                        break;
                    default:
                        control.TextChanged += (sender, args) => IsDirty = true;
                        break;
                }
            }
        }

        public bool IsDirty { get; private set; }

        public TValue Value => _valueGetter.Invoke();
    }

    public static class GUI {
        public static void CenterChildForm(Form parent, Form child) {
            if (!parent.Visible) {
                child.StartPosition = FormStartPosition.CenterScreen;
                return;
            }

            int centerX = ((parent.Left * 2) + parent.Width) / 2;
            int centerY = ((parent.Top * 2) + parent.Height) / 2;
            int formX = ((parent.Left * 2) + parent.Width - child.Width) / 2;
            int formY = ((parent.Top * 2) + parent.Height - child.Height) / 2;

            Rectangle formRect = new Rectangle(formX, formY, child.Width, child.Height);
            Rectangle maxRect = Screen.GetWorkingArea(new Point(centerX, centerY));

            if (formRect.Right > maxRect.Right) {
                formRect.X -= formRect.Right - maxRect.Right;
            }
            if (formRect.Bottom > maxRect.Bottom) {
                formRect.Y -= formRect.Bottom - maxRect.Bottom;
            }
            if (formRect.X < maxRect.X) {
                formRect.X = maxRect.X;
            }
            if (formRect.Y < maxRect.Y) {
                formRect.Y = maxRect.Y;
            }

            child.Location = formRect.Location;
        }

        public static void EnableDoubleBuffering<T>(T control) where T : Control {
            typeof (T).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                control,
                new object[] { true });
        }

        public static void SetFontAndScaling(Form form) {
            form.SuspendLayout();
            form.Font = new Font("Tahoma", 8.25F);
            if (form.Font.Name != "Tahoma") form.Font = new Font("Arial", 8.25F);
            form.AutoScaleMode = AutoScaleMode.Font;
            form.AutoScaleDimensions = new SizeF(6F, 13F);
            form.ResumeLayout(false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JDP {
	public partial class frmDownloads : Form {
		private frmChanThreadWatch _parentForm;
		private Dictionary<long, ListViewItem> _items = new Dictionary<long, ListViewItem>();
		private Dictionary<long, List<DownloadedSizeSnapshot>> _snapshotLists = new Dictionary<long, List<DownloadedSizeSnapshot>>();

		public frmDownloads(frmChanThreadWatch parentForm) {
			InitializeComponent();
			GUI.SetFontAndScaling(this);
			GUI.EnableDoubleBuffering(lvDownloads);
			_parentForm = parentForm;
		}

		private void tmrUpdateList_Tick(object sender, EventArgs e) {
			HashSet<long> oldDownloadIDs = new HashSet<long>(_items.Keys);
			List<DownloadProgressInfo> downloadProgresses;
			lock (_parentForm.DownloadProgresses) {
				downloadProgresses = new List<DownloadProgressInfo>(_parentForm.DownloadProgresses.Values);
			}
			long ticksNow = TickCount.Now;
			long totalDownloadedSize = 0;
			long minTotalDownloadedStartTicks = Int64.MaxValue;
			downloadProgresses.Sort((a, b) => a.StartTicks.CompareTo(b.StartTicks));
			foreach (DownloadProgressInfo info in downloadProgresses) {
				List<DownloadedSizeSnapshot> snapshotList;
				if (!_snapshotLists.TryGetValue(info.DownloadID, out snapshotList)) {
					snapshotList = new List<DownloadedSizeSnapshot>();
					snapshotList.Add(new DownloadedSizeSnapshot(info.StartTicks, 0));
					_snapshotLists[info.DownloadID] = snapshotList;
				}
				while (snapshotList.Count != 0 && ticksNow - snapshotList[0].Ticks > 5000) {
					snapshotList.RemoveAt(0);
				}
				snapshotList.Add(new DownloadedSizeSnapshot(ticksNow, info.DownloadedSize));
				int iLast = snapshotList.Count - 1;
				long size = snapshotList[iLast].DownloadedSize - snapshotList[0].DownloadedSize;
				long ticks = snapshotList[iLast].Ticks - snapshotList[0].Ticks;
				long? bytesPerSec = null;
				if (size > 0 && ticks > 0) {
					bytesPerSec = Convert.ToInt64(size / (ticks / 1000.0));
				}
				int iFirstForTotalWindow = iLast;
				for (int i = 0; i < snapshotList.Count; i++) {
					if (ticksNow - snapshotList[i].Ticks <= 2000 &&
						snapshotList[i].DownloadedSize < snapshotList[iLast].DownloadedSize)
					{
						iFirstForTotalWindow = i;
						break;
					}
				}
				totalDownloadedSize += snapshotList[iLast].DownloadedSize - snapshotList[iFirstForTotalWindow].DownloadedSize;
				minTotalDownloadedStartTicks = Math.Min(minTotalDownloadedStartTicks, snapshotList[iFirstForTotalWindow].Ticks);
				if (info.EndTicks == null) {
					UpdateDownloadProgress(info, bytesPerSec);
					oldDownloadIDs.Remove(info.DownloadID);
				}
			}
			foreach (long downloadID in oldDownloadIDs) {
				RemoveDownloadProgress(downloadID);
			}
			long totalDownloadedTicks = ticksNow - minTotalDownloadedStartTicks;
			if (totalDownloadedSize > 0 && totalDownloadedTicks > 0) {
				Text = "Downloads - " + GetKilobytesString(Convert.ToInt64(
					totalDownloadedSize / (totalDownloadedTicks / 1000.0)), "KB/s");
			}
			else {
				Text = "Downloads";
			}
		}

		public void UpdateDownloadProgress(DownloadProgressInfo info, long? bytesPerSec) {
			ListViewItem item;
			if (!_items.TryGetValue(info.DownloadID, out item)) {
				item = new ListViewItem(String.Empty);
				for (int i = 1; i < lvDownloads.Columns.Count; i++) {
					item.SubItems.Add(String.Empty);
				}
				SetSubItemText(item, ColumnIndex.URL, info.URL);
				SetSubItemText(item, ColumnIndex.Size, GetKilobytesString(info.TotalSize, "KB"));
				SetSubItemText(item, ColumnIndex.Try, info.TryNumber.ToString());
				lvDownloads.Items.Add(item);
				_items[info.DownloadID] = item;
			}
			if (info.TotalSize != null) {
				SetSubItemText(item, ColumnIndex.Percent, (info.DownloadedSize * 100 / info.TotalSize.Value).ToString() + "%");
			}
			else {
				SetSubItemText(item, ColumnIndex.Size, GetKilobytesString(info.DownloadedSize, "KB"));
			}
			SetSubItemText(item, ColumnIndex.Speed, GetKilobytesString(bytesPerSec, "KB/s"));
		}

		private void RemoveDownloadProgress(long downloadID) {
			ListViewItem item;
			if (!_items.TryGetValue(downloadID, out item)) return;
			lvDownloads.Items.Remove(item);
			_items.Remove(downloadID);
		}

		private string GetKilobytesString(long? byteSize, string units) {
			if (byteSize == null) return String.Empty;
			return (byteSize.Value / 1024).ToString("#,##0") + " " + units;
		}

		private void SetSubItemText(ListViewItem item, ColumnIndex columnIndex, string text) {
			var subItem = item.SubItems[(int)columnIndex];
			if (subItem.Text != text) {
				subItem.Text = text;
			}
		}

		private enum ColumnIndex {
			URL = 0,
			Size = 1,
			Percent = 2,
			Speed = 3,
			Try = 4
		}
	}
}

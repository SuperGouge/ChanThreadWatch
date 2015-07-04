using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace JDP {
    public static class Settings {
        private static Dictionary<string, string> _settings;

        public static string ApplicationName {
            get { return "Chan Thread Watch"; }
        }

        public static bool? UseCustomUserAgent {
            get { return GetBool("UseCustomUserAgent"); }
            set { SetBool("UseCustomUserAgent", value); }
        }

        public static string CustomUserAgent {
            get { return Get("CustomUserAgent"); }
            set { Set("CustomUserAgent", value); }
        }

        public static bool? UsePageAuth {
            get { return GetBool("UsePageAuth"); }
            set { SetBool("UsePageAuth", value); }
        }

        public static string PageAuth {
            get { return Get("PageAuth"); }
            set { Set("PageAuth", value); }
        }

        public static bool? UseImageAuth {
            get { return GetBool("UseImageAuth"); }
            set { SetBool("UseImageAuth", value); }
        }

        public static string ImageAuth {
            get { return Get("ImageAuth"); }
            set { Set("ImageAuth", value); }
        }

        public static bool? OneTimeDownload {
            get { return GetBool("OneTimeDownload"); }
            set { SetBool("OneTimeDownload", value); }
        }

        public static bool? AutoFollow {
            get { return GetBool("AutoFollow"); }
            set { SetBool("AutoFollow", value); }
        }

        public static int? CheckEvery {
            get { return GetInt("CheckEvery"); }
            set { SetInt("CheckEvery", value); }
        }

        public static bool? DownloadFolderIsRelative {
            get { return GetBool("DownloadFolderIsRelative"); }
            set { SetBool("DownloadFolderIsRelative", value); }
        }

        public static string DownloadFolder {
            get { return Get("DownloadFolder"); }
            set { Set("DownloadFolder", value); }
        }

        public static bool? RenameDownloadFolderWithDescription {
            get { return GetBool("RenameDownloadFolderWithDescription"); }
            set { SetBool("RenameDownloadFolderWithDescription", value); }
        }

        public static bool? RenameDownloadFolderWithCategory {
            get { return GetBool("RenameDownloadFolderWithCategory"); }
            set { SetBool("RenameDownloadFolderWithCategory", value); }
        }

        public static bool? RenameDownloadFolderWithParentThreadDescription {
            get { return GetBool("RenameDownloadFolderWithParentThreadDescription"); }
            set { SetBool("RenameDownloadFolderWithParentThreadDescription", value); }
        }

        public static string ParentThreadDescriptionFormat {
            get { return Get("ParentThreadDescriptionFormat"); }
            set { Set("ParentThreadDescriptionFormat", value); }
        }

        public static bool? ChildThreadsAreNewFormat {
            get { return GetBool("ChildThreadsAreNewFormat"); }
            set { SetBool("ChildThreadsAreNewFormat", value); }
        }

        public static bool? SortImagesByPoster {
            get { return GetBool("SortImagesByPoster"); }
            set { SetBool("SortImagesByPoster", value); }
        }

        public static bool? RecursiveAutoFollow {
            get { return GetBool("RecursiveAutoFollow"); }
            set { SetBool("RecursiveAutoFollow", value); }
        }

        public static bool? InterBoardAutoFollow {
            get { return GetBool("InterBoardAutoFollow"); }
            set { SetBool("InterBoardAutoFollow", value); }
        }

        public static bool? SaveThumbnails {
            get { return GetBool("SaveThumbnails"); }
            set { SetBool("SaveThumbnails", value); }
        }

        public static bool? UseOriginalFileNames {
            get { return GetBool("UseOriginalFileNames"); }
            set { SetBool("UseOriginalFileNames", value); }
        }

        public static bool? VerifyImageHashes {
            get { return GetBool("VerifyImageHashes"); }
            set { SetBool("VerifyImageHashes", value); }
        }

        public static bool? UseSlug {
            get { return GetBool("UseSlug"); }
            set { SetBool("UseSlug", value); }
        }

        public static SlugType SlugType {
            get {
                string value = Get("SlugType") ?? String.Empty;
                if (String.IsNullOrEmpty(value)) return SlugType.Last;
                SlugType valueSlug;
                try {
                    valueSlug = (SlugType)Enum.Parse(typeof (SlugType), value);
                }
                catch (ArgumentException) {
                    valueSlug = SlugType.Last;
                }
                return valueSlug;
            }
            set { Set("SlugType", value.ToString()); }
        }

        public static bool? CheckForUpdates {
            get { return GetBool("CheckForUpdates"); }
            set { SetBool("CheckForUpdates", value); }
        }

        public static DateTime? LastUpdateCheck {
            get { return GetDate("LastUpdateCheck"); }
            set { SetDate("LastUpdateCheck", value); }
        }

        public static string LatestUpdateVersion {
            get { return Get("LatestUpdateVersion"); }
            set { Set("LatestUpdateVersion", value); }
        }

        public static bool? BlacklistWildcards {
            get { return GetBool("BlacklistWildcards"); }
            set { SetBool("BlacklistWildcards", value); }
        }

        public static bool? MinimizeToTray {
            get { return GetBool("MinimizeToTray"); }
            set {  SetBool("MinimizeToTray", value); }
        }

        public static bool? BackupThreadList {
            get { return GetBool("BackupThreadList"); }
            set { SetBool("BackupThreadList", value); }
        }

        public static int? BackupEvery {
            get { return GetInt("BackupEvery"); }
            set { SetInt("BackupEvery", value); }
        }

        public static bool? BackupCheckSize {
            get { return GetBool("BackupCheckSize"); }
            set { SetBool("BackupCheckSize", value); }
        }

        public static long? MaximumBytesPerSecond {
            get { return GetLong("MaximumBytesPerSecond"); }
            set { SetLong("MaximumBytesPerSecond", value); }
        }
        
        public static string WindowTitle {
            get { return Get("WindowTitle"); }
            set { Set("WindowTitle", value); }
        }

        public static bool? UseExeDirectoryForSettings { get; set; }

        public static string ExeDirectory {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        public static string AppDataDirectory {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName); }
        }

        public static string SettingsFileName {
            get { return "settings.txt"; }
        }

        public static string ThreadsFileName {
            get { return "threads.txt"; }
        }

        public static string LogFileName {
            get { return "log.txt"; }
        }

        public static string BlacklistFileName {
            get { return "blacklist.txt"; }
        }

        public static string DebugFolderName {
            get { return "Debug"; }
        }

        public static ThreadDoubleClickAction? OnThreadDoubleClick {
            get {
                int x = GetInt("OnThreadDoubleClick") ?? -1;
                return Enum.IsDefined(typeof (ThreadDoubleClickAction), x) ?
                    (ThreadDoubleClickAction?)x : null;
            }
            set { SetInt("OnThreadDoubleClick", value.HasValue ? (int?)value.Value : null); }
        }

        public static string GetSettingsDirectory() {
            if (UseExeDirectoryForSettings == null) {
                #if DEBUG
                    UseExeDirectoryForSettings = File.Exists(Path.Combine(Path.Combine(ExeDirectory, DebugFolderName), SettingsFileName));
                #else
                    UseExeDirectoryForSettings = File.Exists(Path.Combine(ExeDirectory, SettingsFileName));
                #endif
            }
            return GetSettingsDirectory(UseExeDirectoryForSettings.Value);
        }

        public static string GetSettingsDirectory(bool useExeDirForSettings) {
            if (useExeDirForSettings) {
                #if DEBUG
                    return Path.Combine(ExeDirectory, DebugFolderName);
                #else
                    return ExeDirectory;
                #endif
            }
            else {
                #if DEBUG
                    string dir = Path.Combine(AppDataDirectory, DebugFolderName);
                #else
                    string dir = AppDataDirectory;
                #endif
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                return dir;
            }
        }

        public static string AbsoluteDownloadDirectory {
            get {
                #if DEBUG
                    string dir = Path.Combine(DownloadFolder, DebugFolderName);
                #else
                    string dir = DownloadFolder;
                #endif
                if (!String.IsNullOrEmpty(dir) && (DownloadFolderIsRelative == true)) {
                    dir = General.GetAbsoluteDirectoryPath(dir, ExeDirectory);
                }
                return dir;
            }
        }

        public static Size? ClientSize {
            get {
                int[] size = GetIntArray("ClientSize");
                if (size.Length != 2 || size[0] < 1 || size[1] < 1) return null;
                return new Size(size[0], size[1]);
            }
            set { Set("ClientSize", value.HasValue ? value.Value.Width + "," + value.Value.Height : null); }
        }

        public static int[] ColumnWidths {
            get { return GetIntArray("ColumnWidths"); }
            set { SetIntArray("ColumnWidths", value); }
        }

        public static int[] DefaultColumnWidths {
            get { return new[] { 110, 150, 115, 115, 110, 75 }; }
        }

        public static int[] ColumnIndices {
            get { return GetIntArray("ColumnIndices"); }
            set { SetIntArray("ColumnIndices", value); }
        }
        
        public static int? SortColumn {
            get { return GetInt("SortColumn"); }
            set { SetInt("SortColumn", value); }
        }

        public static bool? SortAscending {
            get { return GetBool("SortAscending"); }
            set { SetBool("SortAscending", value); }
        }

        private static string Get(string name) {
            lock (_settings) {
                string value;
                return _settings.TryGetValue(name, out value) ? value : null;
            }
        }

        private static bool? GetBool(string name) {
            string value = Get(name);
            if (value == null) return null;
            return value != "0";
        }

        private static int? GetInt(string name) {
            string value = Get(name);
            if (value == null) return null;
            int x;
            return Int32.TryParse(value, out x) ? x : (int?)null;
        }

        private static long? GetLong(string name) {
            string value = Get(name);
            if (value == null) return null;
            long x;
            return Int64.TryParse(value, out x) ? x : (long?)null;
        }

        private static DateTime? GetDate(string name) {
            string value = Get(name);
            if (value == null) return null;
            DateTime x;
            return DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out x) ? x : (DateTime?)null;
        }

        private static int[] GetIntArray(string name) {
            string value = Get(name);
            if (value == null) return new int[0];
            string[] array = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int[] values = new int[array.Length];
            for (int i = 0; i < array.Length; i++) {
                Int32.TryParse(array[i], out values[i]);
            }
            return values;
        }

        private static void Set(string name, string value) {
            lock (_settings) {
                if (value == null) {
                    _settings.Remove(name);
                }
                else {
                    _settings[name] = value;
                }
            }
        }

        private static void SetBool(string name, bool? value) {
            Set(name, value.HasValue ? (value.Value ? "1" : "0") : null);
        }

        private static void SetInt(string name, int? value) {
            Set(name, value.HasValue ? value.Value.ToString() : null);
        }

        private static void SetLong(string name, long? value) {
            Set(name, value.HasValue ? value.Value.ToString() : null);
        }

        private static void SetDate(string name, DateTime? value) {
            Set(name, value.HasValue ? value.Value.ToString("yyyyMMdd") : null);
        }

        private static void SetIntArray(string name, int[] value) {
            Set(name, value.Length > 0 ? String.Join(",", Array.ConvertAll(value, Convert.ToString)) : null);
        }

        public static void Load() {
            string path = Path.Combine(GetSettingsDirectory(), SettingsFileName);

            _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(path)) {
                return;
            }

            try {
                using (StreamReader sr = File.OpenText(path)) {
                    string line;

                    while ((line = sr.ReadLine()) != null) {
                        int pos = line.IndexOf('=');

                        if (pos != -1) {
                            string name = line.Substring(0, pos);
                            string val = line.Substring(pos + 1);

                            if (!_settings.ContainsKey(name)) {
                                _settings.Add(name, val);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }

        public static void Save() {
            string path = Path.Combine(GetSettingsDirectory(), SettingsFileName);
            try {
                using (StreamWriter sw = File.CreateText(path)) {
                    lock (_settings) {
                        foreach (KeyValuePair<string, string> kvp in _settings) {
                            sw.WriteLine(kvp.Key + "=" + kvp.Value);
                        }
                    }
                }
            }
            catch (Exception ex) {
                Logger.Log(ex.ToString());
            }
        }
    }
}
using ValveKeyValue;

namespace Gamevault.Models
{
    public class SteamShortcut
    {
        [KVProperty("appid")]
        public string appID { get; set; }
        [KVProperty("AppName")]
        public string appName { get; set; }
        [KVProperty("Exe")]
        public string exe { get; set; }
        [KVProperty("StartDir")]
        public string startDir { get; set; }
        [KVProperty("LaunchOptions")]
        public string launchOptions { get; set; }
        [KVProperty("AllowDesktopConfig")]
        public int allowDesktopConfig { get; set; }
        [KVProperty("AllowOverlay")]
        public int allowOverlay { get; set; }
        [KVProperty("Icon")]
        public string icon { get; set; }
        [KVProperty("Index")]
        public int index { get; set; }
        [KVProperty("IsHidden")]
        public int isHidden { get; set; }
        [KVProperty("OpenVR")]
        public int openVR { get; set; }
        [KVProperty("ShortcutPath")]
        public string shortcutPath { get; set; }
        [KVProperty("Tags")]
        public int tags { get; set; }
        [KVProperty("Devkit")]
        public int devkit { get; set; }
        [KVProperty("DevkitGameID")]
        public string devkitGameID { get; set; }
        [KVProperty("LastPlayTime")]
        public int lastPlayTime { get; set; }
    }
}
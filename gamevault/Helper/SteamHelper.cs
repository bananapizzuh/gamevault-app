using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using Force.Crc32;
using gamevault.ViewModels;
using gamevault.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using ValveKeyValue;

namespace gamevault.Helper
{
    public static class SteamHelper
    {
        public static string? GetSteamFolder()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Valve\\Steam");
            if (key == null)
                key = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Valve\\Steam");
            if (key == null)
                return null;

            string? path = key.GetValue("InstallPath").ToString();

            if (path.IsNullOrEmpty())
                return null;

            return path;
        }

        public static String[]? GetUsers(string steamInstallPath)
        {

            string[]? userPaths = Directory.GetDirectories(steamInstallPath + "\\userdata");
            List<string> userIDs = new List<string>();
            foreach (string path in userPaths)
            {
                userIDs.Append(Path.GetRelativePath(Path.GetDirectoryName(path), path));
            }
            return userIDs.ToArray();
        }

        public static KVObject? ReadShortcuts(string steamInstallPath, string userID)
        {
            string shortcutFile = $"{steamInstallPath}\\userdata\\{userID}\\config\\shortcuts.vdf";
            if (!File.Exists(shortcutFile))
            {
                Debug.WriteLine("no exist");
                return null;
            }

            FileStream stream;
            KVObject data;
            var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);
            try
            {
                stream = File.OpenRead(shortcutFile);
                data = kv.Deserialize(stream);
                stream.Close();
                return data;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        private static void WriteShortcuts(KVObject data, string steamInstallPath, string userID)
        {
            string shortcutFile = $"{steamInstallPath}\\userdata\\{userID}\\config\\shortcuts.vdf";
            if (!File.Exists(shortcutFile) || !BackupShortcuts(steamInstallPath, userID))
            {
                Debug.WriteLine("No");
                return;
            }

            try
            {
                using var stream = File.OpenWrite(shortcutFile);
                var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);
                kv.Serialize(stream, data);
                Debug.WriteLine("done");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private static bool BackupShortcuts(string steamInstallPath, string userID)
        {
            string shortcutFile = $"{steamInstallPath}\\userdata\\{userID}\\config\\shortcuts.vdf";
            if (!File.Exists(shortcutFile))
                return false;

            string backupFolder = $"{SettingsViewModel.Instance.RootPath}\\GameVault\\Backups\\";
            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);

            string backupPath = Path.Combine(backupFolder, $"{DateTime.Now.ToString("yyyyMMddHHmmss")}.vdf");

            try
            {
                File.Copy(shortcutFile, backupPath);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
            return true;
        }

        private static UInt64 GenerateSteamGridAppId(string appName, string appTarget)
        {
            byte[] nameTargetBytes = Encoding.UTF8.GetBytes(appTarget + appName + "");
            UInt64 crc = Crc32Algorithm.Compute(nameTargetBytes);
            UInt64 gameId = crc | 0x80000000;

            return gameId;
        }

        public static void AddGameToSteam(Game game, string userID)
        {
            string? steamInstallPath = GetSteamFolder();
            if (steamInstallPath.IsNullOrEmpty())
                return;
            Debug.WriteLine(steamInstallPath);

            string? executablePath = Process.GetCurrentProcess().MainModule?.FileName;

            if (string.IsNullOrEmpty(executablePath))
                return;
            Debug.WriteLine(executablePath);

            string? executableDir = Path.GetDirectoryName(executablePath);

            if (string.IsNullOrEmpty(executableDir))
                return;
            Debug.WriteLine(executableDir);

            int appID = (int)GenerateSteamGridAppId(game.Title, executablePath);

            string launchOptions = $"start --gameid {game.ID}";
            int lastPlayed = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            KVObject? shortcuts = ReadShortcuts(steamInstallPath, userID);
            if (shortcuts == null)
                return;

            List<KVObject> shortcutList = shortcuts.ToList();
            int length = shortcutList.ToArray().Length;


            KVObject[] array = {
                new KVObject("appid", appID),
                new KVObject("AppName", game.Title),
                new KVObject("Exe", executablePath),
                new KVObject("StartDir", executableDir),
                new KVObject("LaunchOptions", launchOptions),
                new KVObject("AllowDesktopConfig", 1),
                new KVObject("AllowOverlay", 1),
                new KVObject("Icon", ""),
                new KVObject("Index", length),
                new KVObject("IsHidden", 0),
                new KVObject("OpenVR", 0),
                new KVObject("ShortcutPath", ""),
                new KVObject("Tags", 0),
                new KVObject("Devkit", 0),
                new KVObject("DevkitGameID", ""),
                new KVObject("LastPlayTime", lastPlayed),
            };

            KVObject newShortcut = new KVObject(length.ToString(), array);
            KVObject newShortcuts = new KVObject("shortcuts", shortcutList.Append(newShortcut));
            WriteShortcuts(newShortcuts, steamInstallPath, userID);


            // TODO make a shortcut class, for easier creation/(de)serialization
            // TODO clean up code/better variable names
            // TODO get icon from actual game exe
            // TODO account for duplicates
            // TODO tags
        }
    }
}

using MelonLoader;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ScheduleOne.UI.Settings;

#if IL2CPP
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.UI.MainMenu;
#elif MONO
using ScheduleOne.Persistence;
using ScheduleOne.UI.MainMenu;
#else
    // Other configs could go here
#endif

[assembly: MelonInfo(typeof(AutomaticBackups.Core), "AutomaticBackups", "1.0.1-beta", "coderTrevor", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace AutomaticBackups
{
    public class Core : MelonMod
    {
        public static MelonLogger.Instance logger;
        public static MelonPreferences_Entry<bool> enableAutoDelete;
        private MelonPreferences_Category autoDeleteCategory;

        public override void OnInitializeMelon()
        {
            logger = LoggerInstance;

            // Create our preferences
            autoDeleteCategory = MelonPreferences.CreateCategory("AutoDeleteCategory");
            enableAutoDelete = autoDeleteCategory.CreateEntry<bool>("EnableAutoDelete", false);

            logger.Msg("Automatic Backups Initialized.");
        }

        public static void Log(string msg)
        {
            logger.Msg(msg);
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            // We only care about the Menu scene
            if (sceneName != "Menu")
                return;

            // Add our MenuMod MonoBehavior to the MainMenu object
            GameObject mainMenu = GameObject.Find("MainMenu");
            mainMenu.AddComponent<MenuMod>();            
        }

        [HarmonyPatch(typeof(SaveManager), "Save", new Type[] { typeof(string) })]
        static class SavePatch
        {
            // After saving, uses Schedule I's built-in zip exporter to backup the save that was just made to a timestamped zip file.
            public static void Postfix(string saveFolderPath)
            {
                // Ensure there's no trailing path separator
                saveFolderPath = saveFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                // Get the base name and parent path of the save directory
                var saveFolderName = Path.GetFileName(saveFolderPath);
                var parent = Directory.GetParent(saveFolderPath)?.FullName
                             ?? throw new InvalidOperationException($"No parent directory for '{saveFolderPath}'.");

                // Ensure the backup destination exists - <parent>\Backups\<saveFolderName>\
                var backupDir = Path.Combine(parent, "Backups", saveFolderName);
                Directory.CreateDirectory(backupDir);

                // Export save folder to .zip - <parent>\Backups\<saveFolderName>\<timestamp>.zip
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupDest = Path.Combine(backupDir, timestamp + ".zip");
                SaveExportButton.ZipSaveFolder(saveFolderPath, backupDest);

                Core.logger.Msg($"Exported\n{saveFolderPath} to\n{backupDest}");
            }
        }
    }
}
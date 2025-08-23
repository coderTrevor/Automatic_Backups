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

        // Auto-delete settings
        public static MelonPreferences_Entry<bool> enableAutoDelete;
        public static MelonPreferences_Entry<int> autoDeleteRetentionCount;
        public const int DEFAULT_RETENTION_COUNT = 125;
        private MelonPreferences_Category autoDeleteCategory;

        // These are only used as constants in the code, but we can still let advanced users edit their values in MelonPreferences.cfg
        public const int RETENTION_SLIDER_MULTIPLIER = 5;
        public const int RETENTION_SLIDER_MIN_FILES = 25;
        public const int RETENTION_SLIDER_MAX_FILES = 250;
        public static MelonPreferences_Entry<int> retentionSliderMultiplier;
        public static MelonPreferences_Entry<int> retentionSliderMinFiles;
        public static MelonPreferences_Entry<int> retentionSliderMaxFiles;

        public override void OnInitializeMelon()
        {
            logger = LoggerInstance;

            // Create our preferences
            autoDeleteCategory = MelonPreferences.CreateCategory("AutoDeleteCategory");
            enableAutoDelete = autoDeleteCategory.CreateEntry<bool>("EnableAutoDelete", false);
            retentionSliderMultiplier = autoDeleteCategory.CreateEntry<int>("RetentionSliderMultiplier", RETENTION_SLIDER_MULTIPLIER);
            retentionSliderMinFiles = autoDeleteCategory.CreateEntry<int>("RetentionSliderMinFiles", RETENTION_SLIDER_MIN_FILES);
            retentionSliderMaxFiles = autoDeleteCategory.CreateEntry<int>("RetentionSliderMaxFiles", RETENTION_SLIDER_MAX_FILES);
            autoDeleteRetentionCount = autoDeleteCategory.CreateEntry<int>("AutoDeleteRetentionCount", DEFAULT_RETENTION_COUNT);

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
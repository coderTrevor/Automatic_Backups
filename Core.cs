using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Enumeration;
using System.Collections;

#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.UI.MainMenu;
#elif MONO
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.UI.MainMenu;
#else
    // Other configs could go here
#endif

[assembly: MelonInfo(typeof(AutomaticBackups.Core), "AutomaticBackups", "1.1.0-beta", "coderTrevor", null)]
[assembly: MelonGame("TVGS", "Schedule I")]
#if MONO
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
#else
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
#endif

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

        // Auto-save settings
        public static MelonPreferences_Entry<bool> enableAutoSave;
        public static MelonPreferences_Entry<int> autoSaveTime;
        public const int DEFAULT_SAVE_TIME = 10;
        private MelonPreferences_Category autoSaveCategory;

        // These are only used as constants in the code, but we can still let advanced users edit their values in MelonPreferences.cfg
        public const int RETENTION_SLIDER_MULTIPLIER = 5;
        public const int RETENTION_SLIDER_MIN_FILES = 25;
        public const int RETENTION_SLIDER_MAX_FILES = 250;
        public static MelonPreferences_Entry<int> retentionSliderMultiplier;
        public static MelonPreferences_Entry<int> retentionSliderMinFiles;
        public static MelonPreferences_Entry<int> retentionSliderMaxFiles;
        public const int AUTOSAVE_TIME_MIN_MINUTES = 1;
        public const int AUTOSAVE_TIME_MAX_MINUTES = 60;
        public static MelonPreferences_Entry<int> autoSaveSliderMinTime;
        public static MelonPreferences_Entry<int> autoSaveSliderMaxTime;

        protected static DirectoryInfo backupDirInfo;
        protected static Queue<FileInfo> orderedBackups; // List of backup files, ordered by age. The oldest file will be the next item dequeued.

        public override void OnInitializeMelon()
        {
            logger = LoggerInstance;

            // Auto-delete preferences
            autoDeleteCategory = MelonPreferences.CreateCategory("AutoDeleteCategory");
            enableAutoDelete = autoDeleteCategory.CreateEntry<bool>("EnableAutoDelete", false);
            retentionSliderMultiplier = autoDeleteCategory.CreateEntry<int>("RetentionSliderMultiplier", RETENTION_SLIDER_MULTIPLIER);
            retentionSliderMinFiles = autoDeleteCategory.CreateEntry<int>("RetentionSliderMinFiles", RETENTION_SLIDER_MIN_FILES);
            retentionSliderMaxFiles = autoDeleteCategory.CreateEntry<int>("RetentionSliderMaxFiles", RETENTION_SLIDER_MAX_FILES);
            autoDeleteRetentionCount = autoDeleteCategory.CreateEntry<int>("AutoDeleteRetentionCount", DEFAULT_RETENTION_COUNT);

            // Auto-save preferences
            autoSaveCategory = MelonPreferences.CreateCategory("AutoSaveCategory");
            enableAutoSave = autoSaveCategory.CreateEntry<bool>("EnableAutoSave", true);
            autoSaveSliderMinTime = autoSaveCategory.CreateEntry<int>("timeSliderMinMinutes", AUTOSAVE_TIME_MIN_MINUTES);
            autoSaveSliderMaxTime = autoSaveCategory.CreateEntry<int>("timeSliderMaxMinutes", AUTOSAVE_TIME_MAX_MINUTES);
            autoSaveTime = autoSaveCategory.CreateEntry<int>("autoSaveTime", DEFAULT_SAVE_TIME);

            logger.Msg("Automatic Backups Initialized.");
        }

        public static void Log(string msg)
        {
            logger.Msg(msg);
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (sceneName == "Menu")
            {
                // Add our MenuMod MonoBehavior to the MainMenu object
                GameObject mainMenu = GameObject.Find("MainMenu");
                if (mainMenu.GetComponent<MenuMod>())
                {
                    Log($"Backups menu has already been created.");
                    return;
                }
                mainMenu.AddComponent<MenuMod>();
            }
            else if (sceneName == "Main")
            {
                // Ensure there's not too many files in the backup folder
                ScanBackupsFolder();
            }
        }

        // Scans backups folder and populates the orderedBackups Queue.
        protected void ScanBackupsFolder()
        {
            string savePath = Singleton<LoadManager>.Instance.LoadedGameFolderPath;
            string backupDir = GetBackupPath(savePath);
            Log($"Backups will be saved to\n{backupDir}");

            // Get all .zip backups for the current save, sorted by earliest of creation/modification time, and add them to our queue.
            backupDirInfo = new DirectoryInfo(backupDir);
            var sortedFiles = backupDirInfo.GetFiles("*.zip").OrderBy(f => Math.Min(f.CreationTimeUtc.Ticks, f.LastWriteTimeUtc.Ticks));
            orderedBackups = new Queue<FileInfo>(sortedFiles);

            if (orderedBackups.Count > 0)
            {
                long totalBytes = orderedBackups.Sum(f => f.Length);
                double totalMB = totalBytes / (1024.0 * 1024.0);
                Log($"{orderedBackups.Count} backup files found, using {totalMB:F1} MB of disk space");
            }

            DeleteExpiredBackups();
        }

        // If the user enabled deleting old backups, deletes the oldest backups until there are no more than what's allowed by autoDeleteRetentionCount.
        protected static void DeleteExpiredBackups()
        {
            if (!enableAutoDelete.Value || orderedBackups.Count <= Core.autoDeleteRetentionCount.Value)
                return;

            Log("Number of backup files exceeds user preferences");

            while (orderedBackups.Count > Core.autoDeleteRetentionCount.Value)
            {
                FileInfo toDelete = orderedBackups.Dequeue();
                Log($"{toDelete.Name} from {toDelete.CreationTime.ToShortDateString()} will be deleted");
                toDelete.Delete();
            }
        }

        // Adds a new backup file to the ordered list and ensures there aren't too many files in the directory
        public static void AddNewBackupFile(string path)
        {
            orderedBackups.Enqueue(new FileInfo(path));

            DeleteExpiredBackups();
        }

        [HarmonyPatch(typeof(SaveManager), "Save", new Type[] { typeof(string) })]
        static class SavePatch
        {
            // After saving, uses Schedule I's built-in zip exporter to backup the save that was just made to a timestamped zip file.
            public static void Postfix(string saveFolderPath)
            {
                string backupDir = GetBackupPath(saveFolderPath);

                // Export save folder to .zip - <parent>\Backups\<saveFolderName>\<timestamp>.zip
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupDest = Path.Combine(backupDir, timestamp + ".zip");
                SaveExportButton.ZipSaveFolder(saveFolderPath, backupDest);

                Core.logger.Msg($"Exported\n{saveFolderPath} to\n{backupDest}");

                // Add the newly-created file to our ordered list of backups and ensure we don't have too many files
                Core.AddNewBackupFile(backupDest);
            }
        }

        // Given the save folder path, convert to our backup path and ensure it exists
        static string GetBackupPath(string saveFolderPath)
        {
            // Ensure there's no trailing path separator
            saveFolderPath = saveFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Get the base name and parent path of the save directory
            string saveFolderName = Path.GetFileName(saveFolderPath);
            string parent = Directory.GetParent(saveFolderPath)?.FullName
                         ?? throw new InvalidOperationException($"No parent directory for '{saveFolderPath}'.");

            // Ensure the backup destination exists - <parent>\Backups\<saveFolderName>\
            string backupDir = Path.Combine(parent, "Backups", saveFolderName);
            Directory.CreateDirectory(backupDir);

            return backupDir;
        }
    }
}
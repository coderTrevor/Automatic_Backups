using MelonLoader;
using ScheduleOne.Persistence;
using ScheduleOne.UI.MainMenu;
using HarmonyLib;

[assembly: MelonInfo(typeof(AutomaticBackups.Core), "AutomaticBackups", "1.0.0", "Trevor", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace AutomaticBackups
{
    public class Core : MelonMod
    {
        public static MelonLogger.Instance logger;
        public override void OnInitializeMelon()
        {
            logger = LoggerInstance;
            logger.Msg("Automatic Backups Initialized.");
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

                Core.logger.Msg($"Exported {saveFolderPath} to {backupDest}");
            }
        }
    }
}
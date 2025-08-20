using MelonLoader;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if IL2CPP
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.UI.MainMenu;
#elif MONO
using ScheduleOne.Persistence;
using ScheduleOne.UI.MainMenu;
#else
    // Other configs could go here
#endif

[assembly: MelonInfo(typeof(AutomaticBackups.Core), "AutomaticBackups", "1.0.0-beta", "coderTrevor", null)]
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

    // Allows us to add a category to the Settings display with our mod's settings
    // We need a MonoBehavior so we can clone existing objects in the scene hierarchy (I don't know how to do this outside of a MonoBehavior)
    public class MenuMod : MonoBehaviour
    {
        void Awake()
        {
            Init(); // Adding the Awake code to another function will keep VS from making all the code gray
        }

        void Init()
        {            
            // We should be attached to the MainMenu GameObject
            GameObject mainMenu = gameObject;
            
            // Settings object will be a child of MainMenu
            Transform settings = mainMenu.transform.Find("Settings");                             

            // Create the Backups button (tab on the settings display)
            Button backupsButton = CreateBackupsButton(settings);

            // Create the panel to display our settings
            GameObject backupsPanel = CreateBackupsPanel(settings);

            // Wire up the Button and panel on the settings display
            AddSettingsCategory(settings, backupsButton, backupsPanel);
        }

        // Create a button (tab) in the Settings display for our Backups category
        Button CreateBackupsButton(Transform settings)
        {
            // Find Buttons and Controls in the scene hierarchy
            Transform buttons = settings.Find("Buttons");
            Transform controlsButton = buttons.Find("Controls");

            // Clone the Controls button and update the clone for our Backup category button.
            Transform modSettingsButton = Instantiate(controlsButton, buttons);
            modSettingsButton.name = "Backups";
            TextMeshProUGUI nameUGUI = modSettingsButton.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            nameUGUI.text = "Backups";

            return modSettingsButton.GetComponent<Button>();
        }

        // Create a panel to display our mod's settings
        GameObject CreateBackupsPanel(Transform settings)
        {
            // Find and clone the Controls panel
            Transform panelsParent = settings.Find("Content");
            Transform controlsPanel = panelsParent.Find("Controls");
            Transform modSettingsPanel = Instantiate(controlsPanel, panelsParent);

            // Update the panel clone
            modSettingsPanel.name = "Backups";

            return modSettingsPanel.gameObject;
        }

        // Add a tab to the settings display with our mod's settings
        void AddSettingsCategory(Transform settings, Button modSettingsButton, GameObject modSettingsPanel)
        {
            SettingsScreen settingsScreen = settings.GetComponent<SettingsScreen>();
            int categoryCount = settingsScreen.Categories.Length;

            // Embiggen the Categories array
            Array.Resize<SettingsScreen.SettingsCategory>(ref settingsScreen.Categories, categoryCount + 1);
            categoryCount++;

            // Add a new settings category with our button & panel
            SettingsScreen.SettingsCategory backupCategory = new SettingsScreen.SettingsCategory();
            backupCategory.Button = modSettingsButton;
            backupCategory.Panel = modSettingsPanel;
            int lastCategoryIndex = categoryCount - 1;
            settingsScreen.Categories[lastCategoryIndex] = backupCategory;

            // Add a listener to our button to show the category we've just added
            modSettingsButton.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                GameObject.FindAnyObjectByType<SettingsScreen>().ShowCategory(lastCategoryIndex);
            });
        }
    }
}
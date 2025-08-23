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

            // Setup our controls
            Transform retentionSlider = SetupRetentionSlider(modSettingsPanel);
            Transform deleteOldestToggle = SetupDeleteOldestToggle(modSettingsPanel);

            // Remove all the cloned controls we don't care about
            deleteOldestToggle.SetParent(null);
            retentionSlider.SetParent(null);
            DestroyAllChildren(modSettingsPanel);
            deleteOldestToggle.SetParent(modSettingsPanel);
            retentionSlider.SetParent(modSettingsPanel);

            return modSettingsPanel.gameObject;
        }

        // Finds the "invertY" toggle we cloned from the Controls panel and repurposes it to toggle the option to delete oldest saves
        Transform SetupDeleteOldestToggle(Transform settingsPanel)
        {
            Transform invertY = settingsPanel.Find("InvertY");
            invertY.name = "DeleteOldSaves";
            HorizontalLayoutGroup hlg = invertY.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 25;

            SetLabelText(invertY, "Delete oldest saves");

            Transform toggle = invertY.Find("Toggle");
            toggle.SetAsFirstSibling();
            Toggle toggleComponent = toggle.GetComponent<Toggle>();
            toggleComponent.isOn = Core.enableAutoDelete.Value;
            
            // Replace the invertY component with our DeleteOldestToggle component
            Destroy(toggle.GetComponent<InvertYToggle>());
            toggle.gameObject.AddComponent<DeleteOldestToggle>();

            return invertY;
        }

        // Finds the mouse sensitivity slider we cloned from the Controls panel and repurposes it to control the number of files to back up
        Transform SetupRetentionSlider(Transform settingsPanel)
        {
            Transform sensitivity = settingsPanel.Find("Sensitivity");
            sensitivity.name = "RetainedCount";
            
            // Ensure the GameObject is active so DeleteOldToggle can find it
            sensitivity.gameObject.SetActive(true);

            SetLabelText(sensitivity, "Max Files Per Save Slot");

            Transform slider = sensitivity.Find("Slider");

            // Remove the SensitivitySlider component
            Destroy(slider.GetComponent<SensitivitySlider>());

            slider.gameObject.AddComponent<SettingsSlider>();
            Slider sliderComponent = slider.GetComponent<Slider>();
            sliderComponent.minValue = 15;
            sliderComponent.maxValue = 1000;

            return sensitivity;
        }

        // Given a Transform with a child named Label with a TMPro_Text component, update the label
        void SetLabelText(Transform parent, string text)
        {
            Transform label = parent.Find("Label");
            label.GetComponent<TMP_Text>().text = text;
        }

        // Destroys all children of a given Transform
        void DestroyAllChildren(Transform parent)
        {
            // Iterate backwards to avoid issues while modifying the hierarchy
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
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

    public class DeleteOldestToggle : SettingsToggle
    {
        protected GameObject retainedCountSlider = null;

        public void Start()
        {
            Core.Log("start called");
            // Find the retained count slider and show/hide it based on our enableAutoDelete setting
            retainedCountSlider = GameObject.Find("RetainedCount");
            retainedCountSlider.SetActive(Core.enableAutoDelete.Value);
            Core.Log("start done");
        }

        protected override void OnValueChanged(bool value)
        {
            retainedCountSlider.SetActive(value);
            Core.Log($"{value}");
            Core.enableAutoDelete.Value = value;
        }
    }
}
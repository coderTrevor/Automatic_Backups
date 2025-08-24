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

namespace AutomaticBackups
{
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

            // Remove all the other cloned controls we don't care about
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

            // Replace the InvertYToggle component with our DeleteOldestToggle component
            Destroy(toggle.GetComponent<InvertYToggle>());
            toggle.gameObject.AddComponent<DeleteOldestToggle>();

            return invertY;
        }

        // Finds the mouse sensitivity slider we cloned from the Controls panel and repurposes it to control the number of files to back up
        Transform SetupRetentionSlider(Transform settingsPanel)
        {
            Transform sliderParent = settingsPanel.Find("Sensitivity");
            sliderParent.name = "RetainedCount";

            // Ensure the GameObject is active so DeleteOldToggle can find it
            sliderParent.gameObject.SetActive(true);

            SetLabelText(sliderParent, "Max Files Per Save Slot");

            Transform slider = sliderParent.Find("Slider");

            // Remove the listener that the SensitivitySlider will have added
            slider.GetComponent<Slider>().onValueChanged.RemoveAllListeners();

            // Replace the SensitivitySlider with our RetentionAmountSlider
            DestroyImmediate(slider.GetComponent<SensitivitySlider>());
            slider.gameObject.AddComponent<RetentionAmountSlider>();

            return sliderParent;
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
}
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;
#if MONO
using ScheduleOne.Persistence;
using ScheduleOne.UI.MainMenu;
using ScheduleOne.UI.Settings;
using ScheduleOne.DevUtilities;
using TMPro;
#else
using Il2CppTMPro;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.UI.MainMenu;
using Il2CppScheduleOne.UI.Settings;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif

// Adds a "Load" button to each game slot in the Continue menu
// Also creates a sub-menu to allow for selecting which backup to restore
namespace AutomaticBackups
{
    // Allows us to modify the Continue menu
    // We need a MonoBehavior so we can clone existing objects in the scene hierarchy (I don't know how to do this outside of a MonoBehavior)
    [RegisterTypeInIl2Cpp]
    public class ContinueMenuMod : MonoBehaviour
    {
        protected Transform continueMenu;               // Menu with save slots (Schedule I's built-in Menu)
        protected Transform loadMenu = null;            // Menu for our list of backups
        protected RestoreBackupScreen restoreBackupScreen = null;

        void Awake()
        {
            Init(); // Calling the Awake code from another function will keep VS from making all the code gray
        }

        void Init()
        {
            // We should be attached to the MainMenu GameObject
            GameObject mainMenu = gameObject;

            // Continue object will be a child of MainMenu
            continueMenu = mainMenu.transform.Find("Continue");
            Transform continueContainer = continueMenu.Find("Container");
            //DestroyImmediate(continueContainer.GetComponent<VerticalLayoutGroup>());

            // Add our "Load" button to each slot            
            for (int i = 0; i < continueContainer.childCount; i++) // Enumerate children in an Il2Cpp-friendly manner
                AddButtonToSlot(continueContainer.GetChild(i), i);

            // Instantiate scroll view and move all children to it
            /*GameObject scrollView = Instantiate(Core.scrollViewPrefab);
            Transform scrollViewContainer = scrollView.transform.Find("Viewport").Find("Content");
            Core.Log($"scrollViewContainer: {scrollViewContainer.ToString()}");

            // Iterate backwards over children so we can move them without messing up the ordering
            for (int i = continueContainer.childCount - 1; i >= 0; i--)
                continueContainer.GetChild(i).SetParent(scrollViewContainer, false);

            scrollView.transform.SetParent(continue_, false);*/

            // Create the menu that will be displayed if the user clicks one of the "Load" buttons
            CreateLoadMenu();
        }

        void AddButtonToSlot(Transform slot, int slotNumber)
        {
            Transform container = slot.Find("Container");
            Transform info = container.Find("Info");
            Transform exportButton = info.Find("Export");

            // Clone export button and modify it to become our Load button
            Transform loadButton = Instantiate(exportButton, info);
            loadButton.name = "Load";

            // Update position
            loadButton.localPosition = new Vector3(-215.0f, 20.0f, 0.0f);

            // Update text
            Transform text = loadButton.GetChild(0);
            TextMeshProUGUI ugui = text.GetComponent<TextMeshProUGUI>();
            ugui.text = "Load";

            // Remove Export button component
            DestroyImmediate(loadButton.GetComponent<SaveExportButton>());

            Button button = loadButton.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener( new UnityAction( () =>
            {
                Core.Log("Load button clicked");
                LoadClicked(slotNumber);
            }));
        }

        public void LoadClicked(int slot)
        {
            Core.Log($"Slot {slot} was clicked.");

            if (!loadMenu)
                CreateLoadMenu();
            
            PopulateLoadMenu(slot);
            restoreBackupScreen.Open(true);
        }

        // Clone the "Continue" menu for our "Load/Restore" menu
        void CreateLoadMenu()
        {
            // Create the Restore menu based on our panel prefab
            loadMenu = Instantiate(Core.backupRestorePanelPrefab).transform;

            // Clone the background from the Continue menu
            Transform background = continueMenu.Find("Background");
            Transform ourBG = Instantiate(background, loadMenu);
            ourBG.name = "Background";

            // Clone the title from the Continue menu
            Transform title = continueMenu.Find("Title");
            Transform ourTitle = Instantiate(title, loadMenu);
            ourTitle.name = "Title";
            TextMeshProUGUI tmpUGUI = ourTitle.GetComponent<TextMeshProUGUI>();
            tmpUGUI.text = "Restore Backup";

            // Set the MainMenu as the parent and make sure the panel is hidden
            loadMenu.SetParent(transform, false);
            loadMenu.name = "RestoreBackup";
            loadMenu.gameObject.SetActive(false);

            // Setup our RestoreBackupScreen component
            restoreBackupScreen = loadMenu.gameObject.AddComponent<RestoreBackupScreen>();            
            restoreBackupScreen.Initialize(continueMenu.gameObject.GetComponent<ContinueScreen>());

            // Make sure the ScrollView is drawn last
            Transform scrollView = loadMenu.Find("Scroll View");
            scrollView.SetAsLastSibling();
        }

        // Add buttons to the load menu
        void PopulateLoadMenu(int slot)
        {
            // Get the scroll container
            Transform container = loadMenu.Find("Scroll View").Find("Viewport").Find("Content");
            Core.Log($"Container: {container.name}");

            // Remove any buttons that might have been added
            SettingsMenuMod.DestroyAllChildren(container);

            // Get the backup path for the given slot and ensure it exists
            string backupsPath = GetBackupsPath(slot);
            Core.Log(backupsPath);
            if (!Directory.Exists(backupsPath))
            {
                Core.Log($"No backups folder found at {backupsPath}");
                return;
            }

            // Add a button for each zip file in the backups folder
            DirectoryInfo dirInfo = new DirectoryInfo(backupsPath);
            var files = dirInfo.GetFiles("*.zip").OrderBy(f => Math.Min(f.CreationTimeUtc.Ticks, f.LastWriteTimeUtc.Ticks));
            foreach (FileInfo file in files)
            {
                Core.Log($"Found {file.FullName}");
                Transform buttonTransform = Instantiate(Core.backupRestoreButtonPrefab.transform, container);

                // Update the text on the button
                TextMeshProUGUI text = buttonTransform.GetChild(0).gameObject.AddComponent<TextMeshProUGUI>();
                text.fontSize = 16;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.Left;
                text.text = "  " + file.Name;

                // Add click action
                Button button = buttonTransform.GetComponent<Button>();
                button.onClick.AddListener(new UnityAction(() =>
                {
                    Core.Log("File button clicked");
                    BackupSelected(file);
                }));
            }

        }

        string GetBackupsPath(int slot)
        {
            string backupsPath = Singleton<SaveManager>.Instance.BackupFolderPath;

            return Path.Combine(backupsPath, $"SaveGame_{slot + 1}");
        }

        void BackupSelected(FileInfo file)
        {
            Core.Log($"Player selected {file.Name}");
        }
    }
}
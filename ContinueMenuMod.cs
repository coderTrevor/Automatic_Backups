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
using TMPro;
#else
using Il2CppTMPro;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.UI.MainMenu;
using Il2CppScheduleOne.UI.Settings;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif

namespace AutomaticBackups
{
    // Allows us to modify the Continue menu
    // We need a MonoBehavior so we can clone existing objects in the scene hierarchy (I don't know how to do this outside of a MonoBehavior)
    [RegisterTypeInIl2Cpp]
    public class ContinueMenuMod : MonoBehaviour
    {
        void Awake()
        {
            Init(); // Calling the Awake code from another function will keep VS from making all the code gray
        }

        void Init()
        {
            // We should be attached to the MainMenu GameObject
            GameObject mainMenu = gameObject;

            // Continue object will be a child of MainMenu
            Transform continue_ = mainMenu.transform.Find("Continue");
            Transform container = continue_.Find("Container");            

            // Add our button to each slot            
            for (int i = 0; i < container.childCount; i++) // Enumerate children in an Il2Cpp-friendly manner
                AddButtonToSlot(container.GetChild(i));
            
        }

        void AddButtonToSlot(Transform slot)
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
            button.onClick = null;
        }
    }
}
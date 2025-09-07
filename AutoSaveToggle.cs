using UnityEngine;
using UnityEngine.UI;
using MelonLoader;

#if IL2CPP
using Il2CppScheduleOne.UI.MainMenu;
using Il2CppScheduleOne.UI.Settings;
#elif MONO
using ScheduleOne.UI.MainMenu;
using ScheduleOne.UI.Settings;
#else
    // Other configs could go here
#endif

namespace AutomaticBackups
{
    [RegisterTypeInIl2Cpp]
    public class AutoSaveToggle : SettingsToggle
    {
        protected GameObject saveTimeSlider = null;

        public void Start()
        {
            bool enableAutoSave = Core.enableAutoSave.Value;

            // Set the Toggle according to enableAutoSave
            Toggle toggleComponent = gameObject.GetComponent<Toggle>();
            toggleComponent.isOn = enableAutoSave;

            // Find the retained count slider and show/hide it based on our enableAutoSave setting
            saveTimeSlider = GameObject.Find("AutoSave Time");
            saveTimeSlider.SetActive(enableAutoSave);
        }
        
#if IL2CPP
        public override void OnValueChanged(bool value)
#else
        protected override void OnValueChanged(bool value)
#endif
        {
            saveTimeSlider.SetActive(value);
            Core.enableAutoSave.Value = value;
        }
    }
}
using UnityEngine;
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
    public class DeleteOldestToggle : SettingsToggle
    {
        protected GameObject retainedCountSlider = null;

        public void Start()
        {
            // Find the retained count slider and show/hide it based on our enableAutoDelete setting
            retainedCountSlider = GameObject.Find("RetainedCount");
            retainedCountSlider.SetActive(Core.enableAutoDelete.Value);
        }
        
#if IL2CPP
        public override void OnValueChanged(bool value)
#else
        protected override void OnValueChanged(bool value)
#endif
        {
            retainedCountSlider.SetActive(value);
            Core.enableAutoDelete.Value = value;
        }
    }
}
using UnityEngine;
using ScheduleOne.UI.Settings;

#if IL2CPP
using Il2CppScheduleOne.UI.MainMenu;
#elif MONO
using ScheduleOne.UI.MainMenu;
#else
    // Other configs could go here
#endif

namespace AutomaticBackups
{
    public class DeleteOldestToggle : SettingsToggle
    {
        protected GameObject retainedCountSlider = null;

        public void Start()
        {
            // Find the retained count slider and show/hide it based on our enableAutoDelete setting
            retainedCountSlider = GameObject.Find("RetainedCount");
            retainedCountSlider.SetActive(Core.enableAutoDelete.Value);
        }

        protected override void OnValueChanged(bool value)
        {
            retainedCountSlider.SetActive(value);
            Core.enableAutoDelete.Value = value;
        }
    }
}
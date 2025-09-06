using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

#if IL2CPP
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.UI.MainMenu;
using Il2CppScheduleOne.UI.Settings;
#elif MONO
using ScheduleOne.Persistence;
using ScheduleOne.UI.MainMenu;
using ScheduleOne.UI.Settings;
#else
    // Other configs could go here
#endif

namespace AutomaticBackups
{
    [RegisterTypeInIl2Cpp]
    public class AutoSaveTimeSlider : SettingsSlider
    {
        protected virtual void OnEnable()
        {
            base.slider.minValue = Core.autoSaveSliderMinTime.Value;
            base.slider.maxValue = Core.autoSaveSliderMaxTime.Value;
            base.slider.value = Core.autoSaveTime.Value;
        }
 
#if IL2CPP
        public
#else
        protected 
#endif
            override void OnValueChanged(float value)
        {
            this.timeOnValueChange = Time.time;
            if (this.DisplayValue)
            {
                this.valueLabel.text = this.GetDisplayValue(value);
                this.valueLabel.enabled = true;
            }
            Core.autoSaveTime.Value = (int)value;
        }
    }
}
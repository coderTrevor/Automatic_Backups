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
    public class RetentionAmountSlider : SettingsSlider
    {
        protected int multiplier;
        protected virtual void OnEnable()
        {
            multiplier = Core.retentionSliderMultiplier.Value;
            int minValue = DivideAndFloor(Core.retentionSliderMinFiles.Value, multiplier);
            int maxValue = DivideAndFloor(Core.retentionSliderMaxFiles.Value, multiplier);
            int value = DivideAndFloor(Core.autoDeleteRetentionCount.Value, multiplier);

            base.slider.minValue = minValue;
            base.slider.maxValue = maxValue;
            base.slider.value = value;
        }

        int DivideAndFloor(int numerator, int denominator)
        {
            float quotient = (float)numerator / denominator;
            return (int)Mathf.Floor(quotient);
        }

#if IL2CPP
        public
#else
        protected 
#endif
        override string GetDisplayValue(float value)
        {
            return ((int)Mathf.Round(value * multiplier)).ToString();
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
            Core.autoDeleteRetentionCount.Value = (int)value * multiplier;
        }
    }
}
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

        protected override string GetDisplayValue(float value)
        {
            return ((int)Mathf.Round(value * multiplier)).ToString();
        }

        protected override void OnValueChanged(float value)
        {
            base.OnValueChanged(value);
            Core.autoDeleteRetentionCount.Value = (int)value * multiplier;
        }
    }
}
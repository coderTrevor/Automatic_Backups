using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;
using ScheduleOne.Tools;
using ScheduleOne.DevUtilities;


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
    [RegisterTypeInIl2Cpp]
    public class RestoreBackupScreen : MainMenuScreen
    {
        protected override void Awake()
        {
            OpenOnStart = false;
            base.Awake();
        }

        public void Initialize(MainMenuScreen previous)
        {
            this.PreviousScreen = previous;
        }

        public override void Close(bool openPrevious)
        {
            base.Close(openPrevious);
            gameObject.SetActive(false);
        }
    }
}
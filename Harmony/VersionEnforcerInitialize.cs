using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace VersionEnforcer.Harmony
{
    [UsedImplicitly]
    public class VersionEnforcerInitialize : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            // Reduce extra logging
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
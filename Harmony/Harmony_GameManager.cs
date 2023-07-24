using HarmonyLib;
using JetBrains.Annotations;
using VersionEnforcer.Scripts;

namespace VersionEnforcer.Harmony
{
    [UsedImplicitly]
    public class Harmony_GameManager
    {
        [HarmonyPatch(typeof(GameManager), "ShowMessagePlayerDenied")]
        public class GameManager_ShowMessagePlayerDeniedPatch
        {
            [UsedImplicitly]
            public static bool Prefix(GameManager __instance, GameUtils.KickPlayerData _kickData)
            {
                string text;
                string kickReason;
                if (_kickData.reason == (GameUtils.EKickReason)500)
                {
                    kickReason = "Mod version does not match server";
                    text = string.Format(Localization.Get("auth_ModVersionMismatch"), Globals.CustomVersion,
                        _kickData.customReason);
                }
                else
                {
                    kickReason = _kickData.ToString();
                    text = _kickData.LocalizedMessage();
                }

                var guiWindowManager = Traverse.Create(__instance).Field("windowManager")?.GetValue() as GUIWindowManager;
                if (guiWindowManager == null)
                {
                    Log.Warning($"{Globals.LOG_TAG} Unable to find windowManager");
                    return true;
                }

                var messageBoxWindowGroupController =
                    ((XUiWindowGroup)(guiWindowManager).GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller as
                    XUiC_MessageBoxWindowGroup;
                if (messageBoxWindowGroupController == null)
                {
                    Log.Warning($"{Globals.LOG_TAG} Unable to get window with ID {XUiC_MessageBoxWindowGroup.ID}");
                    return true;
                }
                
                Log.Out($"[NET] Kicked from server: {kickReason}");
                messageBoxWindowGroupController.ShowMessage(Localization.Get("auth_messageTitle"), text);

                return false;
            }
        }
    }
}
using HarmonyLib;
using UnityEngine;

namespace QuickChat
{
    [HarmonyPatch]
    public class QuickChatPatches
    {
        [HarmonyPatch(typeof(ChatManager), "Awake")]
        [HarmonyPostfix]
        public static void ChatManagerAwakePostfix(ChatManager __instance)
        {
            if (__instance.gameObject.GetComponent<QuickChatWheel>() == null)
            {
                __instance.gameObject.AddComponent<QuickChatWheel>();
                QuickChatPlugin.Log.LogInfo("[Harmony] Successfully attached QuickChatWheel to ChatManager!");
            }
        }

        [HarmonyPatch(typeof(ChatManager), "Update")]
        [HarmonyPrefix]
        public static bool ChatManagerUpdatePrefix()
        {
            if (MenuManager.instance != null && MenuManager.instance.textInputActive)
            {
                return false; 
            }
            return true;
        }
    }
}

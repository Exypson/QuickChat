using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;

namespace QuickChat
{
    [BepInPlugin("com.exypson.quickchat", "QuickChat", "1.0.0")]
    public class QuickChatPlugin : BaseUnityPlugin 
    {
        internal static ManualLogSource Log { get; private set; }

        public static ConfigEntry<string> ConfigTop;
        public static ConfigEntry<string> ConfigTopRight;
        public static ConfigEntry<string> ConfigBottomRight;
        public static ConfigEntry<string> ConfigBottom;
        public static ConfigEntry<string> ConfigBottomLeft;
        public static ConfigEntry<string> ConfigTopLeft;

        private void Awake() 
        {
            Log = base.Logger; 

            ConfigTop = Config.Bind("Chat Wheel Presets", "1. Top", "Yes", "Text for the Top slice.");
            ConfigTopRight = Config.Bind("Chat Wheel Presets", "2. Top Right", "No", "Text for the Top-Right slice.");
            ConfigBottomRight = Config.Bind("Chat Wheel Presets", "3. Bottom Right", "Help", "Text for the Bottom-Right slice.");
            ConfigBottom = Config.Bind("Chat Wheel Presets", "4. Bottom", "Hide", "Text for the Bottom slice.");
            ConfigBottomLeft = Config.Bind("Chat Wheel Presets", "5. Bottom Left", "Follow Me", "Text for the Bottom-Left slice.");
            ConfigTopLeft = Config.Bind("Chat Wheel Presets", "6. Top Left", "Thanks", "Text for the Top-Left slice.");

            Harmony.CreateAndPatchAll(typeof(QuickChatPatches));

            Log.LogInfo("QuickChat mod loaded! Harmony patches applied successfully.");
        }
    }
}

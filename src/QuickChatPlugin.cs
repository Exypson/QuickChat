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
        public static ConfigEntry<string> ConfigRight;
        public static ConfigEntry<string> ConfigBottomRight;
        public static ConfigEntry<string> ConfigBottom;
        public static ConfigEntry<string> ConfigBottomLeft;
        public static ConfigEntry<string> ConfigLeft;
        public static ConfigEntry<string> ConfigTopLeft;

        private void Awake() 
        {
            Log = base.Logger; 

            ConfigTop = Config.Bind("Chat Wheel Presets", "1. Top", "Yes", "Text for the Top slice.");
            ConfigTopRight = Config.Bind("Chat Wheel Presets", "2. Top Right", "Help", "Text for the Top-Right slice.");
            ConfigRight = Config.Bind("Chat Wheel Presets", "3. Right", "Run", "Text for the Right slice.");
            ConfigBottomRight = Config.Bind("Chat Wheel Presets", "4. Bottom Right", "Thanks", "Text for the Bottom-Right slice.");
            ConfigBottom = Config.Bind("Chat Wheel Presets", "5. Bottom", "No", "Text for the Bottom slice.");
            ConfigBottomLeft = Config.Bind("Chat Wheel Presets", "6. Bottom Left", "Hide", "Text for the Bottom-Left slice.");
            ConfigLeft = Config.Bind("Chat Wheel Presets", "7. Left", "Stop", "Text for the Left slice.");
            ConfigTopLeft = Config.Bind("Chat Wheel Presets", "8. Top Left", "Follow Me", "Text for the Top-Left slice.");

            Harmony.CreateAndPatchAll(typeof(QuickChatPatches));

            Log.LogInfo("QuickChat mod loaded! Harmony patches applied successfully.");
        }
    }
}

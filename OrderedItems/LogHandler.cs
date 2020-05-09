using RoR2;
using UnityEngine;


namespace OrderedItems
{
    // Handles the mod's logging.
    public static class LogHandler
    {
        private static BepInEx.Logging.ManualLogSource Logger;
        private static string gamePrefix;
        private static Color32 Color = new Color32(111, 10, 170, 255);


        // Initialises the LogHandler
        public static void Init(BepInEx.Logging.ManualLogSource loggerSource)
        {
            LogHandler.Logger = loggerSource;
            LogHandler.GeneratePrefix();
        }


        // Logs a message.
        public static void Log(string message, bool chat = false, BepInEx.Logging.LogLevel priority = BepInEx.Logging.LogLevel.Info)
        {
            if (chat)
            {
                Chat.AddMessage(LogHandler.gamePrefix + message);
            }
            LogHandler.Logger.Log(priority, message);
        }


        // Generates the mod's prefix according to its name and version number.
        public static void GeneratePrefix()
        {
            string prefix = "[" + OrderedItems.MODNAME + " v" + OrderedItems.MODVER + "]: ";
            LogHandler.gamePrefix = prefix.Colored(LogHandler.Color);
        }


        // Colors a string for the game chat.
        public static string Colored(this string message, Color32 color)
        {
            string construction = "<color=#{0:X2}{1:X2}{2:X2}>{3}</color>";
            string result = string.Format(construction, color.r, color.g, color.b, message);
            return result;
        }
    }
}
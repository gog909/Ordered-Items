using System;
using System.Linq;
using System.Collections.Generic;

using RoR2;


namespace OrderedItems
{
    // Handles the mod's configuration file.
    public static class ConfigHandler
    {
        private static BepInEx.Configuration.ConfigFile Config;
        private static BepInEx.Configuration.ConfigEntry<bool> alphabeticOrderConfig;
        private static BepInEx.Configuration.ConfigEntry<string> tierOrderConfig;
        private static Dictionary<string, string> tierNameMap = new Dictionary<string, string>()
        {
            { "Tier1", "White" },
            { "Tier2", "Green" },
            { "Tier3", "Red" }
        };

        public const int tierOrderLength = 6;
        private static int[] tierOrderDefault = new int[ConfigHandler.tierOrderLength] { 3, 2, 4, 1, 0, 5 };
        private static char[] tierOrderValueLast = new char[ConfigHandler.tierOrderLength];
        private static bool alphabeticOrderLast = false;

        public static bool alphabeticOrder = false;
        public static int[] tierOrder = new int[ConfigHandler.tierOrderLength];


        public static void Init(BepInEx.Configuration.ConfigFile configSource)
        {
            ConfigHandler.Config = configSource;

            // Wrap the AlphabeticOrder setting, writing to config file if not present.
            ConfigHandler.alphabeticOrderConfig = ConfigHandler.Config.Bind
            (
                "Settings",
                "alphabeticOrder",
                false,
                "This setting determines if items are sorted by name instead of ID.\n" +    
                "If changed in game, changes will take effect on the next item pickup.\n" +
                "--------------------------------------------------------------------\n"
            );

            // Generates a message which relates each number to its enum name for the user config.
            string itemTierString = "";
            int length = Enum.GetValues(typeof(ItemTier)).Length;
            for (int i = 0; i < length; i++)
            {
                // Rename certain itemTiers according to tierNameMap.
                string rawTierName = Enum.GetName(typeof(ItemTier), (ItemIndex)i);
                if (!tierNameMap.TryGetValue(rawTierName, out string tierName))
                {
                    tierName = rawTierName;
                }

                itemTierString += i.ToString() + " = " + tierName; 

                // Don't include trailing comma on the last entry.
                if (i < length - 1)
                {
                    itemTierString += ", ";
                }
            }

            // Turns the tierOrderDefault member into a string for the config file.
            string tierOrderDefaultString = "";
            foreach (int i in ConfigHandler.tierOrderDefault)
            {
                tierOrderDefaultString += i.ToString();
            }

            // Wrap the TierOrder setting, writing to config file if not present.
            ConfigHandler.tierOrderConfig = ConfigHandler.Config.Bind
            (
                "Settings",
                "tierOrder",
                tierOrderDefaultString,
                "This setting determines the order in which items are sorted by tier,\n" +
                "   the order from left to right indicates the position in the\n" +
                "   itembar, with the order starting from the top left and continuing\n" +
                "   to the bottom right.\n" +
                "Each tier is represented by a number where,\n" +
                "   " + itemTierString +"\n" +
                "If changed in game, changes will take effect on the next item pickup.\n" +
                "--------------------------------------------------------------------\n" 
            );

            // Reminds the user every time a game starts to change config if it is invalid.
            RoR2.Run.onRunStartGlobal += (Run run) =>
            {
                // Potential logic error can occur here, if the new user config is five null characters
                // ConfigHandler will not tell the user that their config is invalid at the beginning 
                // of a run. Creating a dummy class which is valid for the 
                // System.Linq.Enumerable.SequenceEqual method may fix this.
                ConfigHandler.tierOrderValueLast = new char[ConfigHandler.tierOrderLength];
            };

            // Write the config file if it doesn't exist.
            ConfigHandler.Config.Save();
        }


        // Reloads the configuration file and updates the mod with new config.
        public static bool UpdateConfig()
        {
            ConfigHandler.Config.Reload();
            bool updated = false;

            // Checks if alphabeticOrder config has changed.
            bool alphabeticOrder = ConfigHandler.alphabeticOrderConfig.Value;
            if (ConfigHandler.alphabeticOrder != ConfigHandler.alphabeticOrderLast)
            {
                // Handles the changed alphabeticOrder config.
                ConfigHandler.HandleAlphabeticOrderConfig(alphabeticOrder);
                ConfigHandler.alphabeticOrderLast = alphabeticOrder;
                updated = true;
            }

            // Checks if tierOrder config has changed.
            char[] tierOrderValue = ConfigHandler.tierOrderConfig.Value.ToCharArray();
            if (!ConfigHandler.tierOrderValueLast.SequenceEqual(tierOrderValue))
            {
                // Handles the changed tierOrder config.
                ConfigHandler.HandleTierOrderConfig(tierOrderValue);
                ConfigHandler.tierOrderValueLast = (char[])tierOrderValue.Clone();
                updated = true;
            }

            return updated;
        }


        // Handles the AlphabeticOrder configuration.
        public static void HandleAlphabeticOrderConfig(bool alphabeticOrder)
        {
            // Update the alphabeticOrder config.
            ConfigHandler.alphabeticOrder = alphabeticOrder;

            // Tells the user that the alphabeticOrder config has changed.
            LogHandler.Log(
                "alphabeticOrder configuration is now " + alphabeticOrder.ToString(),
                true,
                BepInEx.Logging.LogLevel.Info
            );
        }


        // Manages converting the TierOrder value from a string to an integer array.
        public static void HandleTierOrderConfig(char[] tierOrderValue)
        {
            // Used to check that the characters are not repeated (eg. config doesn't have 012334)
            bool[] digitCheck = new bool[ConfigHandler.tierOrderLength];

            // Ensures that the user defined tierOrder has the correct length.
            bool valid = tierOrderValue.Length == ConfigHandler.tierOrderLength;

            // Iterates throught each character in tierOrderValue.
            int idx = 0;
            while (idx < ConfigHandler.tierOrderLength & valid)
            {
                // Converts to integer.
                int chr = (int)tierOrderValue[idx] - 48;

                // Checks that the integer is in range and has not already been used.
                if ((0 <= chr && chr < ConfigHandler.tierOrderLength) && !digitCheck[chr])
                {
                    digitCheck[chr] = true;
                    ConfigHandler.tierOrder[idx] = chr;
                }

                // If the integer has already been used, then the config is invalid.
                else
                {
                    valid = false;
                }

                idx += 1;
            }

            // Checks if the new tierOrder is invalid.
            if (!valid)
            {
                // Tells the user that their config is invalid (only once per run).
                LogHandler.Log(
                    "tierOrder configuration is invalid, using default.",
                    true,
                    BepInEx.Logging.LogLevel.Warning
                );

                // Uses the default tierOrder config.
                ConfigHandler.tierOrderDefault.CopyTo(ConfigHandler.tierOrder, 0);
            }
            else
            {
                // Build a string of the tierOrder data.
                string tierOrderString = "";
                foreach (int i in  ConfigHandler.tierOrder)
                {
                    tierOrderString += i.ToString();
                }

                // Tells the user that the config has been updated.
                LogHandler.Log(
                    "tierOrder configuration is now " + tierOrderString,
                    true,
                    BepInEx.Logging.LogLevel.Info
                );
            }
        }
    }
}
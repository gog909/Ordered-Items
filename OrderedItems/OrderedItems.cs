using BepInEx;
using RoR2;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;

namespace OrderedItems
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin(MODGUID, MODNAME, MODVER)]

    public class OrderedItems : BaseUnityPlugin
    {

        // Mod metadata.
        public const string MODGUID = "com.gog909.ordereditems";
        public const string MODNAME = "Ordered Items";
        public const string MODVER = "2.1.0";
        public const string Dependancy = MODGUID;

        public bool isRunActive;


        // Initialises mod at the beginning of the game.
        public void Awake()
        {

            // Initialises other classes.
            Reflection.Init();
            LogHandler.Init(this.Logger);
            ConfigHandler.Init(this.Config);

            // Injects inventory hook.
            On.RoR2.UI.ItemInventoryDisplay.OnInventoryChanged += this.InventoryHook;


            LogHandler.Log("Ordered Items is enabled.", false);
        }


        // Inventory hook which manages the UI display of the inventory
        public void InventoryHook(On.RoR2.UI.ItemInventoryDisplay.orig_OnInventoryChanged orig, RoR2.UI.ItemInventoryDisplay self)
        {
            // Reloads config each time the inventory ordering is called.
            ConfigHandler.UpdateConfig();

            // Short Circuit if ItemInventoryDisplay class is not enabled.
            if (!self.isActiveAndEnabled)
            {
                return;
            }

            // Handle ItemInventoryDisplay fields.
            var inventory = Reflection.cachedInventoryField.GetValue(self) as RoR2.Inventory;
            if (inventory == null)
            {
                Reflection.cachedItemStacksField.SetValue(self, ItemCatalog.RequestItemStackArray());
                Reflection.cachedItemOrderCountField.SetValue(self, 0);
            }
            else
            {
                this.OrderInventory(inventory, out ItemIndex[] itemOrder, out int[] itemStacks);
                Reflection.cachedItemStacksField.SetValue(self, itemStacks);
                Reflection.cachedItemOrderField.SetValue(self, itemOrder);
                Reflection.cachedItemOrderCountField.SetValue(self, itemOrder.Length);
            }

            // Update Display.
            Reflection.cachedRequestDisplayUpdateMethod.Invoke(self, new object[] { });
        }


        // Called by InventoryHook to actually order the inventories itemAcquisitionOrder list.
        public void OrderInventory(RoR2.Inventory inventory, out ItemIndex[] itemOrderOut, out int[] itemStacksOut)
        {
            // Reads itemstacks from inventory.
            int[] itemStacks = ItemCatalog.RequestItemStackArray();
            inventory.WriteItemStacks(itemStacks);

            //Read item tiers from itemstacks and organises by tier.
            var listWhite = new List<ItemIndex>();
            var listGreen = new List<ItemIndex>();
            var listBoss = new List<ItemIndex>();
            var listLunar = new List<ItemIndex>();
            var listRed = new List<ItemIndex>();
            var listHidden = new List<ItemIndex>();

            for (int i = 0; i < itemStacks.Length; i++)
            {
                if (itemStacks[i] > 0)
                {
                    ItemIndex index = (ItemIndex)i;
                    switch (ItemCatalog.GetItemDef(index).tier)
                    {
                        case ItemTier.Tier1:
                            listWhite.Add(index);
                            break;
                        case ItemTier.Tier2:
                            listGreen.Add(index);
                            break;
                        case ItemTier.Tier3:
                            listRed.Add(index);
                            break;
                        case ItemTier.Boss:
                            listBoss.Add(index);
                            break;
                        case ItemTier.Lunar:
                            listLunar.Add(index);
                            break;
                        default:
                            listHidden.Add(index);
                            break;
                    }
                }
            }

            var tierList = new List<ItemIndex>[]
            {
                    listWhite,
                    listGreen,
                    listBoss,
                    listLunar,
                    listRed
            };

            // Organizes items in each tier alphabetically, determined in config file.
            if (ConfigHandler.alphabeticOrder)
            {
                foreach (List<ItemIndex> list in tierList)
                {
                    list.Sort(new ItemNameComparer());
                }
            }

            // Orders items by tier using config file.
            List<ItemIndex> itemOrder = new List<ItemIndex>();

            for (int i = 0; i < 5; i++)
            {
                itemOrder.AddRange(tierList[ConfigHandler.tierOrder[i]]);
            }
            itemOrder.AddRange(listHidden);

            // Returns ordered inventory information.
            itemOrderOut = itemOrder.ToArray();
            itemStacksOut = itemStacks;
        }
    }

    // Comparer for alphabetic order.
    public class ItemNameComparer : IComparer<ItemIndex>
    {
        public int Compare(ItemIndex a, ItemIndex b)
        {
            string aName = Language.GetString(ItemCatalog.GetItemDef(a).nameToken);
            string bName = Language.GetString(ItemCatalog.GetItemDef(b).nameToken);
            if (aName == null || bName == null)
            {
                return 0;
            }

            return aName.CompareTo(bName);
        }
    }


    // Handles the mod's configuration file.
    public static class ConfigHandler
    {
        public static BepInEx.Configuration.ConfigFile Config;

        public static BepInEx.Configuration.ConfigWrapper<bool> alphabeticOrderWrapper;
        public static bool alphabeticOrder;

        public static BepInEx.Configuration.ConfigWrapper<string> tierOrderWrapper;
        public static int[] tierOrderDefault = new int[5] { 3, 4, 2, 1, 0 };
        public static int[] tierOrderLast;
        public static int[] tierOrder = new int[5];

        public static bool isFirstWarning = true;


        public static void Init(BepInEx.Configuration.ConfigFile configSource)
        {
            ConfigHandler.Config = configSource;

            ConfigHandler.alphabeticOrderWrapper = ConfigHandler.Config.Wrap
            (
                "Settings",
                "AlphabeticOrder",
                "This setting determines if items are sorted by name instead of ID.\n" +
                "--------------------------------------------------------------------\n" +
                "DEFAULT: false",
                false
            );

            ConfigHandler.tierOrderWrapper = ConfigHandler.Config.Wrap
            (
                "Settings",
                "TierOrder",
                "This setting determines the order in which items are sorted by tier,\n" +
                "   the order from left to right indicates the position in the\n" +
                "   itembar, with the order starting from the top left and continuing\n" +
                "   to the bottom right.\n" +
                "Each tier is represented by a number between 1 and 5 where,\n" +
                "   1 = White, 2 = Green, 3 = Boss, 4 = Lunar and 5 = Red.\n" +
                "--------------------------------------------------------------------\n" +
                "If changed in game, changes will take effect on the next item pickup\n" +
                "DEFAULT: 45321",
                "45321"
            );

            // Reminder every time a game starts to change config if broken.
            RoR2.Run.onRunStartGlobal += (Run run) =>
            {
                ConfigHandler.tierOrderLast = null;
            };

            ConfigHandler.Config.Save();
            ConfigHandler.UpdateConfig();
        }


        // Reloades the configuration file and updates the mod with new config.
        public static void UpdateConfig()
        {
            ConfigHandler.Config.Reload();

            ConfigHandler.alphabeticOrder = ConfigHandler.alphabeticOrderWrapper.Value;

            ConfigHandler.HandleTierOrderConfig();
        }


        // Resets the TierOrder value to default
        public static void DefaultTierOrderConfig()
        {

            // Handle warning on startup and during game.
            if (ConfigHandler.tierOrderLast != ConfigHandler.tierOrder)
            {
                ConfigHandler.tierOrderLast = ConfigHandler.tierOrder;
                if (ConfigHandler.isFirstWarning)
                {
                    ConfigHandler.isFirstWarning = false;
                    LogHandler.Log("TierOrder configuration is invalid, currently using default settings.", false, BepInEx.Logging.LogLevel.Warning);
                }
                else
                {
                    LogHandler.Log("TierOrder configuration is invalid, currently using default settings.", true, BepInEx.Logging.LogLevel.Warning);
                }
            }

            // Default the tierOrder value.
            if (ConfigHandler.tierOrder != ConfigHandler.tierOrderDefault)
            {
                ConfigHandler.tierOrderDefault.CopyTo(ConfigHandler.tierOrder, 0);
            }
        }


        // Manages converting the TierOrder value from a string to an integer array.
        public static void HandleTierOrderConfig()
        {
            string TierOrderValue = ConfigHandler.tierOrderWrapper.Value;

            if (TierOrderValue.Length < 5 | TierOrderValue.Length > 5)
            {
                ConfigHandler.DefaultTierOrderConfig();
            }
            else
            {
                var count = 0;
                foreach (char chr in TierOrderValue.ToCharArray())
                {
                    var i = (int)chr - 49;
                    if (0 <= i && i <= 4)
                    {
                        ConfigHandler.tierOrder[count] = i;
                        count += 1;
                    }
                    else
                    {
                        ConfigHandler.DefaultTierOrderConfig();
                        break;
                    }
                }
            }
        }
    }


    // Handles the mod's logging.
    public static class LogHandler
    {
        public static BepInEx.Logging.ManualLogSource Logger;

        public static string prefix;

        public static Color32[] Colors = new Color32[]
        {
            new Color32(111, 10, 170, 255),
            new Color32(14, 83, 167, 255),
            new Color32(0, 155, 149, 255),
            new Color32(255, 113, 0, 255),
            new Color32(255, 40, 0, 255)
        };


        public static void Init(BepInEx.Logging.ManualLogSource loggerSource)
        {
            LogHandler.Logger = loggerSource;
            LogHandler.GeneratePrefix();
        }


        public static void Log(string message, bool chat, BepInEx.Logging.LogLevel priority = BepInEx.Logging.LogLevel.Debug)
        { 
            if (chat)
            {
                LogHandler.GeneratePrefix();
                Chat.AddMessage(LogHandler.prefix + message);
            }
            LogHandler.Logger.Log(priority, message);
        }


        public static void GeneratePrefix()
        {
            LogHandler.prefix = ("[" + OrderedItems.MODNAME + " v" + OrderedItems.MODVER + "]: ")
                .Coloured(LogHandler.Colors[UnityEngine.Random.Range(0, LogHandler.Colors.Length)]);
        }


        public static string Coloured(this string message, Color32 color)
        {
            string construction = "<color=#{0:X2}{1:X2}{2:X2}>{3}</color>";
            string result = string.Format(construction, color.r, color.g, color.b, message);
            return result;
        }
    }

    // Cached reflection fields and types.
    public static class Reflection
    {
        public static System.Type cachedItemInventoryDisplayType;
        public static FieldInfo cachedInventoryField;
        public static FieldInfo cachedItemOrderCountField;
        public static FieldInfo cachedItemOrderField;
        public static FieldInfo cachedItemStacksField;
        public static MethodInfo cachedRequestDisplayUpdateMethod;

        public static void Init()
        {
            Reflection.cachedItemInventoryDisplayType = typeof(RoR2.UI.ItemInventoryDisplay);
            Reflection.cachedInventoryField = cachedItemInventoryDisplayType.GetField("inventory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); 
            Reflection.cachedItemOrderCountField = cachedItemInventoryDisplayType.GetField("itemOrderCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Reflection.cachedItemOrderField = cachedItemInventoryDisplayType.GetField("itemOrder", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Reflection.cachedItemStacksField = cachedItemInventoryDisplayType.GetField("itemStacks", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Reflection.cachedRequestDisplayUpdateMethod = cachedItemInventoryDisplayType.GetMethod("RequestUpdateDisplay", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
}
}
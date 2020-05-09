using System;
using System.Reflection;
using System.Collections.Generic;

using RoR2;
using BepInEx;


namespace OrderedItems
{
    //[BepInDependency("com.bepis.r2api")]
    [BepInPlugin(MODGUID, MODNAME, MODVER)]

    public class OrderedItems : BaseUnityPlugin
    {
        public const string MODGUID = "com.gog909.ordereditems";
        public const string MODNAME = "Ordered Items";
        public const string MODVER = "2.2.1";
        public const string Dependancy = MODGUID;


        // Initialises mod at the beginning of the game.
        public void Awake()
        {
#if DEBUG
            // Enables Godmode.
            On.RoR2.CharacterMaster.Awake += (On.RoR2.CharacterMaster.orig_Awake orig, RoR2.CharacterMaster self) => {
                orig(self);
                typeof(RoR2.CharacterMaster).GetMethod("ToggleGod", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(self, new object[] { });
            };
#endif

            // Initialises other classes.
            CachedReflection.Init();
            LogHandler.Init(this.Logger);
            ConfigHandler.Init(this.Config);

            // Injects ItemInventoryDisplay hook.
            On.RoR2.UI.ItemInventoryDisplay.OnInventoryChanged += this.OnInventoryChanged;

            LogHandler.Log("Ordered Items is enabled.");
        }

        
        // ItemInventoryDisplay hook.
        public void OnInventoryChanged(On.RoR2.UI.ItemInventoryDisplay.orig_OnInventoryChanged orig, RoR2.UI.ItemInventoryDisplay self)
        {
            // Reloads config each time the inventory ordering is called, if the config has changed
            // then resort the items before updating the display.
            if (ConfigHandler.UpdateConfig())
            {
                ItemSorter.GenerateSortedItems();
            }

            // Short Circuit if ItemInventoryDisplay class is not enabled.
            if (!self.isActiveAndEnabled)
            {
                return;
            }

            // Handle ItemInventoryDisplay fields.
            var inventory = CachedReflection.InventoryField.GetValue(self) as RoR2.Inventory;
            if (inventory == null)
            {
                int[] itemStacks = (int[])CachedReflection.ItemStacksField.GetValue(self);
                Array.Clear(itemStacks, 0, itemStacks.Length);
                CachedReflection.ItemOrderCountField.SetValue(self, 0);
            }
            else
            {
                ItemSorter.SortItems(inventory, self, out int[] itemStacks, out ItemIndex[] itemOrder);
                CachedReflection.ItemStacksField.SetValue(self, itemStacks);
                CachedReflection.ItemOrderField.SetValue(self, itemOrder);
                CachedReflection.ItemOrderCountField.SetValue(self, itemOrder.Length);
            }

            // Updates the display.
            CachedReflection.RequestDisplayUpdateMethod.Invoke(self, new object[] { });
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
}
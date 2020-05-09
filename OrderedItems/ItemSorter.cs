using System;
using System.Collections.Generic;

using RoR2;


namespace OrderedItems
{
    class ItemSorter
    {
        private static List<List<ItemIndex>> sortedItems;

        public static void SortItems(RoR2.Inventory inventory, RoR2.UI.ItemInventoryDisplay display, out int[] itemStacks, out ItemIndex[] itemOrder)
        {
            // Reads itemStacks from inventory.
            itemStacks = CachedReflection.ItemStacksField.GetValue(display) as int[];
            inventory.WriteItemStacks(itemStacks);

            // Counts the number of items in the inventory to initialise itemOrder array
            // with the correct length.
            int itemOrderCount = 0;
            for (int item = 0; item < itemStacks.Length; item++)
            {
                if (itemStacks[item] > 0)
                {
                    itemOrderCount += 1;
                }
            }

            itemOrder = new ItemIndex[itemOrderCount];

            // Iterates through each item in sorted order and adds to itemOrder array if 
            // itemStacks contains one or more of that item.
            int idx = 0;
            for (int tier = 0; tier < ItemSorter.sortedItems.Count; tier++)
            {
                foreach (ItemIndex item in ItemSorter.sortedItems[tier])
                {
                    if (itemStacks[(int)item] > 0)
                    {
                        itemOrder[idx] = item;
                        idx += 1;
                    }
                }
            }
        }

        public static void GenerateSortedItems()
        {
            int[] itemStacks = ItemCatalog.RequestItemStackArray();

            // Generate lists for each ItemTier.
            var tierList = new List<List<ItemIndex>>();
            foreach (ItemTier i in (ItemTier[])Enum.GetValues(typeof(ItemTier)))
            {
                tierList.Add(new List<ItemIndex>());
            }

            // Add each item to its associated tier list.
            for (int item = 0; item < itemStacks.Length; item++)
            {
                var tier = (int)ItemCatalog.GetItemDef((ItemIndex)item).tier;
                tierList[tier].Add((ItemIndex)item);
            }

            // Performs alphabetic sorting.
            if (ConfigHandler.alphabeticOrder)
            {
                foreach (List<ItemIndex> tier in tierList)
                {
                    tier.Sort(new ItemNameComparer());
                }
            }

            // Clears previous sortedItems and performs tier sorting.
            ItemSorter.sortedItems = new List<List<ItemIndex>>();
            for (int i = 0; i < tierList.Count; i++)
            {
                ItemSorter.sortedItems.Add(tierList[ConfigHandler.tierOrder[i]]);
            }

            ItemCatalog.ReturnItemStackArray(itemStacks);
        }
    }
}

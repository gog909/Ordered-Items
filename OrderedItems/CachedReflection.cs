using System.Reflection;


namespace OrderedItems
{
    // Holds cached reflection fields and types.
    public static class CachedReflection
    {
        public static System.Type ItemInventoryDisplayType;
        public static FieldInfo InventoryField;
        public static FieldInfo ItemOrderCountField;
        public static FieldInfo ItemOrderField;
        public static FieldInfo ItemStacksField;
        public static MethodInfo RequestDisplayUpdateMethod;

        private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;


        // Finds and caches all of the stored reflection fields and types.
        public static void Init()
        {
            CachedReflection.ItemInventoryDisplayType = typeof(RoR2.UI.ItemInventoryDisplay);
            CachedReflection.InventoryField = ItemInventoryDisplayType.GetField("inventory", flags);
            CachedReflection.ItemOrderCountField = ItemInventoryDisplayType.GetField("itemOrderCount", flags);
            CachedReflection.ItemOrderField = ItemInventoryDisplayType.GetField("itemOrder", flags);
            CachedReflection.ItemStacksField = ItemInventoryDisplayType.GetField("itemStacks", flags);
            CachedReflection.RequestDisplayUpdateMethod = ItemInventoryDisplayType.GetMethod("RequestUpdateDisplay", flags);
        }
    }
}
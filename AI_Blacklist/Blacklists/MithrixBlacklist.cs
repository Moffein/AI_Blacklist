using RoR2;
using System.Collections.Generic;

namespace AI_Blacklist
{
    public class MithrixBlacklist
    {
        public static bool useVanillaMithrixBlacklist = false;
        public static Dictionary<ItemIndex, int> mithrixItemLimits;
        public static string mithrixBlacklistString;

        public MithrixBlacklist()
        {
            mithrixItemLimits = new Dictionary<ItemIndex, int>();

            On.RoR2.ItemCatalog.Init += (orig) =>
            {
                orig();

                mithrixBlacklistString = new string(mithrixBlacklistString.ToCharArray().Where(c => !System.Char.IsWhiteSpace(c)).ToArray());
                string[] splitBlacklist = mithrixBlacklistString.Split(',');
                foreach (string str in splitBlacklist)
                {
                    string[] current = str.Split('-');

                    if (current.Length == 2 && int.TryParse(current[1], out int cap) && cap > 0)
                    {
                        ItemIndex index = ItemCatalog.FindItemIndex(current[0]);
                        if (index != ItemIndex.None)
                        {
                            mithrixItemLimits.Add(index, cap);
                        }
                    }
                    else if (current.Length > 0)
                    {
                        AddToMithrixBlacklist(current[0]);
                    }
                }
            };

            On.RoR2.ItemStealController.FixedUpdate += (orig, self) =>
            {
                orig(self);
                if (self.lendeeInventory)
                {
                    foreach (KeyValuePair<ItemIndex, int> pair in mithrixItemLimits)
                    {
                        int count = self.lendeeInventory.GetItemCount(pair.Key);
                        if (count > pair.Value)
                        {
                            self.lendeeInventory.RemoveItem(pair.Key, count - pair.Value);
                        }
                    }
                }
            };
        }

        public void AddToMithrixBlacklist(string itemName)
        {
            ItemIndex i = ItemCatalog.FindItemIndex(itemName);
            if (i != ItemIndex.None)
            {
                AddToMithrixBlacklist(i);
            }
        }

        public static void AddToMithrixBlacklist(ItemIndex index)
        {
            //Debug.Log("Adding BrotherBlacklist tag to ItemIndex " + index);
            ItemDef itemDef = ItemCatalog.GetItemDef(index);
            if (itemDef.DoesNotContainTag(ItemTag.BrotherBlacklist))
            {
                System.Array.Resize(ref itemDef.tags, itemDef.tags.Length + 1);
                itemDef.tags[itemDef.tags.Length - 1] = ItemTag.BrotherBlacklist;
            }
        }
    }
}

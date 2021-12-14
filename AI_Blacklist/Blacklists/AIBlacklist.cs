using RoR2;
using System.Collections.Generic;
using System;
using System.Linq;

namespace AI_Blacklist
{
    public class AIBlacklist
    {
        public static bool useVanillaAIBlacklist = false;
        public static string itemBlacklistString;

        public AIBlacklist()
        {
            On.RoR2.ItemCatalog.Init += (orig) =>
            {
                orig();

                itemBlacklistString = new string(itemBlacklistString.ToCharArray().Where(c => !System.Char.IsWhiteSpace(c)).ToArray());
                string[] splitBlacklist = itemBlacklistString.Split(',');
                foreach (string str in splitBlacklist)
                {
                    AddToBlacklist(str);
                }
            };
        }

        public static void AddToBlacklist(string itemName)
        {
            ItemIndex i = ItemCatalog.FindItemIndex(itemName);
            if (i != ItemIndex.None)
            {
                AddToBlacklist(i);
            }
        }

        public static void AddToBlacklist(ItemIndex index)
        {
            ItemDef itemDef = ItemCatalog.GetItemDef(index);
            if (itemDef.DoesNotContainTag(ItemTag.AIBlacklist))
            {
                System.Array.Resize(ref itemDef.tags, itemDef.tags.Length + 1);
                itemDef.tags[itemDef.tags.Length - 1] = ItemTag.AIBlacklist;
            }
        }
    }
}

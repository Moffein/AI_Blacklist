
using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace AI_Blacklist
{
    public class VengeanceBlacklist
    {
        public static bool fixVengeanceScaling = true;
        public static string vengeanceItemBlacklistString;
        public static HashSet<ItemIndex> vengeanceItemBlacklist;

        public VengeanceBlacklist()
        {
            On.RoR2.ItemCatalog.Init += (orig) =>
            {
                orig();

                //Build vengeanceBlacklist
                vengeanceItemBlacklist = new HashSet<ItemIndex>();
                vengeanceItemBlacklistString = new string(vengeanceItemBlacklistString.ToCharArray().Where(c => !System.Char.IsWhiteSpace(c)).ToArray());
                string[] vsplitBlacklist = vengeanceItemBlacklistString.Split(',');
                foreach (string str in vsplitBlacklist)
                {
                    AddToVengeanceBlacklist(str);
                }
            };
        }

        public static void AddToVengeanceBlacklist(string itemName)
        {
            ItemIndex i = ItemCatalog.FindItemIndex(itemName);
            if (i != ItemIndex.None)
            {
                AddToVengeanceBlacklist(i);
            }
        }

        public static void AddToVengeanceBlacklist(ItemIndex item)
        {
            vengeanceItemBlacklist.Add(item);
        }
    }
}

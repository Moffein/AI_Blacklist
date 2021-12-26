
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

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

            //Remove Blacklisted items from Vengeance Clones
            if (fixVengeanceScaling || vengeanceItemBlacklist.Count > 0)
            {
                On.RoR2.CharacterBody.Start += (orig2, self) =>
                {
                    orig2(self);
                    if (NetworkServer.active && self.inventory)
                    {
                        if (fixVengeanceScaling)
                        {
                            self.inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);
                        }
                        if (vengeanceItemBlacklist.Count > 0)
                        {
                            foreach (ItemIndex item in vengeanceItemBlacklist)
                            {
                                int itemCount = self.inventory.GetItemCount(item);
                                if (itemCount > 0)
                                {
                                    self.inventory.RemoveItem(item, itemCount);
                                }
                            }
                        }
                    }
                };
            }
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

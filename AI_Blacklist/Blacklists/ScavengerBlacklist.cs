using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI_Blacklist
{
    public class ScavengerBlacklist
    {
        public static bool useScavBlacklist = false;
        public static string scavBlacklistString;
        public static HashSet<ItemDef> scavBlacklist;

        public ScavengerBlacklist()
        {
            if (!useScavBlacklist) return;

            scavBlacklist = new HashSet<ItemDef>();

            On.RoR2.ItemCatalog.Init += (orig) =>
            {
                orig();
                scavBlacklistString = new string(scavBlacklistString.ToCharArray().Where(c => !System.Char.IsWhiteSpace(c)).ToArray());
                string[] splitBlacklist = scavBlacklistString.Split(',');
                foreach (string str in splitBlacklist)
                {
                    AddToBlacklist(str);
                }
            };

            //Scavenger Item Granting uses the Scavenger Blacklist instead of AI Blacklist
            On.RoR2.ScavengerItemGranter.Start += (orig, self) =>
            {
                Inventory inventory = self.GetComponent<Inventory>();

                List<PickupIndex> list = Run.instance.availableTier1DropList.Where(PickupIsNonBlacklistedItem).ToList();
                List<PickupIndex> list2 = Run.instance.availableTier2DropList.Where(PickupIsNonBlacklistedItem).ToList();
                List<PickupIndex> list3 = Run.instance.availableTier3DropList.Where(PickupIsNonBlacklistedItem).ToList();

                List<PickupIndex> availableEquipmentDropList = Run.instance.availableEquipmentDropList;

                self.GrantItems(inventory, list, self.tier1Types, self.tier1StackSize);
                self.GrantItems(inventory, list2, self.tier2Types, self.tier2StackSize);
                self.GrantItems(inventory, list3, self.tier3Types, self.tier3StackSize);

                if (self.overwriteEquipment || inventory.currentEquipmentIndex == EquipmentIndex.None)
                {
                    inventory.GiveRandomEquipment();
                }
            };

            On.EntityStates.ScavMonster.FindItem.PickupIsNonBlacklistedItem += (orig, self, pickupIndex) =>
            {
                return PickupIsNonBlacklistedItem(pickupIndex);
            };
        }

        public static bool PickupIsNonBlacklistedItem(PickupIndex pickupIndex)
        {
            PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
            if (pickupDef == null)
            {
                return false;
            }
            ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
            return !(itemDef == null) && !scavBlacklist.Contains(itemDef);
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
            scavBlacklist.Add(itemDef);
        }
    }
}

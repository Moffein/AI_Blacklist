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

                //Old Code
                /*List<PickupIndex> list1 = Run.instance.availableTier1DropList.Where(PickupIsNonBlacklistedItem).ToList();
                List<PickupIndex> list2 = Run.instance.availableTier2DropList.Where(PickupIsNonBlacklistedItem).ToList();
                List<PickupIndex> list3 = Run.instance.availableTier3DropList.Where(PickupIsNonBlacklistedItem).ToList();
                self.GrantItems(inventory, list, self.tier1Types, self.tier1StackSize);
                self.GrantItems(inventory, list2, self.tier2Types, self.tier2StackSize);
                self.GrantItems(inventory, list3, self.tier3Types, self.tier3StackSize);*/

                //New Code
                foreach (ScavengerItemGranter.StackRollData stackRollData in self.stackRollDataList)
                {
                    if (stackRollData.dropTable)
                    {
                        for (int j = 0; j < stackRollData.numRolls; j++)
                        {
                            PickupDef pickupDef = PickupCatalog.GetPickupDef(stackRollData.dropTable.GenerateDrop(ScavengerItemGranter.rng));

                            List<ItemIndex> itemList = Run.instance.availableItems.Where(IsNonBlacklistedItem).ToList();
                            itemList = SelectItemTier(itemList, pickupDef.itemTier);

                            if (itemList.Count > 0)
                            {
                                int dropIndex = ScavengerItemGranter.rng.RangeInt(0, itemList.Count);
                                inventory.GiveItem(itemList[dropIndex], stackRollData.stacks);
                            }
                        }
                    }
                }

                if (self.overwriteEquipment || inventory.currentEquipmentIndex == EquipmentIndex.None)
                {
                    inventory.GiveRandomEquipment(ScavengerItemGranter.rng);
                }
            };

            On.EntityStates.ScavMonster.FindItem.PickupIsNonBlacklistedItem += (orig, self, pickupIndex) =>
            {
                return PickupIsNonBlacklistedItem(pickupIndex);
            };
        }

        private static List<ItemIndex> SelectItemTier(List<ItemIndex> itemList, ItemTier tier)
        {
            List<ItemIndex> toRemove = new List<ItemIndex>();
            foreach (ItemIndex itemIndex in itemList)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                if (itemDef.tier != tier || itemIndex == ItemIndex.None)
                {
                    toRemove.Add(itemIndex);
                }
            }

            foreach(ItemIndex remove in toRemove)
            {
                itemList.Remove(remove);
            }

            return itemList;
        }

        public static bool IsNonBlacklistedItem(ItemIndex itemIndex)
        {
            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
            return !(itemDef == null) && !scavBlacklist.Contains(itemDef);
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
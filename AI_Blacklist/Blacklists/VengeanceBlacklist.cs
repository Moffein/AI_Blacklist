
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace AI_Blacklist
{
    public class VengeanceBlacklist
    {
        public static bool fixVengeanceScaling = true;
        public static string vengeanceItemBlacklistString;
        public static HashSet<ItemIndex> vengeanceItemBlacklist;
        public static bool useAIBlacklist = true;
        public static bool useTurretBlacklist = true;

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
            if (fixVengeanceScaling || vengeanceItemBlacklist.Count > 0 || useAIBlacklist || useTurretBlacklist)
            {
                RoR2.CharacterMaster.onStartGlobal += RunVengeanceChanges;
            }
        }

        private void RunVengeanceChanges(CharacterMaster self)
        {
            if (NetworkServer.active && self.inventory && self.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0)
            {
                if (fixVengeanceScaling)
                {
                    int alCount = self.inventory.GetItemCount(RoR2Content.Items.UseAmbientLevel);
                    if (alCount > 0) self.inventory.RemoveItem(RoR2Content.Items.UseAmbientLevel, alCount);

                    int lbCount = self.inventory.GetItemCount(RoR2Content.Items.LevelBonus);
                    if (lbCount > 0) self.inventory.RemoveItem(RoR2Content.Items.LevelBonus, lbCount);

                    self.inventory.GiveItem(RoR2Content.Items.LevelBonus, (int)TeamManager.instance.GetTeamLevel(TeamIndex.Player) - 1);
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

                //Seems inefficient
                if (useAIBlacklist || useTurretBlacklist)
                {
                    foreach (ItemDef item in ItemCatalog.itemDefs)
                    {
                        if ((useAIBlacklist && item.ContainsTag(ItemTag.AIBlacklist)) || (useTurretBlacklist && item.ContainsTag(ItemTag.CannotCopy)))
                        {
                            int itemCount = self.inventory.GetItemCount(item);
                            if (itemCount > 0)
                            {
                                self.inventory.RemoveItem(item, itemCount);
                            }
                        }
                    }
                }
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

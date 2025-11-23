using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI_Blacklist
{
    public class AIEquipBlacklist
    {
        public static HashSet<EquipmentIndex> equipBlacklist;
        public static HashSet<EliteDef> allowedEliteDefs;

        public static bool blacklistVengeanceEquipment = false;
        public static string equipmentBlacklistString;

        public AIEquipBlacklist()
        {
            //Build Equipment blacklist
            On.RoR2.EquipmentCatalog.Init += (orig) =>
            {
                orig();
                equipBlacklist = new HashSet<EquipmentIndex>();
                equipmentBlacklistString = new string(equipmentBlacklistString.ToCharArray().Where(c => !System.Char.IsWhiteSpace(c)).ToArray());
                string[] splitEquipBlacklist = equipmentBlacklistString.Split(',');
                foreach (string str in splitEquipBlacklist)
                {
                    EquipmentIndex ei = EquipmentCatalog.FindEquipmentIndex(str);
                    if (ei != EquipmentIndex.None)
                    {
                        equipBlacklist.Add(ei);
                    }
                }

                if (equipBlacklist.Count > 0)
                {
                    On.RoR2.Inventory.SetEquipmentIndexForSlot_EquipmentIndex_uint_uint += Inventory_SetEquipmentIndexForSlot_EquipmentIndex_uint_uint;
                }
            };
        }

        private void Inventory_SetEquipmentIndexForSlot_EquipmentIndex_uint_uint(On.RoR2.Inventory.orig_SetEquipmentIndexForSlot_EquipmentIndex_uint_uint orig, Inventory self, EquipmentIndex newEquipmentIndex, uint slot, uint set)
        {
            CharacterMaster cm = self.gameObject.GetComponent<CharacterMaster>();
            if (cm && cm.teamIndex != TeamIndex.Player && cm.aiComponents != null && cm.aiComponents.Length > 0)
            {
                Inventory inv = cm.inventory;
                if (inv)
                {
                    if (inv.GetItemCount(RoR2Content.Items.InvadingDoppelganger) <= 0)
                    {
                        if (equipBlacklist.Contains(newEquipmentIndex))
                        {
                            newEquipmentIndex = GetRandomNonBlacklistEquipment();
                        }
                    }
                }
            }

            orig(self, newEquipmentIndex, slot, set);
        }

        private static IEnumerable<PickupIndex> EquipmentToPickupIndices(IEnumerable<EquipmentIndex> equipIndices)
        {
            foreach (var index in equipIndices)
            {
                yield return PickupCatalog.FindPickupIndex(index);
            }
        }

        private static EquipmentIndex GetRandomNonBlacklistEquipment()
        {
            IEnumerable<PickupIndex> blacklistEquips = EquipmentToPickupIndices(equipBlacklist as IEnumerable<EquipmentIndex>);
            List<PickupIndex> equipsExceptBlacklist = Run.instance?.availableEquipmentDropList.Except(blacklistEquips).ToList();

            if (equipsExceptBlacklist is null)
            {
                return EquipmentIndex.None;
            }

            PickupDef randomEquip = PickupCatalog.GetPickupDef(equipsExceptBlacklist[UnityEngine.Random.Range(0, equipsExceptBlacklist.Count)]);
            return randomEquip.equipmentIndex;
        }
    }
}

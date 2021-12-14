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

                    //This seems really inefficient.
                    allowedEliteDefs = new HashSet<EliteDef>();
                    for (int i = 0; i < EliteCatalog.eliteDefs.Length; i++)
                    {
                        if (EliteCatalog.eliteDefs[i].eliteEquipmentDef)
                        {
                            bool isAllowed = !(EliteCatalog.eliteDefs[i].eliteIndex == RoR2Content.Elites.Echo.eliteIndex
                            || EliteCatalog.eliteDefs[i].eliteIndex == RoR2Content.Elites.Gold.eliteIndex
                            || EliteCatalog.eliteDefs[i].eliteIndex == RoR2Content.Elites.Lunar.eliteIndex
                            || EliteCatalog.eliteDefs[i].eliteIndex == RoR2Content.Elites.Poison.eliteIndex
                            || EliteCatalog.eliteDefs[i].eliteIndex == RoR2Content.Elites.Haunted.eliteIndex);
                            if (isAllowed)
                            {
                                foreach (EquipmentIndex ei in equipBlacklist)
                                {
                                    EquipmentDef ed = EquipmentCatalog.GetEquipmentDef(ei);
                                    if (ed == EliteCatalog.eliteDefs[i].eliteEquipmentDef)
                                    {
                                        isAllowed = false;
                                        break;
                                    }
                                }
                            }
                            if (isAllowed)
                            {
                                allowedEliteDefs.Add(EliteCatalog.eliteDefs[i]);
                            }
                        }
                    }

                    //This seems really inefficient.
                    On.RoR2.Inventory.SetEquipmentIndex += (orig2, self, equipmentIndex) =>
                    {
                        CharacterMaster cm = self.gameObject.GetComponent<CharacterMaster>();
                        if (cm && cm.teamIndex != TeamIndex.Player && cm.aiComponents != null && cm.aiComponents.Length > 0)
                        {
                            Inventory inv = cm.inventory;
                            if (inv)
                            {
                                if (inv.GetItemCount(RoR2Content.Items.InvadingDoppelganger) <= 0)
                                {
                                    foreach (EquipmentIndex ei in equipBlacklist)
                                    {
                                        if (ei == equipmentIndex)
                                        {
                                            EquipmentDef ed = EquipmentCatalog.GetEquipmentDef(ei);
                                            if (ed.passiveBuffDef && ed.passiveBuffDef.isElite)
                                            {
                                                equipmentIndex = GetRandomAllowedElite();
                                            }
                                            else
                                            {
                                                equipmentIndex = GetRandomNonBlacklistEquipment();
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        orig2(self, equipmentIndex);
                    };
                }
            };
        }


        //These functions are based on https://github.com/Unordinal/UnosRoR2Mods/tree/master/AIBlacklister
        private static EquipmentIndex GetRandomAllowedElite()
        {
            if (allowedEliteDefs.Count > 0)
            {
                EliteDef selectedElite = (allowedEliteDefs.ToList<EliteDef>())[Random.Range(0, allowedEliteDefs.Count)];
                return selectedElite.eliteEquipmentDef.equipmentIndex;
            }
            else
            {
                return EquipmentIndex.None;
            }
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

﻿using RoR2;
using BepInEx;
using R2API.Utils;
using BepInEx.Configuration;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AI_Blacklist
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.AI_Blacklist", "AI Blacklist", "1.1.0")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class AI_Blacklist : BaseUnityPlugin
    {
        private static List<EquipmentIndex> equipBlacklist;
        private static List<EliteDef> allowedEliteDefs;

        public void Awake()
        {
            string itemBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Settings", "Item Blacklist"), "ShockNearby, NovaOnHeal", new ConfigDescription("List item codenames separated by commas (ex. Behemoth, ShockNearby, Clover). List of item codenames can be found at https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names")).Value;
            string equipmentBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Settings", "Equipment Blacklist"), "", new ConfigDescription("List equipment codenames separated by commas. List of item codenames can be found at https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names")).Value;

            //Blacklist items
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

            //Build Equipment blacklist
            On.RoR2.EquipmentCatalog.Init += (orig) =>
            {
                orig();

                equipBlacklist = new List<EquipmentIndex>();
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
                    allowedEliteDefs = new List<EliteDef>();
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
                        if (cm && cm.aiComponents != null && cm.aiComponents.Length > 0)
                        {
                            Inventory inv = cm.inventory;
                            if (inv)
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
                        orig2(self, equipmentIndex);
                    };
                }
            };
        }

        public void AddToBlacklist(string itemName)
        {
            ItemIndex i = ItemCatalog.FindItemIndex(itemName);
            if (i != ItemIndex.None)
            {
                AddToBlacklist(i);
            }
        }
        public void AddToBlacklist(ItemIndex index)
        {
            ItemDef itemDef = ItemCatalog.GetItemDef(index);
            if (itemDef.DoesNotContainTag(ItemTag.AIBlacklist))
            {
                System.Array.Resize(ref itemDef.tags, itemDef.tags.Length + 1);
                itemDef.tags[itemDef.tags.Length - 1] = ItemTag.AIBlacklist;
            }
        }


        //Based on https://github.com/Unordinal/UnosRoR2Mods/tree/master/AIBlacklister
        #region equipment
        private static EquipmentIndex GetRandomAllowedElite()
        {
            if (allowedEliteDefs.Count > 0)
            {
                EliteDef selectedElite = allowedEliteDefs[Random.Range(0, allowedEliteDefs.Count)];
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
        #endregion
    }
}

using RoR2;
using BepInEx;
using BepInEx.Configuration;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AI_Blacklist
{
    [BepInPlugin("com.Moffein.AI_Blacklist", "AI Blacklist", "1.4.0")]
    public class AI_Blacklist : BaseUnityPlugin
    {
        public void ReadConfig()
        {
            AIBlacklist.useVanillaAIBlacklist = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Item Blacklist - Use Vanilla Blacklist"), true, new ConfigDescription("Automatically blacklist items that are blacklisted in vanilla.")).Value;
            AIBlacklist.itemBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Settings", "Item Blacklist"), "ShockNearby, NovaOnHeal, Thorns", new ConfigDescription("List item codenames separated by commas (ex. Behemoth, ShockNearby, Clover). List of item codenames can be found at https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names")).Value;
            
            AIEquipBlacklist.equipmentBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Settings", "Equipment Blacklist"), "", new ConfigDescription("List equipment codenames separated by commas. List of item codenames can be found at https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names")).Value;
            
            VengeanceBlacklist.vengeanceItemBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Vengeance Settings", "Vengeance Item Blacklist"), "ShockNearby, NovaOnHeal, Thorns", new ConfigDescription("Item Blacklist for Vengeance Clones. Same format as the global AI item blacklist.")).Value;
            VengeanceBlacklist.fixVengeanceScaling = base.Config.Bind<bool>(new ConfigDefinition("Vengeance Settings", "Fix Scaling"), true, new ConfigDescription("Fix Vengeance clones always being level 1.")).Value;

            MithrixBlacklist.useVanillaMithrixBlacklist = base.Config.Bind<bool>(new ConfigDefinition("Mithrix Settings", "Use Vanilla Blacklist"), true, new ConfigDescription("Automatically blacklist items that are blacklisted in vanilla.")).Value;
            MithrixBlacklist.mithrixBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Mithrix Settings", "Mithrix Blacklist"), "ShockNearby, NovaOnHeal, Thorns", new ConfigDescription("List item codenames separated by commas (ex. Behemoth, ShockNearby, Clover). To specify an item cap instead, enter a - followed by the max cap (ex. Behemoth - 5, ShockNearby, Clover - 1). List of item codenames can be found at https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names")).Value;

            ScavengerBlacklist.useScavBlacklist = base.Config.Bind<bool>(new ConfigDefinition("Scavenger Settings", "Use Separate Scavenger Blacklist"), false, new ConfigDescription("Scavengers get a separate blacklist from the generic AI Blacklist.")).Value;
            ScavengerBlacklist.scavBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Scavenger Settings", "Scavenger Blacklist"), "ShockNearby, NovaOnHeal, Dagger, Feather, Mushroom, FallBoots, WardOnLevel, StunChanceOnHit, Firework, TreasureCache, BossDamageBonus, HeadHunter, KillEliteFrenzy, ExecuteLowHealthElite, TPHealingNova, LunarUtilityReplacement, LunarPrimaryReplacement, Thorns, Squid, FocusConvergence, MonstersOnShrineUse",
                new ConfigDescription("List item codenames separated by commas (ex. Behemoth, ShockNearby, Clover). List of item codenames can be found at https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names Vanilla AI Blacklist is included by default.")).Value;
        }

        public void Awake()
        {
            ReadConfig();
            new AIBlacklist();
            new VengeanceBlacklist();
            new AIEquipBlacklist();
            new MithrixBlacklist();
            new ScavengerBlacklist();
        }
    }
}

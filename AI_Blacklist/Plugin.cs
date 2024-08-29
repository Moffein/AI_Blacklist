using BepInEx;
using BepInEx.Configuration;
using System;

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute
    {
    }
}

namespace AI_Blacklist
{
    [BepInPlugin("com.Moffein.AI_Blacklist", "AI Blacklist", "1.6.4")]
    public class AI_Blacklist : BaseUnityPlugin
    {
        public void ReadConfig()
        {
            AIBlacklist.useVanillaAIBlacklist = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Item Blacklist - Use Vanilla Blacklist"), true, new ConfigDescription("Automatically blacklist items that are blacklisted in vanilla.")).Value;
            AIBlacklist.itemBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Settings", "Item Blacklist"), "DelayedDamage, NegateAttack, ExtraStatsOnLevelUp, TeleportOnLowHealth, ExtraShrineItem, TriggerEnemyDebuffs, LowerPricedChests, GoldOnStageStart, ResetChests, Icicle, PrimarySkillShuriken, ImmuneToDebuff, ShockNearby, NovaOnHeal, Thorns, DroneWeapons, FreeChest, RegeneratingScrap", new ConfigDescription("List item codenames separated by commas (ex. Behemoth, ShockNearby, Clover). List of item codenames can be found by using the list_item console command from the DebugToolKit mod.")).Value;
            
            AIEquipBlacklist.equipmentBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Settings", "Equipment Blacklist"), "HealAndRevive, MultiShopCard, BossHunter, BossHunterConsumed, VendingMachine, LeaveStage, CLASSICITEMSRETURNS_EQUIPMENT_CREATEGHOSTTARGETING, CLASSICITEMSRETURNS_EQUIPMENT_LOSTDOLL, CursedScythe", new ConfigDescription("List equipment codenames separated by commas. List of item codenames can be found by using the list_item console command from the DebugToolKit mod.")).Value;
            
            VengeanceBlacklist.vengeanceItemBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Vengeance Settings", "Vengeance Item Blacklist"), "NegateAttack, TriggerEnemyDebuffs, ShockNearby, NovaOnHeal, Thorns", new ConfigDescription("Item Blacklist for Vengeance Clones. Same format as the global AI item blacklist.")).Value;
            VengeanceBlacklist.fixVengeanceScaling = base.Config.Bind<bool>(new ConfigDefinition("Vengeance Settings", "Fix Scaling"), true, new ConfigDescription("Fix Vengeance clones always being level 1.")).Value;
            VengeanceBlacklist.useAIBlacklist = base.Config.Bind<bool>(new ConfigDefinition("Vengeance Settings", "Use AI Blacklist"), true, new ConfigDescription("Automatically remove items with the AIBlacklist tag.")).Value;
            VengeanceBlacklist.useTurretBlacklist = base.Config.Bind<bool>(new ConfigDefinition("Vengeance Settings", "Use Engi Turret Blacklist"), true, new ConfigDescription("Automatically remove items with the CannotCopy tag.")).Value;

            MithrixBlacklist.useVanillaMithrixBlacklist = base.Config.Bind<bool>(new ConfigDefinition("Mithrix Settings", "Use Vanilla Blacklist"), true, new ConfigDescription("Automatically blacklist items that are blacklisted in vanilla.")).Value;
            MithrixBlacklist.mithrixBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Mithrix Settings", "Mithrix Blacklist"), "ShockNearby, NovaOnHeal, Thorns", new ConfigDescription("List item codenames separated by commas (ex. Behemoth, ShockNearby, Clover). To specify an item cap instead, enter a - followed by the max cap (ex. Behemoth - 5, ShockNearby, Clover - 1). List of item codenames can be found by using the list_item console command from the DebugToolKit mod.")).Value;
            MithrixBlacklist.blacklistAllItems = base.Config.Bind<bool>(new ConfigDefinition("Mithrix Settings", "Blacklist All Items (EXPERIMENTAL)"), false, new ConfigDescription("Blacklist EVERY item from Mithrix. MAY CAUSE UNEXPECTED BEHAVIOR.")).Value;

            ScavengerBlacklist.useScavBlacklist = base.Config.Bind<bool>(new ConfigDefinition("Scavenger Settings", "Use Separate Scavenger Blacklist"), false, new ConfigDescription("Scavengers get a separate blacklist from the generic AI Blacklist. Enabling this might break compatibility with mods that modify Scavengers.")).Value;
            ScavengerBlacklist.scavBlacklistString = base.Config.Bind<string>(new ConfigDefinition("Scavenger Settings", "Scavenger Blacklist"), "PrimarySkillShuriken, ImmuneToDebuff, DroneWeapons, FreeChest, RegeneratingScrap, SprintBonus, SprintArmor, MushroomVoid, BossDamageBonus, Dagger, ExecuteLowHealthElite, FallBoots, Feather, Firework, FocusConvergence, GoldOnHurt, HeadHunter, HealingPotion, KillEliteFrenzy, LunarPrimaryReplacement, LunarSecondaryReplacement, LunarSpecialReplacement, LunarUtilityReplacement, MonstersOnShrineUse, Mushroom, Squid, StunChanceOnHit, TPHealingNova, Thorns, TreasureCache, TreasureCacheVoid, WardOnLevel, ShockNearby, NovaOnHeal",
                new ConfigDescription("List item codenames separated by commas (ex. Behemoth, ShockNearby, Clover). List of item codenames can be found by using the list_item console command from the DebugToolKit mod. Vanilla AI Blacklist is included by default.")).Value;
        }

        public void Awake()
        {
            ReadConfig();

            /*On.RoR2.ItemCatalog.Init += (orig) =>
            {
                orig();

                foreach(ItemDef id in ItemCatalog.itemDefs)
                {
                    Debug.Log(id.name);
                }

                Debug.Log("\n\n\nAI Blacklist:");
                foreach (ItemDef id in ItemCatalog.itemDefs)
                {
                    if (id.ContainsTag(ItemTag.AIBlacklist))
                    {
                        Debug.Log(id.name);
                    }
                }

                Debug.Log("\n\n\nBrother Blacklist:");
                foreach (ItemDef id in ItemCatalog.itemDefs)
                {
                    if (id.ContainsTag(ItemTag.BrotherBlacklist))
                    {
                        Debug.Log(id.name);
                    }
                }
            };*/

            new AIBlacklist();
            new VengeanceBlacklist();
            new AIEquipBlacklist();
            new MithrixBlacklist();
            new ScavengerBlacklist();
        }
    }
}

using RoR2;
using BepInEx;
using R2API.Utils;
using BepInEx.Configuration;
using System.Linq;

namespace AI_Blacklist
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.AI_Blacklist", "AI Blacklist", "1.0.0")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class AI_Blacklist : BaseUnityPlugin
    {
        public void Awake()
        {
            string blacklistString = base.Config.Bind<string>(new ConfigDefinition("Settings", "Blacklist"), "ShockNearby, NovaOnHeal", new ConfigDescription("List item codenames separated by commas (ex. Behemoth, ShockNearby, Clover). List of item codenames can be found at https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names")).Value;

            On.RoR2.ItemCatalog.Init += (orig) =>
            {
                orig();

                blacklistString = new string(blacklistString.ToCharArray().Where(c => !System.Char.IsWhiteSpace(c)).ToArray());
                string[] splitBlacklist = blacklistString.Split(',');
                foreach (string str in splitBlacklist)
                {
                    AddToBlacklist(str);
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
                itemDef.tags[itemDef.tags.Length - 1] = ItemTag.BrotherBlacklist;
            }
        }
    }
}

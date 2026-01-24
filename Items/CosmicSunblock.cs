using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Items;

public class CosmicSunblock : ModItem
{
    static LocalizedText apocalypseOver;
    static LocalizedText tooLate;

    public override void SetStaticDefaults()
    {
        apocalypseOver = Language.GetOrRegister("Mods.SolarDoomsday.ApocalypseEnd");
        tooLate = Language.GetOrRegister("Mods.SolarDoomsday.TooLate");
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 28;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 60;
        Item.useAnimation = 60;
        Item.UseSound = SoundID.Item120;
        Item.consumable = true;
        Item.rare = ItemRarityID.Red;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight || player.ZoneUnderworldHeight)
        {
            return false;
        }
        if (DoomsdayManager.savedEverybody)
        {
            return false;
        }
        return true;
    }

    public override void UpdateInventory(Player player)
    {
        Item.consumable = true;
        if (DoomsdayManager.sunDied || DoomsdayManager.savedEverybody)
        {
            Item.UseSound = SoundID.Item1;
            Item.useTime = 10;
            Item.useAnimation = 10;
        }
        else
        {
            Item.UseSound = SoundID.Item120;
            Item.useTime = 60;
            Item.useAnimation = 60;
        }
    }

    public override bool? UseItem(Player player)
    {
        Item.consumable = !DoomsdayManager.sunDied;
        if (DoomsdayManager.sunDied)
        {
            if (DoomsdayManager.sentTheMessage)
            {
                return true;
            }
            Main.NewText(tooLate.Value, 50, 255, 130);
            DoomsdayManager.sentTheMessage = true;
            return true;
        }
        DoomsdayManager.shaderTime = 120;
        Main.NewText(apocalypseOver.Value, 50, 255, 130);
        DoomsdayManager.savedEverybody = true;
        return true;
    }
}

public class SunblockDropping : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type != NPCID.MoonLordCore)
        {
            return;
        }
        npcLoot.Add(ItemDropRule.ByCondition(new WorldNotSavedCondition(), ModContent.ItemType<CosmicSunblock>()));
    }

    public class WorldNotSavedCondition : IItemDropRuleCondition
    {
        private static LocalizedText Desc;

        public WorldNotSavedCondition()
        {
            Desc ??= Language.GetOrRegister("Mods.SolarDoomsday.DropConditions.WorldNotSavedCondition");
        }

        public bool CanDrop(DropAttemptInfo info)
        {
            return DoomsdayClock.Ongoing;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return Desc.Value;
        }
    }
}

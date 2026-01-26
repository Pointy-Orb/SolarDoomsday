using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Items;

public class SolarAccelerationPill : ModItem
{
    private static LocalizedText apocalypseAgain;

    public override void SetStaticDefaults()
    {
        apocalypseAgain = Language.GetOrRegister("Mods.SolarDoomsday.Announcements.ApocalypseAgain");
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Red;
        Item.width = 20;
        Item.height = 26;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 60;
        Item.useAnimation = 60;
        Item.UseSound = SoundID.Roar;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight || player.ZoneUnderworldHeight)
        {
            return false;
        }
        if (!Main.IsItDay())
        {
            return false;
        }
        if (DoomsdayClock.Ongoing)
        {
            return false;
        }
        return true;
    }

    public override bool? UseItem(Player player)
    {
        Item.consumable = !DoomsdayManager.sunDied;
        DoomsdayManager.savedEverybody = false;
        DoomsdayClock.daysLeft = DoomsdayClock.DayCount;
        DoomsdayManager.shaderTime = 70;
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return true;
        }
        if (Main.dedServ)
        {
            ChatHelper.BroadcastChatMessage(apocalypseAgain.ToNetworkText(), new Color(50, 255, 130));
        }
        else
        {
            Main.NewText(apocalypseAgain.Value, 50, 255, 130);
        }
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.FragmentSolar, 20)
            .AddIngredient(ItemID.LunarOre, 5)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Items;

public class Match : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 50;
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item73;
        Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 15;
        Item.useTurn = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.autoReuse = true;
        Item.consumable = true;
        Item.value = Item.sellPrice(copper: 50);
    }

    public override bool CanUseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
        {
            return false;
        }
        Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
        if (!tile.HasTile && tile.WallType == 0)
        {
            return false;
        }
        return true;
    }

    public override void HoldItem(Player player)
    {
        if (!CanUseItem(player))
        {
            return;
        }
        if (!(player.position.X / 16f - (float)Player.tileRangeX - (float)player.blockRange <= (float)Player.tileTargetX)
                || !((player.position.X + (float)player.width) / 16f + (float)Player.tileRangeX + (float)player.blockRange >= (float)Player.tileTargetX)
                || !(player.position.Y / 16f - (float)Player.tileRangeY - (float)player.blockRange <= (float)Player.tileTargetY)
                || !((player.position.Y + (float)player.height) / 16f + (float)Player.tileRangeY - 2f + (float)player.blockRange >= (float)Player.tileTargetY))
        {
            return;
        }
        player.cursorItemIconID = Type;
        player.cursorItemIconEnabled = true;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
        {
            return true;
        }
        if (!(player.position.X / 16f - (float)Player.tileRangeX - (float)player.blockRange <= (float)Player.tileTargetX)
                || !((player.position.X + (float)player.width) / 16f + (float)Player.tileRangeX + (float)player.blockRange >= (float)Player.tileTargetX)
                || !(player.position.Y / 16f - (float)Player.tileRangeY - (float)player.blockRange <= (float)Player.tileTargetY)
                || !((player.position.Y + (float)player.height) / 16f + (float)Player.tileRangeY - 2f + (float)player.blockRange >= (float)Player.tileTargetY))
        {
            return false;
        }
        Fire.SetOnFire(Player.tileTargetX, Player.tileTargetY);
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe(5)
            .AddIngredient(ItemID.Wood, 1)
            .AddIngredient(ItemID.Hellstone, 1)
            .Register();
    }
}

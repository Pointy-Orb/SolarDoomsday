using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Walls;

public class AshWall : ModWall
{
    public override void SetStaticDefaults()
    {
        AddMapEntry(new Color(39, 39, 44));
    }
}

public class AshWallSafe : ModWall
{
    public override string Texture => ModContent.GetInstance<AshWall>().Texture;

    public override void SetStaticDefaults()
    {
        AddMapEntry(new Color(39, 39, 44));
        Main.wallHouse[Type] = true;
        Main.wallBlend[Type] = ModContent.WallType<AshWall>();
    }
}

public class AshWallItem : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
        ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<AshWallSafe>());
        Item.width = 24;
        Item.height = 24;
    }

    public override void AddRecipes()
    {
        CreateRecipe(4)
            .AddIngredient(ItemID.AshBlock)
            .AddTile(TileID.WorkBenches)
            .Register();

        Recipe.Create(ItemID.AshBlock)
            .AddIngredient(this, 4)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

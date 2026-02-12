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
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SuperAliveFire>());
        Item.width = 20;
        Item.height = 20;
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item73;
        Item.value = Item.sellPrice(copper: 50);
    }

    public override void AddRecipes()
    {
        CreateRecipe(5)
            .AddIngredient(ItemID.Wood, 1)
            .AddIngredient(ItemID.Hellstone, 1)
            .Register();
    }
}

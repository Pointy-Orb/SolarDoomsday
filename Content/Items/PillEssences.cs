using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Items;

public class PillEssences : ModItem
{
    public override string Texture => "SolarDoomsday/Content/Items/PillEssences";

    public DoomsdayOptions WorldEndMethod { get; private set; }

    public PillEssences(DoomsdayOptions WorldEndMethod)
    {
        this.WorldEndMethod = WorldEndMethod;
    }

    private Color color => SolarAccelerationPill.auraColor[(int)WorldEndMethod];

    public override void SetDefaults()
    {
        Item.color = color;
        Item.rare = ItemRarityID.Red;
        Item.width = 26;
        Item.height = 26;
    }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemIconPulse[Type] = true;
        ItemID.Sets.ItemNoGravity[Type] = true;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        Lighting.AddLight(Item.position, color.R, color.G, color.B);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}

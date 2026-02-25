using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Tiles;

public class Hornfels : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileBlendAll[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileShine[Type] = 20000;
        Main.tileLighted[Type] = true;

        MineResist = 2f;

        TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;

        HitSound = SoundID.Tink;
        DustType = DustID.MeteorHead;
        AddMapEntry(new Color(84, 0, 0));
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (j > Main.rockLayer || fail || effectOnly || j < Main.worldSurface || !DoomsdayClock.Ongoing)
        {
            return;
        }
        Tile tile = Main.tile[i, j];
        tile.LiquidAmount = 128;
        tile.LiquidType = LiquidID.Lava;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0f;
        g = 0f;
        b = 0f;
        if (WorldGen.TileIsExposedToAir(i, j))
        {
            r = 0.5f;
            g = 0.2f;
        }
    }
}

public class HornfelsItem : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Hornfels>());
    }
}

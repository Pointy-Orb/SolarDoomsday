using Terraria;
using SolarDoomsday.Content.Tiles;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ID;
using SolarDoomsday.Content.Walls;

namespace SolarDoomsday;

public class AshConversion : ModBiomeConversion
{
    public override void PostSetupContent()
    {
        FlammabilitySystem.MarkFlammability();

        for (int i = 0; i < TileLoader.TileCount; i++)
        {
            if (i == TileID.DirtiestBlock || i == TileID.Ash)
            {
                continue;
            }
            if (TileID.Sets.Dirt[i] || TileID.Sets.Conversion.Dirt[i] || AshVictims.Contains(i) || TileID.Sets.Conversion.Sand[i] || TileID.Sets.Conversion.HardenedSand[i] || TileID.Sets.CanBeDugByShovel[i])
            {
                TileLoader.RegisterConversion(i, Type, TileID.Ash);
            }
        }

        for (int i = 0; i < TileLoader.TileCount; i++)
        {
            if (TileID.Sets.Stone[i] || TileID.Sets.Ore[i] || TileID.Sets.Conversion.Stone[i] || TileID.Sets.Conversion.Sandstone[i] || MeltVictims.Contains(i))
            {
                TileLoader.RegisterConversion(i, Type, MeltTile);
            }
        }

        for (int i = 0; i < WallLoader.WallCount; i++)
        {
            if (WallID.Sets.Conversion.Dirt[i] || WallID.Sets.Conversion.HardenedSand[i])
            {
                WallLoader.RegisterConversion(i, Type, WallID.LavaUnsafe1);
            }
            else if (!SuperAliveFire.FlammableWall[i] && i != WallID.ObsidianBrick && i != WallID.LavaUnsafe1 && i != WallID.Lava1Echo && !Main.wallDungeon[i])
            {
                WallLoader.RegisterConversion(i, Type, WallID.LavaUnsafe3);
            }
        }
    }

    private bool MeltTile(int i, int j, int type, int conversionType)
    {
        if (WorldGen.InWorld(i, j - 1) && (TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Main.tile[i, j - 1].TileType] || TileID.Sets.BasicChest[Main.tile[i, j - 1].TileType]))
        {
            return false;
        }
        if (!(j < Main.worldSurface || Main.rand.NextBool(3) || DoomsdayClock.LastDay))
        {
            return false;
        }
        Tile tile = Main.tile[i, j];
        tile.HasTile = false;
        WorldGen.PlaceLiquid(i, j, (byte)LiquidID.Lava, 255);
        WorldGen.Reframe(i, j);
        return false;
    }

    private readonly int[] AshVictims = new int[]
    {
        TileID.RedBrick,
        TileID.ClayBlock,
        TileID.SnowBrick,
        TileID.IceBrick,
        TileID.Mudstone,
    };

    private readonly int[] MeltVictims = new int[]
    {
        TileID.GrayBrick,
        TileID.SandstoneBrick,
        TileID.IridescentBrick,
        TileID.CopperBrick,
        TileID.AncientCopperBrick,
        TileID.TinBrick,
        TileID.IronBrick,
        TileID.LeadBrick,
        TileID.SilverBrick,
        TileID.AncientSilverBrick,
        TileID.TungstenBrick,
        TileID.GoldBrick,
        TileID.AncientGoldBrick,
        TileID.PlatinumBrick,
        TileID.EbonstoneBrick,
        TileID.CrimstoneBrick,
        TileID.PearlstoneBrick
    };
}

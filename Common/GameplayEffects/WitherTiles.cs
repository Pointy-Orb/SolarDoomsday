using Terraria;
using SolarDoomsday.Tiles;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.GameplayEffects;

public class WitherTiles : GlobalTile
{
    public override void Load()
    {
        On_Liquid.Update += EvaporateWater;
    }

    private static void EvaporateWater(On_Liquid.orig_Update orig, Liquid self)
    {
        orig(self);
        var tile = Main.tile[self.x, self.y];
        if (tile.LiquidType != LiquidID.Water)
        {
            return;
        }
        if (self.y > Main.worldSurface && !DoomsdayClock.TimeLeftInRange(2))
        {
            return;
        }
        if (self.y > Main.rockLayer)
        {
            return;
        }
        if (!Main.IsItDay() || Main.raining)
        {
            return;
        }
        if (DoomsdayManager.savedEverybody)
        {
            return;
        }
        if (DoomsdayClock.TimeLeftInRange(6, 5))
        {
            if (Main.rand.NextBool(4))
            {
                tile.LiquidAmount -= byte.Min(tile.LiquidAmount, 2);
            }
            if (DoomsdayClock.TimeLeftInRange(2))
            {
                tile.LiquidAmount -= byte.Min(tile.LiquidAmount, 2);
            }
            if (DoomsdayClock.TimeLeftInRange(3))
            {
                tile.LiquidAmount -= byte.Min(tile.LiquidAmount, 10);
            }
        }
    }

    public override void RandomUpdate(int i, int j, int type)
    {
        if (DoomsdayManager.savedEverybody)
        {
            return;
        }
        if (j > Main.worldSurface && !DoomsdayClock.TimeLeftInRange(2))
        {
            return;
        }
        if (j > Main.rockLayer)
        {
            return;
        }
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }
        bool didSomething = false;
        if (DoomsdayManager.sunDied && Main.rand.NextBool(90))
        {
            if (type == TileID.Ash)
            {
                Main.tile[i, j].TileType = TileID.SnowBlock;
                didSomething = true;
            }
            if (type == TileID.Stone)
            {
                Main.tile[i, j].TileType = TileID.IceBlock;
                didSomething = true;
            }
        }
        else if (DoomsdayManager.sunDied && Main.rand.NextBool(10))
        {
            int x = Main.rand.Next(0, Main.maxTilesX);
            int y = Main.rand.Next(0, Main.maxTilesY);
            for (int k = x - 1; k <= x + 1; k++)
            {
                for (int l = y - 1; l <= y + 1; l++)
                {
                    if (!WorldGen.InWorld(k, l))
                    {
                        continue;
                    }
                    Tile tile = Main.tile[k, l];
                    if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Lava)
                    {
                        tile.LiquidAmount = 0;
                        WorldGen.PlaceTile(k, l, TileID.Stone, true);
                    }
                }
            }
            if (Main.dedServ)
            {
                NetMessage.SendTileSquare(-1, x - 1, y - 1, 3, 3);
            }
        }
        if (Main.rand.NextBool() && !DoomsdayClock.TimeLeftInRange(10))
        {
            goto serverSync;
        }
        if (!Main.IsItDay() || (Main.raining && DoomsdayClock.DayCount >= 9))
        {
            goto serverSync;
        }
        if (type == TileID.PalmTree && DoomsdayClock.TimeLeftInRange(3))
        {
            WorldGen.KillTile(i, j, noItem: true);
        }
        if (WorldGen.InWorld(i, j - 1) && TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Main.tile[i, j - 1].TileType] && !TileID.Sets.IsATreeTrunk[Main.tile[i, j - 1].TileType])
        {
            return;
        }
        if (TileID.Sets.IsATreeTrunk[type] && DoomsdayClock.TimeLeftInRange(2))
        {
            WorldGen.GetTreeBottom(i, j, out var k, out var l);
            while (WorldGen.InWorld(k, l - 1) && TileID.Sets.IsATreeTrunk[Main.tile[k, l - 1].TileType])
            {
                l--;
            }
            Main.tile[k, l].TileType = (ushort)ModContent.TileType<SuperAliveFire>();
            didSomething = true;
            WorldGen.KillWall(k, l);
            WorldGen.Reframe(k, l);
        }
        if (DoomsdayClock.TimeLeftInRange(3) && (Main.rand.NextBool(3) || DoomsdayClock.TimeLeftInRange(6)))
        {
            WorldGen.Convert(i, j, ModContent.GetInstance<AshConversion>().Type, 0, true, true);
            didSomething = true;
        }
        if (DoomsdayClock.TimeLeftInRange(3, 2) && TileID.Sets.Dirt[Main.tile[i, j + 1].TileType] && Main.tileCut[type])
        {
            WorldGen.KillTile(i, j);
            didSomething = true;
        }
        if (DoomsdayClock.TimeLeftInRange(6, 5) && (Main.rand.NextBool(7) || DoomsdayClock.TimeLeftInRange(2)))
        {
            if (TileID.Sets.Snow[Main.tile[i, j].TileType])
            {
                if (WorldGen.TileIsExposedToAir(i, j))
                {
                    Main.tile[i, j].TileType = TileID.Grass;
                }
                else
                {
                    Main.tile[i, j].TileType = TileID.Dirt;
                }
                WorldGen.Reframe(i, j);
                didSomething = true;
            }
            if (TileID.Sets.Ices[Main.tile[i, j].TileType])
            {
                Main.tile[i, j].TileType = TileID.Stone;
                WorldGen.Reframe(i, j);
                didSomething = true;
            }
            if (type == TileID.BreakableIce)
            {
                WorldGen.KillTile(i, j);
            }
            if (Main.tile[i, j].WallType == WallID.SnowWallUnsafe)
            {
                Main.tile[i, j].WallType = WallID.Dirt;
                NetMessage.SendTileSquare(-1, i, j, 1, 1);
            }
        }
        if (DoomsdayClock.TimeLeftInRange(3, 2) && (Main.rand.NextBool(3) || DoomsdayClock.TimeLeftInRange(3)))
        {
            if (type == TileID.Mud)
            {
                Main.tile[i, j].TileType = TileID.Dirt;
                WorldGen.Reframe(i, j);
                NetMessage.SendTileSquare(-1, i, j, 1, 1);
            }
            if (TileID.Sets.Leaves[type])
            {
                WorldGen.KillTile(i, j);
            }
        }
        if (DoomsdayClock.TimeLeftInRange(3) && SuperAliveFire.Flammable[type])
        {
            WorldGen.KillTile(i, j, noItem: true);
            WorldGen.PlaceTile(i, j, ModContent.TileType<SuperAliveFire>(), true);
            WorldGen.KillWall(i, j);
            WorldGen.Reframe(i, j);
            didSomething = true;
        }
        if (DoomsdayClock.TimeLeftInRange(3, 2) && (Main.rand.NextBool(6) || DoomsdayClock.TimeLeftInRange(2)))
        {
            if (type == TileID.JungleGrass)
            {
                Main.tile[i, j].TileType = TileID.Grass;
                WorldGen.Reframe(i, j);
                NetMessage.SendTileSquare(-1, i, j, 1, 1);
            }
            for (int k = i - 2; k <= i + 2; k++)
            {
                for (int l = j - 2; l <= j + 2; l++)
                {
                    if (TileID.Sets.Grass[Main.tile[k, l].TileType])
                    {
                        Main.tile[k, l].TileType = TileID.Dirt;
                        WorldGen.Reframe(k, l);
                    }
                }
            }
            NetMessage.SendTileSquare(-1, i - 1, j - 1, 3, 3, TileChangeType.None);
        }
    serverSync:
        if (didSomething)
        {
            NetMessage.SendTileSquare(-1, i, j, 1, 1);
        }
    }
}

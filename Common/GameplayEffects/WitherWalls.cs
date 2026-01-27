using Terraria;
using SolarDoomsday.Tiles;
using SolarDoomsday.Walls;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.GameplayEffects;

public class WitherWalls : GlobalWall
{
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
        if (type <= 0)
        {
            return;
        }

        bool didSomething = false;
        if (DoomsdayManager.sunDied && Main.rand.NextBool(90))
        {
            if (type == WallID.LavaUnsafe1)
            {
                Main.tile[i, j].WallType = WallID.SnowWallUnsafe;
                didSomething = true;
            }
            if (type == WallID.LavaUnsafe3)
            {
                Main.tile[i, j].WallType = WallID.IceUnsafe;
                didSomething = true;
            }
        }
        if (!Main.IsItDay())
        {
            goto serverSync;
        }

        if (DoomsdayClock.TimeLeftInRange(6, 5) && (Main.rand.NextBool(7) || DoomsdayClock.TimeLeftInRange(2)))
        {
            if (type == WallID.SnowWallUnsafe || type == WallID.SnowWallEcho)
            {
                WorldGen.ConvertWall(i, j, WallID.Dirt);
                didSomething = true;
            }
            if (type == WallID.IceUnsafe || type == WallID.IceEcho)
            {
                WorldGen.ConvertWall(i, j, WallID.Stone);
                didSomething = true;
            }
        }
        if (DoomsdayClock.TimeLeftInRange(3) && SuperAliveFire.FlammableWall[type])
        {
            WorldGen.PlaceTile(i, j, ModContent.TileType<SuperAliveFire>(), true);
            WorldGen.KillWall(i, j, true);
            WorldGen.ConvertWall(i, j, 0);
            WorldGen.Reframe(i, j);
            didSomething = true;
        }
        if (DoomsdayClock.TimeLeftInRange(3, 2) && (Main.rand.NextBool(3) || DoomsdayClock.TimeLeftInRange(3)))
        {
            if (type == WallID.MudUnsafe || type == WallID.MudWallEcho)
            {
                WorldGen.ConvertWall(i, j, WallID.DirtUnsafe);
                didSomething = true;
            }
            if (type == WallID.Grass || type == WallID.GrassUnsafe || type == WallID.Flower || type == WallID.FlowerUnsafe || type == WallID.Jungle || type == WallID.JungleUnsafe)
            {
                WorldGen.ConvertWall(i, j, WallID.DirtUnsafe);
                didSomething = true;
            }
        }
        if (DoomsdayClock.TimeLeftInRange(3) && !Main.wallDungeon[type] && (Main.rand.NextBool(3) || DoomsdayClock.TimeLeftInRange(6)))
        {
            WorldGen.Convert(i, j, ModContent.GetInstance<AshConversion>().Type, 0, true, true);
        }

    serverSync:
        if (Main.dedServ && didSomething)
        {
            NetMessage.SendTileSquare(-1, i, j, 1, 1);
        }
    }
}

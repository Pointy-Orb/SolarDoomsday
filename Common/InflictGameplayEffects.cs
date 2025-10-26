using Terraria;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace SolarDoomsday;

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
        if (!Main.IsItDay())
        {
            return;
        }
        if (DoomsdayClock.TimeLeftInRange(6, 5))
        {
            if (Main.rand.NextBool(7))
            {
                tile.LiquidAmount -= byte.Min(tile.LiquidAmount, 2);
            }
            if (DoomsdayClock.TimeLeftInRange(2))
            {
                tile.LiquidAmount -= byte.Min(tile.LiquidAmount, 2);
            }
            if (DoomsdayClock.TimeLeftInRange(3))
            {
                tile.LiquidAmount -= byte.Min(tile.LiquidAmount, 2);
            }
        }
    }

    public override void RandomUpdate(int i, int j, int type)
    {
        if (j > Main.worldSurface && !DoomsdayClock.TimeLeftInRange(2))
        {
            return;
        }
        if (j > Main.rockLayer)
        {
            return;
        }
        if (Main.rand.NextBool() && !DoomsdayClock.TimeLeftInRange(10))
        {
            return;
        }
        if (!Main.IsItDay())
        {
            return;
        }
        if (type == TileID.PalmTree && DoomsdayClock.TimeLeftInRange(3))
        {
            WorldGen.KillTile(i, j, noItem: true);
        }
        if (TileID.Sets.IsATreeTrunk[type] && DoomsdayClock.TimeLeftInRange(3))
        {
            var k = j;
            while (TileID.Sets.IsATreeTrunk[Main.tile[i, k + 1].TileType])
            {
                k++;
            }
            WorldGen.KillTile(i, k, noItem: true);
        }
        if (DoomsdayClock.TimeLeftInRange(6) && (TileID.Sets.Dirt[type] || TileID.Sets.isDesertBiomeSand[type]))
        {
            Main.tile[i, j].TileType = TileID.Ash;
            WorldGen.Reframe(i, j);
        }
        if (DoomsdayClock.TimeLeftInRange(6) && TileID.Sets.Stone[type])
        {
            WorldGen.KillTile(i, j, noItem: true);
            WorldGen.PlaceLiquid(i, j, (byte)LiquidID.Lava, 255);
        }
        if (DoomsdayClock.TimeLeftInRange(6, 5) && (Main.rand.NextBool(7) || DoomsdayClock.TimeLeftInRange(2)))
        {
            if (TileID.Sets.Snow[Main.tile[i, j].TileType])
            {
                Main.tile[i, j].TileType = TileID.Dirt;
                WorldGen.Reframe(i, j);
            }
            if (TileID.Sets.Ices[Main.tile[i, j].TileType])
            {
                Main.tile[i, j].TileType = TileID.Stone;
                WorldGen.Reframe(i, j);
            }
            if (type == TileID.BreakableIce)
            {
                WorldGen.KillTile(i, j);
            }
            if (Main.tile[i, j].WallType == WallID.SnowWallUnsafe)
            {
                Main.tile[i, j].WallType = WallID.Dirt;
            }
        }
        if (DoomsdayClock.TimeLeftInRange(3, 2) && (Main.rand.NextBool(3) || DoomsdayClock.TimeLeftInRange(3)))
        {
            if (type == TileID.Mud)
            {
                Main.tile[i, j].TileType = TileID.Dirt;
                WorldGen.Reframe(i, j);
            }
            if (TileID.Sets.Leaves[type])
            {
                WorldGen.KillTile(i, j);
            }
        }
        if (DoomsdayClock.TimeLeftInRange(3) && (Main.rand.NextBool(4) || DoomsdayClock.TimeLeftInRange(6)))
        {
            if (type == TileID.JungleGrass)
            {
                Main.tile[i, j].TileType = TileID.Grass;
                WorldGen.Reframe(i, j);
            }
            if (TileID.Sets.Grass[Main.tile[i, j].TileType])
            {
                return;
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
        }
    }
}

public class Weather : ModSystem
{
    public override void PostUpdateTime()
    {
        if (DoomsdayClock.TimeLeftInRange(2) && Main.raining && !CreativePowerManager.Instance.GetPower<CreativePowers.FreezeRainPower>().Enabled && Main.IsItDay())
        {
            Main.StopRain();
        }
    }
}

public class BurnEverybody : ModSystem
{
    public override void PostUpdateTime()
    {
        if (!DoomsdayClock.TimeLeftInRange(3))
        {
            return;
        }
        if (!Main.IsItDay())
        {
            return;
        }
        foreach (Player player in Main.ActivePlayers)
        {
            if ((player.ZoneOverworldHeight || player.ZoneSkyHeight) && (!player.behindBackWall || DoomsdayClock.TimeLeftInRange(6)))
            {
                if (!player.fireWalk)
                {
                    player.AddBuff(BuffID.Burning, 10);
                }
                if (DoomsdayClock.TimeLeftInRange(6))
                {
                    player.AddBuff(BuffID.OnFire, 120);
                }
            }
        }
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.buffImmune[BuffID.OnFire] || npc.buffImmune[BuffID.OnFire3] || npc.type == NPCID.OldMan || npc.type == NPCID.CultistArcherBlue || npc.aiStyle == NPCAIStyleID.LunaticDevote)
            {
                continue;
            }
            Point npcSpot = npc.Center.ToTileCoordinates();
            if (npcSpot.Y < Main.worldSurface && (Main.tile[npcSpot.X, npcSpot.Y].WallType == 0 || DoomsdayClock.TimeLeftInRange(6)))
            {
                npc.AddBuff(BuffID.OnFire, 10);
            }
            if (npcSpot.Y < Main.worldSurface && (Main.tile[npcSpot.X, npcSpot.Y].WallType == 0 && DoomsdayClock.LastDay))
            {
                npc.AddBuff(BuffID.OnFire3, 10);
            }
        }
    }
}

public class FireMonsters : GlobalNPC
{
    public override bool CheckDead(NPC npc)
    {
        if (!DoomsdayClock.TimeLeftInRange(3))
        {
            return true;
        }
        if (npc.position.ToTileCoordinates().Y > Main.worldSurface)
        {
            return true;
        }
        if (npc.aiStyle != NPCAIStyleID.Slime)
        {
            return true;
        }
        if (!npc.HasBuff(BuffID.OnFire))
        {
            return true;
        }
        npc.Transform(NPCID.LavaSlime);
        return false;
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (!DoomsdayClock.TimeLeftInRange(3))
        {
            return;
        }
        if (spawnInfo.SpawnTileY > Main.worldSurface)
        {
            return;
        }
        pool[NPCID.Hellbat] = 0.8f;
    }
}

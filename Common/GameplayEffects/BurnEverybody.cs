using Terraria;
using SolarDoomsday.Content.Buffs;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Linq;

namespace SolarDoomsday;

public class BurnEverybody : ModSystem
{
    public override void PostAddRecipes()
    {
        safeWalls = new bool[TileLoader.TileCount];
        for (int i = 0; i < TileLoader.TileCount; i++)
        {
            if (safeWallsCore.Contains(i))
            {
                safeWalls[i] = true;
            }
        }
    }

    public override void PostUpdateTime()
    {
        if (!Main.IsItDay())
        {
            return;
        }
        if (DoomsdayManager.savedEverybody)
        {
            return;
        }

        if (DoomsdayClock.TimeLeftInRange(3))
        {
            IncinerateEveryone();
        }
        if (DoomsdayClock.TimeLeftInRange(3, 2))
        {
            DiscomfortEveryone();
        }
    }

    public static bool[] safeWalls = new bool[1];

    private static readonly int[] safeWallsCore = new int[]
    {
        WallID.ObsidianBrick,
        WallID.ObsidianBrickUnsafe,
        WallID.AncientObsidianBrickWall
    };

    private void IncinerateEveryone()
    {
        foreach (Player player in Main.ActivePlayers)
        {
            var wall = Framing.GetTileSafely(player.Center);
            if ((player.ZoneOverworldHeight || player.ZoneSkyHeight) && (!player.behindBackWall || DoomsdayClock.TimeLeftInRange(6)) && !safeWalls[wall.WallType])
            {
                player.AddBuff(ModContent.BuffType<SolFire>(), 20);
                continue;
            }
        }
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.buffImmune[BuffID.OnFire] || npc.buffImmune[BuffID.OnFire3] || npc.type == NPCID.OldMan || npc.type == NPCID.CultistArcherBlue || npc.aiStyle == NPCAIStyleID.LunaticDevote || NPCID.Sets.ProjectileNPC[npc.type] || npc.dontTakeDamage)
            {
                continue;
            }
            Point npcSpot = npc.Center.ToTileCoordinates();
            Tile npcTile = Framing.GetTileSafely(npc.Center);
            if (npcSpot.Y < Main.worldSurface && (npcTile.WallType == 0 || DoomsdayClock.TimeLeftInRange(6)) && !safeWalls[npcTile.WallType])
            {
                npc.AddBuff(ModContent.BuffType<SolFire>(), 10);
            }
        }
    }

    private void DiscomfortEveryone()
    {
        foreach (Player player in Main.ActivePlayers)
        {
            if ((player.ZoneOverworldHeight || player.ZoneSkyHeight) && (!player.behindBackWall || DoomsdayClock.TimeLeftInRange(2)))
            {
                player.AddBuff(ModContent.BuffType<HeatStroke>(), 5);
                continue;
            }
        }
    }
}

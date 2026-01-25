using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace SolarDoomsday;

public class BurnEverybody : ModSystem
{
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

    private void IncinerateEveryone()
    {
        foreach (Player player in Main.ActivePlayers)
        {
            if ((player.ZoneOverworldHeight || player.ZoneSkyHeight) && (!player.behindBackWall || DoomsdayClock.TimeLeftInRange(6)))
            {
                player.AddBuff(ModContent.BuffType<Buffs.SolFire>(), 20);
                continue;
            }
        }
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.buffImmune[BuffID.OnFire] || npc.buffImmune[BuffID.OnFire3] || npc.type == NPCID.OldMan || npc.type == NPCID.CultistArcherBlue || npc.aiStyle == NPCAIStyleID.LunaticDevote || NPCID.Sets.ProjectileNPC[npc.type])
            {
                continue;
            }
            Point npcSpot = npc.Center.ToTileCoordinates();
            if (npcSpot.Y < Main.worldSurface && (Main.tile[npcSpot.X, npcSpot.Y].WallType == 0 || DoomsdayClock.TimeLeftInRange(6)))
            {
                npc.AddBuff(ModContent.BuffType<Buffs.SolFire>(), 10);
            }
        }
    }

    private void DiscomfortEveryone()
    {
        foreach (Player player in Main.ActivePlayers)
        {
            if ((player.ZoneOverworldHeight || player.ZoneSkyHeight) && (!player.behindBackWall || DoomsdayClock.TimeLeftInRange(2)))
            {
                player.AddBuff(ModContent.BuffType<Buffs.HeatStroke>(), 5);
                continue;
            }
        }
    }
}

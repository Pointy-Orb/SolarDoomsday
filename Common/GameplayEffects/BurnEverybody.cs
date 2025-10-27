using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace SolarDoomsday;

public class BurnEverybody : ModSystem
{
    public override void PostUpdateTime()
    {
        if (!DoomsdayClock.TimeLeftInRange(2))
        {
            return;
        }
        if (!Main.IsItDay())
        {
            return;
        }
        foreach (Player player in Main.ActivePlayers)
        {
            if ((player.ZoneOverworldHeight || player.ZoneSkyHeight) && (!player.behindBackWall || DoomsdayClock.TimeLeftInRange(3)))
            {
                if (DoomsdayClock.TimeLeftInRange(2))
                {
                    player.AddBuff(ModContent.BuffType<Buffs.SolFire>(), 20);
                    continue;
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
            if (npcSpot.Y < Main.worldSurface && (Main.tile[npcSpot.X, npcSpot.Y].WallType == 0 && DoomsdayClock.TimeLeftInRange(6)))
            {
                npc.AddBuff(BuffID.OnFire3, 10);
            }
        }
    }
}

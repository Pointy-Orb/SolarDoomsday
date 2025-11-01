using Terraria;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace SolarDoomsday;

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

public class FireMonsters : GlobalNPC
{
    public override bool CheckDead(NPC npc)
    {
        if (!DoomsdayClock.TimeLeftInRange(3))
        {
            return true;
        }
        if (!Main.IsItDay())
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
        if (!Main.IsItDay())
        {
            return;
        }
        foreach (int key in pool.Keys)
        {
            pool[key] = 0f;
        }
        pool[NPCID.Hellbat] = 0.6f;
        pool[NPCID.LavaSlime] = 0.5f;
        pool[NPCID.FireImp] = 0.5f;
        pool[NPCID.BlazingWheel] = 0.1f;
        if (!Main.hardMode)
        {
            return;
        }
        for (int i = NPCID.HellArmoredBones; i <= NPCID.HellArmoredBonesSword; i++)
        {
            pool[i] = 0.6f;
        }
        pool[NPCID.Lavabat] = 0.6f;
        pool[NPCID.Hellbat] = 0.3f;
    }
}

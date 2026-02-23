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
    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (!DoomsdayClock.TimeLeftInRange(3))
        {
            return;
        }
        bool inScorched = spawnInfo.Player.InModBiome<Content.Biomes.ScorchedUnderground>();
        if (spawnInfo.SpawnTileY > Main.worldSurface && !inScorched)
        {
            return;
        }
        if (!Main.IsItDay() && !inScorched)
        {
            return;
        }
        if (DoomsdayManager.savedEverybody)
        {
            return;
        }
        foreach (int key in pool.Keys)
        {
            pool[key] = 0f;
        }
        if (!inScorched)
        {
            pool[NPCID.FireImp] = 0.5f;
        }
        pool[NPCID.Hellbat] = 0.6f;
        pool[NPCID.LavaSlime] = 0.5f;
        pool[NPCID.RedSlime] = 0.7f;
        pool[NPCID.BlazingWheel] = 0.1f;
        if (!Main.hardMode && inScorched)
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

public class NoTownBenefits : ModPlayer
{
    public override void PostUpdateMiscEffects()
    {
        if (Player.ZoneShadowCandle == false)
        {
            Player.ZoneShadowCandle = DoomsdayClock.Ongoing && Main.IsItDay() && DoomsdayClock.TimeLeftInRange(3, 2);
        }
    }
}

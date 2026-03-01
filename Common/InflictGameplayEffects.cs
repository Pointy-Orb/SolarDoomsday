using Terraria;
using System;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;

namespace SolarDoomsday;

public class Weather : ModSystem
{
    public override void PostUpdateTime()
    {
        if (DoomsdayClock.TimeLeftInRange(2) && DoomsdayManager.RainingAndSafe && !CreativePowerManager.Instance.GetPower<CreativePowers.FreezeRainPower>().Enabled && Main.IsItDay())
        {
            Main.StopRain();
        }
    }
}

public class FireMonsters : GlobalNPC
{
    public override void Load()
    {
        On_NPC.DespawnEncouragement_AIStyle3_Fighters_NotDiscouraged += KeepBloodZombiesAround;
        IL_NPC.VanillaAI_Inner += MakeDripplersNotDepressed;
    }

    public override void Unload()
    {
        On_NPC.DespawnEncouragement_AIStyle3_Fighters_NotDiscouraged -= KeepBloodZombiesAround;
        IL_NPC.VanillaAI_Inner -= MakeDripplersNotDepressed;
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
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
            Phase3Monsters(pool, spawnInfo);
        }
        else if ((float)(DoomsdayClock.daysLeft + 1) / (float)DoomsdayClock.DayCount <= 2f / 3f && spawnInfo.SpawnTileY < Main.worldSurface)
        {
            pool[NPCID.BloodZombie] = 0.1f;
            pool[NPCID.Drippler] = 0.1f;
        }
    }

    private void Phase3Monsters(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        bool inScorched = spawnInfo.Player.InModBiome<Content.Biomes.ScorchedUnderground>();
        if (spawnInfo.SpawnTileY > Main.worldSurface && !inScorched)
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
        if (!Main.hardMode || inScorched)
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

    private static bool KeepBloodZombiesAround(On_NPC.orig_DespawnEncouragement_AIStyle3_Fighters_NotDiscouraged orig, int npcID, Vector2 position, NPC npcInstance)
    {
        var normal = orig(npcID, position, npcInstance);
        if (npcID != NPCID.BloodZombie)
        {
            return normal;
        }
        if (!DoomsdayClock.TimeLeftInRange(3, 2))
        {
            return normal;
        }
        return true;
    }

    private static void MakeDripplersNotDepressed(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            var skipDaytimeCheck = il.DefineLabel();

            c.GotoNext(i => i.MatchLdcI4(NPCID.Drippler));
            c.GotoNext(i => i.MatchBneUn(out skipDaytimeCheck));
            c.GotoNext(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.dayTime))));

            c.EmitDelegate<Func<bool>>(() => DoomsdayClock.TimeLeftInRange(3, 2));
            c.Emit(Brtrue_S, skipDaytimeCheck);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<SolarDoomsday>(), il);
        }
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

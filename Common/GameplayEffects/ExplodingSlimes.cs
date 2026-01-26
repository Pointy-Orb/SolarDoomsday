using Terraria;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using System.Collections.Generic;

namespace SolarDoomsday;

public class ExplodingSlimes : GlobalNPC
{
    private static Dictionary<int, int> naughtyList = new();

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if ((npc.aiStyle != NPCAIStyleID.Slime && !Slimes.Contains(npc.type)) || npc.SpawnedFromStatue)
        {
            return;
        }
        if (!Main.IsItDay() || DoomsdayManager.savedEverybody || npc.position.Y / 16 > Main.worldSurface)
        {
            return;
        }
        if (!DoomsdayClock.TimeLeftInRange(3, 2) || !(Main.rand.NextBool(3) || DoomsdayClock.TimeLeftInRange(2)))
        {
            return;
        }
        if (Main.rand.Next(3) != 0)
        {
            return;
        }
        naughtyList.Add(npc.whoAmI, 5);
    }

    public override void PostAI(NPC npc)
    {
        if (!naughtyList.ContainsKey(npc.whoAmI))
        {
            return;
        }
        if (naughtyList[npc.whoAmI] > 0)
        {
            naughtyList[npc.whoAmI]--;
        }
        else
        {
            naughtyList.Remove(npc.whoAmI);
            npc.AddBuff(ModContent.BuffType<Buffs.KamikazeSlime>(), 21600);
        }
        foreach (int key in naughtyList.Keys)
        {
            if (!Main.npc[key].active)
            {
                naughtyList.Remove(key);
            }
        }
        if (!Main.IsItDay())
        {
            naughtyList.Clear();
        }
    }

    private static readonly int[] Slimes =
    {
        NPCID.GreenSlime,
        NPCID.BlueSlime,
        NPCID.RedSlime,
        NPCID.PurpleSlime,
        NPCID.YellowSlime,
        NPCID.BlackSlime,
        NPCID.IceSlime,
        NPCID.SandSlime,
        NPCID.JungleSlime,
        NPCID.BabySlime,
        NPCID.Pinky,
        NPCID.Slimeling
    };
}

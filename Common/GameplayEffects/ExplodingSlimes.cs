using System.Collections.Generic;
using System.Linq;
using SolarDoomsday.Content.Buffs;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SolarDoomsday;

public class ExplodingSlimes : GlobalNPC
{
    private static Dictionary<int, int> naughtyList = new();

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (npc.aiStyle != NPCAIStyleID.Slime || npc.SpawnedFromStatue)
        {
            return;
        }
        if (!Main.IsItDay() || DoomsdayManager.savedEverybody)
        {
            return;
        }
        var tileY = npc.position.Y / 16;
        if ((tileY > Main.worldSurface && !DoomsdayClock.TimeLeftInRange(3)) || tileY > Main.rockLayer)
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
        if (naughtyList.ContainsKey(npc.whoAmI))
        {
            naughtyList[npc.whoAmI] = 5;
        }
        else
        {
            naughtyList.Add(npc.whoAmI, 5);
        }
    }

    public override void PostAI(NPC npc)
    {
        if (!naughtyList.ContainsKey(npc.whoAmI))
        {
            return;
        }
        if (!npc.active || npc.aiStyle != NPCAIStyleID.Slime)
        {
            naughtyList.Remove(npc.whoAmI);
            return;
        }
        var tileY = npc.position.Y / 16;
        if ((tileY > Main.worldSurface && !DoomsdayClock.TimeLeftInRange(3)) || tileY > Main.rockLayer)
        {
            naughtyList.Remove(npc.whoAmI);
            return;
        }

        if (naughtyList[npc.whoAmI] > 0)
        {
            naughtyList[npc.whoAmI]--;
        }
        else
        {
            naughtyList.Remove(npc.whoAmI);
            npc.AddBuff(ModContent.BuffType<KamikazeSlime>(), 21600);
        }
        if (!Main.IsItDay())
        {
            naughtyList.Clear();
        }
    }
}

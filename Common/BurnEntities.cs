using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using SolarDoomsday.Content.Buffs;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace SolarDoomsday;

public class BurnPlayers : ModPlayer
{
    public override void PreUpdateBuffs()
    {
        bool onFire = false;
        var worldPos = Player.position.ToTileCoordinates();
        for (int i = worldPos.X - 1; i <= worldPos.X + (Player.width / 16) + 1; i++)
        {
            if (onFire)
            {
                break;
            }
            for (int j = worldPos.Y - 1; j <= worldPos.Y + (Player.height / 16) + 1; j++)
            {
                if (!WorldGen.InWorld(i, j))
                {
                    continue;
                }
                onFire |= Main.tile[i, j].Get<FireTileData>().fireAmount > 0;
            }
        }

        if (!onFire)
        {
            return;
        }
        if (Player.lavaImmune)
        {
            return;
        }
        if (Player.lavaTime > 0)
        {
            Player.lavaTime -= 2;
            return;
        }
        BurnPlayer();
    }

    private void BurnPlayer()
    {
        var hurtAmount = 40;
        if (Player.lavaRose)
        {
            hurtAmount -= 20;
        }
        if (Player.ashWoodBonus)
        {
            hurtAmount -= 20;
        }
        if (Player.buffImmune[BuffID.OnFire])
        {
            hurtAmount -= 10;
        }
        if (hurtAmount <= 0)
        {
            return;
        }
        Player.AddBuff(BuffID.OnFire, 420);
        Player.Hurt(PlayerDeathReason.ByOther(8), hurtAmount, 0, false, false, ImmunityCooldownID.Lava, false);
        Player.GetModPlayer<SolFirePlayer>().touchingFireBlock = true;
    }
}

public class BurnNPCs : GlobalNPC
{
    public override void PostAI(NPC npc)
    {
        if (npc.type == NPCID.OldMan || npc.immortal || npc.dontTakeDamage || npc.lavaImmune || npc.immune[255] != 0)
        {
            return;
        }
        bool onFire = false;
        var worldPos = npc.position.ToTileCoordinates();
        for (int i = worldPos.X - 1; i <= worldPos.X + (npc.width / 16) + 1; i++)
        {
            if (onFire)
            {
                break;
            }
            for (int j = worldPos.Y - 1; j <= worldPos.Y + (npc.height / 16) + 1; j++)
            {
                if (!WorldGen.InWorld(i, j))
                {
                    continue;
                }
                onFire |= Main.tile[i, j].Get<FireTileData>().fireAmount > 0;
            }
        }
        if (!onFire)
        {
            return;
        }
        var hitInfo = new NPC.HitInfo();
        npc.immune[255] = 30;
        npc.AddBuff(BuffID.OnFire, 420);
        npc.SimpleStrikeNPC(30, 0);
        npc.GetGlobalNPC<SolFireNPC>().touchingFireBlock = true;
    }
}

using Terraria;
using SolarDoomsday.Content.Tiles;
using Terraria.DataStructures;
using Terraria.ModLoader;
using SolarDoomsday.Content.Buffs;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace SolarDoomsday;

public class BurnPlayers : ModPlayer
{
    public override void Load()
    {
        On_Player.ApplyTouchDamage += GetBurning;
        On_Collision.CanTileHurt += FireHurts;
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

    private static void GetBurning(On_Player.orig_ApplyTouchDamage orig, Player self, int tileId, int x, int y)
    {
        orig(self, tileId, x, y);
        if (tileId != ModContent.TileType<Hornfels>() && Main.tile[x, y].Get<FireTileData>().fireAmount <= 0)
        {
            return;
        }
        if (self.lavaImmune)
        {
            return;
        }
        if (self.lavaTime > 0)
        {
            self.lavaTime -= 2;
            return;
        }
        self.GetModPlayer<BurnPlayers>().BurnPlayer();
    }

    private static bool FireHurts(On_Collision.orig_CanTileHurt orig, ushort type, int i, int j, Player player)
    {
        if (orig(type, i, j, player))
        {
            return true;
        }
        Tile tile = Main.tile[i, j];
        if (tile.Get<FireTileData>().fireAmount > 0)
        {
            return true;
        }
        return type == ModContent.TileType<Hornfels>();
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

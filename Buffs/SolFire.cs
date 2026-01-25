using Terraria;
using Microsoft.Xna.Framework;
using System;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;

namespace SolarDoomsday.Buffs;

public class SolFire : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public static int DebuffDamage => (int)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, 1, 50);

    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.GetGlobalNPC<SolFireNPC>().onSolFire = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        if (player.HasBuff(BuffID.ObsidianSkin))
        {
            player.DelBuff(buffIndex);
        }
        player.GetModPlayer<SolFirePlayer>().onSolFire = true;
    }
}

public class SolFirePlayer : ModPlayer
{
    public bool onSolFire = false;

    public override void ResetEffects()
    {
        onSolFire = false;
    }

    public override void PostUpdateBuffs()
    {
        Player.buffImmune[ModContent.BuffType<SolFire>()] = Player.lavaImmune || Player.wet;
    }

    public override void UpdateBadLifeRegen()
    {
        if (!onSolFire)
        {
            return;
        }
        if (Player.lavaTime > 0)
        {
            Player.lavaTime -= Int32.Min(2, Player.lavaTime);
            if (Player.lavaTime > 0)
            {
                return;
            }
        }
        if (Player.lifeRegen > 0)
        {
            Player.lifeRegen = 0;
        }
        Player.lifeRegenTime = 0;
        int dot = SolFire.DebuffDamage;
        if (Player.behindBackWall)
        {
            dot /= 2;
        }
        Player.lifeRegen -= dot;
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
    {
        if (!onSolFire)
        {
            return;
        }
        if (drawInfo.shadow == 0f && Player.lavaTime < 2)
        {
            float scale = Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, 0.5f, 3f);
            if (Player.behindBackWall)
            {
                scale /= 2;
            }
            Dust dust6 = Dust.NewDustDirect(new Vector2(drawInfo.Position.X - 2f, drawInfo.Position.Y - 2f), Player.width + 4, Player.height + 4, 6, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default(Color), scale);
            dust6.noGravity = true;
            dust6.velocity *= 1.8f;
            dust6.velocity.Y -= 0.75f;
            drawInfo.DustCache.Add(dust6.dustIndex);
        }
        r = Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, r, 1f);
        g = Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, g, g * 0.5f);
        b = Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, b, b * 0.4f);
    }
}

public class SolFireNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public bool onSolFire = false;

    public override void ResetEffects(NPC npc)
    {
        npc.GetGlobalNPC<SolFireNPC>().onSolFire = false;
        npc.buffImmune[ModContent.BuffType<SolFire>()] = npc.buffImmune[BuffID.OnFire] || npc.lavaImmune || npc.wet;
    }

    public override void AI(NPC npc)
    {
        if (!npc.GetGlobalNPC<SolFireNPC>().onSolFire)
        {
            return;
        }
        int dot = SolFire.DebuffDamage;
        Main.NewText(dot);
        var point = npc.Center.ToTileCoordinates();
        if (Main.tile[point.X, point.Y].WallType > 0)
        {
            dot /= 2;
        }
        npc.lifeRegen -= dot;
    }

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        if (!npc.GetGlobalNPC<SolFireNPC>().onSolFire)
        {
            return;
        }
        float scale = Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, 0.5f, 3f);
        var point = npc.Center.ToTileCoordinates();
        if (Main.tile[point.X, point.Y].WallType > 0)
        {
            scale /= 2;
        }
        Dust dust6 = Dust.NewDustDirect(new Vector2(npc.position.X - 2f, npc.position.Y - 2f), npc.width + 4, npc.height + 4, 6, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default(Color), scale);
        dust6.noGravity = true;
        dust6.velocity *= 1.8f;
        dust6.velocity.Y -= 0.75f;
        drawColor.R = (byte)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, drawColor.R, byte.MaxValue);
        drawColor.G = (byte)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, drawColor.G, byte.MaxValue);
        drawColor.B = (byte)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, drawColor.B, byte.MaxValue);
    }
}

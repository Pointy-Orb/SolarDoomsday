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

    public override void Update(NPC npc, ref int buffIndex)
    {
        base.Update(npc, ref buffIndex);
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
        Player.lifeRegen -= 40;
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
    {
        if (!onSolFire)
        {
            return;
        }
        if (drawInfo.shadow == 0f && Player.lavaTime < 2)
        {
            Dust dust6 = Dust.NewDustDirect(new Vector2(drawInfo.Position.X - 2f, drawInfo.Position.Y - 2f), Player.width + 4, Player.height + 4, 6, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default(Color), 3f);
            dust6.noGravity = true;
            dust6.velocity *= 1.8f;
            dust6.velocity.Y -= 0.75f;
            drawInfo.DustCache.Add(dust6.dustIndex);
        }
        r = 1f;
        g *= 0.5f;
        b *= 0.4f;
    }
}

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Buffs;

public class HeatStroke : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetModPlayer<HeatStrokePlayer>().heatStroke = true;
    }
}

public class HeatStrokePlayer : ModPlayer
{
    public bool heatStroke = false;

    public override void ResetEffects()
    {
        heatStroke = false;
    }

    public override void UpdateDead()
    {
        heatStroke = false;
    }

    public override void PostUpdateBuffs()
    {
        Player.buffImmune[ModContent.BuffType<HeatStroke>()] = Player.buffImmune[BuffID.OnFire] || Player.wet;
        if (!heatStroke)
        {
            return;
        }
        Player.endurance -= 0.1f;
        Player.GetDamage(DamageClass.Generic) *= 0.9f;
    }
}

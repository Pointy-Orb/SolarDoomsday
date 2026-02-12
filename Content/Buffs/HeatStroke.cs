using Terraria;
using System;
using static Terraria.GameContent.PlayerEyeHelper;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Buffs;

public class HeatStroke : ModBuff
{
    private static LocalizedText TooltipText;

    public static float Modifier => Utils.Remap(DoomsdayClock.PercentTimeLeft(), 2f / 3f, 1f / 3f, 0.05f, 0.2f);

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;

        TooltipText = this.GetLocalization("Description");
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetModPlayer<HeatStrokePlayer>().heatStroke = true;
        player.buffImmune[BuffID.Chilled] = true;
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        var modifier = Modifier;
        if (Main.LocalPlayer.behindBackWall)
        {
            modifier /= 2;
        }
        tip = TooltipText.Format(MathF.Round(modifier * 100, 2));
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
        Player.buffImmune[ModContent.BuffType<HeatStroke>()] = Player.buffImmune[BuffID.OnFire] || (Player.wet && !Player.lavaWet) || Main.raining;
        if (!heatStroke)
        {
            return;
        }
        var modifier = HeatStroke.Modifier;
        if (Player.behindBackWall)
        {
            modifier /= 2;
        }
        Player.eyeHelper.SwitchToState(EyeState.IsPoisoned);
        Player.endurance -= modifier;
        Player.GetDamage(DamageClass.Generic) *= (1f - modifier);
    }
}

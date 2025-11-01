using Terraria;
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday;

public class Effects : ModSystem
{
    public static Asset<Texture2D> deadSun;

    public override void Load()
    {
        deadSun = ModContent.Request<Texture2D>("SolarDoomsday/Assets/Sun");
        IL_Main.DrawSunAndMoon += IL_BiggerSun;

        On_Main.DrawSunAndMoon += ControlSunColor;
    }

    public override void PostUpdateEverything()
    {
        if (DoomsdayClock.TimeLeftInRange(3, 2) && (Main.LocalPlayer.ZoneOverworldHeight || Main.LocalPlayer.ZoneSkyHeight || Main.LocalPlayer.ZoneDirtLayerHeight))
        {
            Main.LocalPlayer.ManageSpecialBiomeVisuals("HeatDistortion", Main.UseHeatDistortion && Main.IsItDay());
            Filters.Scene["HeatDistortion"].GetShader().UseIntensity(5f - DoomsdayClock.PercentTimeLeft() * 4);
        }
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        //TODO: Time it better so that the transition from scary flash to not scary flash is more gradual
        if (DoomsdayManager.shaderTime > 0)
        {
            return;
        }
        if (DoomsdayManager.sunDied)
        {
            backgroundColor *= 0f;
            tileColor *= 0.05f;
            return;
        }
        var defaultColor = backgroundColor;
        backgroundColor = Color.Lerp(backgroundColor, Color.Orange.MultiplyRGB(backgroundColor), Utils.GetLerpValue(DoomsdayClock.DayCount, DoomsdayClock.DayCount / 2, DoomsdayClock.daysLeft, true));
        if (!Main.IsItDay() && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19f)
        {
            return;
        }
        tileColor = Color.Lerp(tileColor, Color.OrangeRed.MultiplyRGB(tileColor), Utils.GetLerpValue(DoomsdayClock.DayCount, 0, DoomsdayClock.daysLeft, true));
        if (DoomsdayClock.TimeLeftInRange(2))
        {
            backgroundColor = defaultColor;
            backgroundColor = Color.Lerp(Color.Orange.MultiplyRGB(backgroundColor), Color.DarkRed.MultiplyRGB(backgroundColor), Utils.GetLerpValue(DoomsdayClock.DayCount / 2, 0, DoomsdayClock.daysLeft, true));
        }
        if (DoomsdayClock.LastDay)
        {
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() <= DoomsdayClock.doomsdayTime / 2)
            {
                tileColor = Color.Lerp(tileColor, Color.Red, Utils.GetLerpValue(4.5f, DoomsdayClock.doomsdayTime / 2, Utils.GetDayTimeAs24FloatStartingFromMidnight()));
            }
            else
            {
                tileColor = Color.Lerp(Color.Red, Color.Red * 0.05f, Utils.GetLerpValue(DoomsdayClock.doomsdayTime / 2, DoomsdayClock.doomsdayTime, Utils.GetDayTimeAs24FloatStartingFromMidnight()));
            }
            backgroundColor = Color.Lerp(backgroundColor, backgroundColor * 0f, Utils.GetLerpValue(4.5f, DoomsdayClock.doomsdayTime, Utils.GetDayTimeAs24FloatStartingFromMidnight()));
        }
    }

    private static void IL_BiggerSun(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            int textureIndex = 0;
            int scaleIndex = 0;
            var dwarfSetSkip = il.DefineLabel();

            c.GotoNext(MoveType.After, i => i.MatchStloc(out textureIndex));
            c.EmitDelegate<Func<bool>>(() =>
            {
                return DoomsdayManager.sunDied;
            });
            c.Emit(Brfalse_S, dwarfSetSkip);
            c.EmitDelegate<Func<Texture2D>>(() =>
            {
                return deadSun.Value;
            });
            c.Emit(Stloc, scaleIndex);
            c.MarkLabel(dwarfSetSkip);

            //Change the sun size depending on the current day and hour
            c.GotoNext(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.ForcedMinimumZoom))));
            c.GotoNext(MoveType.After, i => i.MatchStloc(out scaleIndex));
            c.Emit(Ldloc, scaleIndex);
            c.EmitDelegate<Func<float>>(() =>
            {
                if (Main.gameMenu)
                {
                    return 1f;
                }
                if (DoomsdayManager.sunDied)
                {
                    return 0.1f;
                }
                if (DoomsdayClock.LastDay)
                {
                    return Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 4.5f, DoomsdayClock.doomsdayTime, 25, 100);
                }
                return Utils.Remap(DoomsdayClock.daysLeft, DoomsdayClock.DayCount, 0, 1, 25);
            });
            c.Emit(Mul);
            c.Emit(Stloc, scaleIndex);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<SolarDoomsday>(), il);
        }
    }

    private static void ControlSunColor(On_Main.orig_DrawSunAndMoon orig, Main self, Main.SceneArea sceneArea, Color moonColor, Color sunColor, float tempMushroomInfluence)
    {
        var realSunColor = Color.Lerp(sunColor, Color.DarkRed, Utils.GetLerpValue(DoomsdayClock.DayCount, 0, DoomsdayClock.daysLeft, true));
        if (DoomsdayManager.sunDied)
        {
            realSunColor = Color.White;
        }
        if (Main.gameMenu)
        {
            realSunColor = sunColor;
        }
        orig(self, sceneArea, moonColor, realSunColor, tempMushroomInfluence);
    }
}

public class BigScaryFlashShader : ScreenShaderData
{
    public BigScaryFlashShader(string passName) : base(passName)
    { }

    public override void Update(GameTime gameTime)
    {
        UseColor(3000, 500, 500);
    }
}

public class MakeAtmosphereHellish : ModSceneEffect
{
    public override int Music
    {
        get
        {
            if (DoomsdayManager.sunDied)
            {
                return Main.dayTime ? MusicID.SpaceDay : MusicID.Space;
            }
            if (DoomsdayClock.LastDay)
            {
                return MusicID.Boss2;
            }
            if (DoomsdayClock.PercentTimeLeft() <= (1f / 3f))
            {
                return !Main.swapMusic == Main.drunkWorld && !Main.remixWorld ? MusicID.OtherworldlyUnderworld : MusicID.Hell;
            }
            else if (DoomsdayClock.PercentTimeLeft() <= (2f / 3f))
            {
                return !Main.swapMusic == Main.drunkWorld && !Main.remixWorld ? MusicID.OtherworldlyEerie : MusicID.Eerie;
            }
            return -1;
        }
    }

    public override SceneEffectPriority Priority => DoomsdayClock.LastDay ? SceneEffectPriority.Event : SceneEffectPriority.BiomeLow;

    public override void SpecialVisuals(Player player, bool isActive)
    {
    }

    public override bool IsSceneEffectActive(Player player)
    {
        return (player.ZoneOverworldHeight || player.ZoneSkyHeight) && (Main.IsItDay() || DoomsdayManager.sunDied);
    }
}

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using SolarDoomsday.Content.Biomes;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Light;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Mono.Cecil.Cil.OpCodes;

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

    public override void Unload()
    {
        IL_Main.DrawSunAndMoon -= IL_BiggerSun;

        On_Main.DrawSunAndMoon -= ControlSunColor;
    }

    public override void PostUpdateTime()
    {
        if (DoomsdayManager.savedEverybody)
        {
            return;
        }
        for (
            int i = Main.maxClouds - 1;
            i >= (int)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1, 0.5f, Main.maxClouds - 1, 0);
            i--
        )
        {
            Main.cloud[i].active = false;
            if (i < Main.numClouds)
            {
                Main.numClouds = i;
            }
        }
        if (Main.LocalPlayer.InModBiome<ScorchedUnderground>())
        {
            Main.LocalPlayer.ManageSpecialBiomeVisuals("HeatDistortion", Main.UseHeatDistortion);
            Filters.Scene["HeatDistortion"].GetShader().UseIntensity(3);
        }
        if (
            !(
                DoomsdayClock.TimeLeftInRange(3, 2)
                && (Main.LocalPlayer.ZoneOverworldHeight || Main.LocalPlayer.ZoneSkyHeight)
            )
        )
        {
            return;
        }
        Main.cloudBGActive = 0f;
        Main.LocalPlayer.ManageSpecialBiomeVisuals(
            "HeatDistortion",
            Main.UseHeatDistortion && Main.IsItDay()
        );
        Filters
            .Scene["HeatDistortion"]
            .GetShader()
            .UseIntensity(
                (5f - DoomsdayClock.PercentTimeLeft() * 4)
                    / (Main.LocalPlayer.behindBackWall ? 2 : 1)
            );
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }
        var whichBackground = BackgroundCorrection_Inner();
        if (whichBackground < 0)
        {
            return;
        }
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            BackgroundCorrectionSinglePlayer(whichBackground);
            return;
        }
        if (_multiplayerBackgroundInterval > 0)
        {
            _multiplayerBackgroundInterval--;
            return;
        }
        _multiplayerBackgroundInterval = _multiplayerBackgroundIntervalMax;
        SolarDoomsday.RandomizeBackground(whichBackground, Main.myPlayer);
    }

    private int _multiplayerBackgroundInterval = 0;
    private const int _multiplayerBackgroundIntervalMax = 120;

    private int BackgroundCorrection_Inner()
    {
        switch (Main.bgStyle)
        {
            case 0:
                if (badBackgrounds.Contains(WorldGen.treeBG1))
                {
                    return 0;
                }
                break;
            case 10:
                if (badBackgrounds.Contains(WorldGen.treeBG2))
                {
                    return 1;
                }
                break;
            case 11:
                if (badBackgrounds.Contains(WorldGen.treeBG3))
                {
                    return 2;
                }
                break;
            case 12:
                if (badBackgrounds.Contains(WorldGen.treeBG4))
                {
                    return 3;
                }
                break;
        }
        return -1;
    }

    private void BackgroundCorrectionSinglePlayer(int which)
    {
        BackgroundCorrectionServer(which, Main.myPlayer);
    }

    public static int BackgroundCorrectionServer(int which, int player)
    {
        int newType;
        int attempts = 0;
        switch (which)
        {
            default:
            case 0:
                while (badBackgrounds.Contains(WorldGen.treeBG1) && attempts < 100)
                {
                    WorldGen.RandomizeBackgroundBasedOnPlayer(Main.rand, Main.player[player]);
                    attempts++;
                }
                newType = WorldGen.treeBG1;
                break;
            case 1:
                while (badBackgrounds.Contains(WorldGen.treeBG2) && attempts < 100)
                {
                    WorldGen.RandomizeBackgroundBasedOnPlayer(Main.rand, Main.player[player]);
                    attempts++;
                }
                newType = WorldGen.treeBG2;
                break;
            case 2:
                while (badBackgrounds.Contains(WorldGen.treeBG3) && attempts < 100)
                {
                    WorldGen.RandomizeBackgroundBasedOnPlayer(Main.rand, Main.player[player]);
                    attempts++;
                }
                newType = WorldGen.treeBG3;
                break;
            case 3:
                while (badBackgrounds.Contains(WorldGen.treeBG4) && attempts < 100)
                {
                    WorldGen.RandomizeBackgroundBasedOnPlayer(Main.rand, Main.player[player]);
                    attempts++;
                }
                newType = WorldGen.treeBG4;
                break;
        }
        return newType;
    }

    public static void SetBackground(int which, int type)
    {
        switch (which)
        {
            case 0:
                WorldGen.treeBG1 = type;
                break;
            case 1:
                WorldGen.treeBG2 = type;
                break;
            case 2:
                WorldGen.treeBG3 = type;
                break;
            case 3:
                WorldGen.treeBG4 = type;
                break;
        }
    }

    private static int[] badBackgrounds = new int[] { 10, 9, 8, 3, 6 };

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (DoomsdayManager.savedEverybody)
        {
            return;
        }
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
        backgroundColor = Color.Lerp(
            backgroundColor,
            Color.OrangeRed.MultiplyRGB(backgroundColor),
            Utils.GetLerpValue(
                DoomsdayClock.DayCount,
                DoomsdayClock.DayCount / 2,
                DoomsdayClock.daysLeft,
                true
            )
        );
        if (!Main.IsItDay() && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19f)
        {
            return;
        }
        if (Lighting.Mode != LightMode.Retro)
        {
            tileColor = Color.Lerp(
                tileColor,
                Color.OrangeRed.MultiplyRGB(tileColor),
                Utils.GetLerpValue(DoomsdayClock.DayCount, 0, DoomsdayClock.daysLeft, true)
            );
        }
        if (DoomsdayClock.TimeLeftInRange(2))
        {
            backgroundColor = defaultColor;
            backgroundColor = Color.Lerp(
                Color.OrangeRed.MultiplyRGB(backgroundColor),
                Color.DarkRed.MultiplyRGB(backgroundColor),
                Utils.GetLerpValue(DoomsdayClock.DayCount / 2, 0, DoomsdayClock.daysLeft, true)
            );
        }
        if (DoomsdayClock.LastDay)
        {
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() <= DoomsdayClock.doomsdayTime / 2)
            {
                tileColor = Color.Lerp(
                    tileColor,
                    Color.Red,
                    Utils.GetLerpValue(
                        4.5f,
                        DoomsdayClock.doomsdayTime / 2,
                        Utils.GetDayTimeAs24FloatStartingFromMidnight()
                    )
                );
            }
            else
            {
                tileColor = Color.Lerp(
                    Color.Red,
                    Color.Red * 0.05f,
                    Utils.GetLerpValue(
                        DoomsdayClock.doomsdayTime / 2,
                        DoomsdayClock.doomsdayTime,
                        Utils.GetDayTimeAs24FloatStartingFromMidnight()
                    )
                );
            }
            backgroundColor = Color.Lerp(
                backgroundColor,
                backgroundColor * 0f,
                Utils.GetLerpValue(
                    4.5f,
                    DoomsdayClock.doomsdayTime,
                    Utils.GetDayTimeAs24FloatStartingFromMidnight()
                )
            );
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
                if (DoomsdayManager.savedEverybody)
                {
                    return 1f;
                }
                if (DoomsdayManager.sunDied)
                {
                    return 0.1f;
                }
                if (DoomsdayClock.LastDay)
                {
                    return Utils.Remap(
                        Utils.GetDayTimeAs24FloatStartingFromMidnight(),
                        4.5f,
                        DoomsdayClock.doomsdayTime,
                        25,
                        100
                    );
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

    private static void ControlSunColor(
        On_Main.orig_DrawSunAndMoon orig,
        Main self,
        Main.SceneArea sceneArea,
        Color moonColor,
        Color sunColor,
        float tempMushroomInfluence
    )
    {
        var realSunColor = Color.Lerp(
            sunColor,
            Color.DarkRed,
            Utils.GetLerpValue(DoomsdayClock.DayCount, 0, DoomsdayClock.daysLeft, true)
        );
        if (DoomsdayManager.sunDied)
        {
            realSunColor = Color.White;
        }
        if (Main.gameMenu || DoomsdayManager.savedEverybody)
        {
            realSunColor = sunColor;
        }
        orig(self, sceneArea, moonColor, realSunColor, tempMushroomInfluence);
    }
}

public class BigScaryFlashShader : ScreenShaderData
{
    public BigScaryFlashShader(string passName)
        : base(passName) { }

    public override void Update(GameTime gameTime)
    {
        UseColor(3000, 500, 500);
    }
}

public class MakeAtmosphereHellish : ModSceneEffect
{
    public override void Load()
    {
        On_Main.UpdateBGVisibility_FrontLayer += RemoveFrontLayerOverTime;
        On_Main.DrawBG_ModifyBGFarBackLayerAlpha += RemoveOceanBackground;
    }

    public override void Unload()
    {
        On_Main.UpdateBGVisibility_FrontLayer -= RemoveFrontLayerOverTime;
        On_Main.DrawBG_ModifyBGFarBackLayerAlpha -= RemoveOceanBackground;
    }

    public override int Music
    {
        get
        {
            if (DoomsdayClock.LastDay || DoomsdayManager.novaTime > 0)
            {
                return !Main.swapMusic == Main.drunkWorld && !Main.remixWorld
                    ? MusicID.OtherworldlyTowers
                    : MusicID.Boss2;
            }
            if (DoomsdayManager.sunDied)
            {
                return Main.dayTime ? MusicID.SpaceDay : MusicID.Space;
            }
            if (DoomsdayClock.PercentTimeLeft() <= (1f / 3f))
            {
                return !Main.swapMusic == Main.drunkWorld && !Main.remixWorld
                    ? MusicID.OtherworldlyEerie
                    : MusicID.Hell;
            }
            else if (DoomsdayClock.TimeLeftInRange(3, 2) && !DoomsdayManager.RainingAndSafe)
            {
                return !Main.swapMusic == Main.drunkWorld && !Main.remixWorld
                    ? MusicID.OtherworldlyUnderworld
                    : MusicID.Eerie;
            }
            return -1;
        }
    }

    public override SceneEffectPriority Priority =>
        DoomsdayClock.LastDay ? SceneEffectPriority.Event : SceneEffectPriority.BiomeLow;

    public override void SpecialVisuals(Player player, bool isActive) { }

    public override bool IsSceneEffectActive(Player player)
    {
        return (
                player.ZoneOverworldHeight
                || player.ZoneSkyHeight
                || (player.ZoneDirtLayerHeight && DoomsdayClock.LastDay)
            )
            && (Main.IsItDay() || DoomsdayManager.sunDied)
            && !DoomsdayManager.savedEverybody;
    }

    private static void RemoveFrontLayerOverTime(
        On_Main.orig_UpdateBGVisibility_FrontLayer orig,
        Main self,
        int? targetBiomeOverride,
        float? transitionAmountOverride
    )
    {
        orig(self, targetBiomeOverride, transitionAmountOverride);
        if (!Main.LocalPlayer.ZonePurity || DoomsdayManager.savedEverybody)
        {
            return;
        }
        for (int i = 0; i < Main.bgAlphaFrontLayer.Length; i++)
        {
            Main.bgAlphaFrontLayer[i] *= Utils.Remap(
                DoomsdayClock.PercentTimeLeft(),
                (2f / 3f),
                (1f / 3f),
                1,
                0
            );
        }
    }

    private static void RemoveOceanBackground(
        On_Main.orig_DrawBG_ModifyBGFarBackLayerAlpha orig,
        Main self,
        int desiredBG,
        int? desiredBG2 = null,
        float? transitionAmountOverride = null
    )
    {
        orig(self, desiredBG, desiredBG2, transitionAmountOverride);
        if (!Main.LocalPlayer.ZoneBeach || DoomsdayManager.savedEverybody)
        {
            return;
        }
        for (int i = 0; i < Main.bgAlphaFarBackLayer.Length; i++)
        {
            Main.bgAlphaFarBackLayer[i] *= Utils.Remap(
                DoomsdayClock.PercentTimeLeft(),
                (2f / 3f),
                (1f / 3f),
                1,
                0
            );
        }
    }
}

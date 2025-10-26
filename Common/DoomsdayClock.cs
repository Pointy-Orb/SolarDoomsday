using Terraria;
using Terraria.Localization;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.OpCodes;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace SolarDoomsday;

public class DoomsdayClock : ModSystem
{
    public static int DayCount { get; private set; } = 30;

    public static int daysLeft = DayCount;

    public static DayText dayText;

    public const float doomsdayTime = 16.5f;


    public static bool LastDay => daysLeft == 1;

    public override void Load()
    {
        IL_Main.DrawSunAndMoon += IL_BiggerSun;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["daysLeft"] = daysLeft;
        if (DayCount != 30)
        {
            tag["DayCount"] = DayCount;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.ContainsKey("DayCount"))
        {
            DayCount = tag.GetAsInt("DayCount");
        }
        if (tag.ContainsKey("daysLeft"))
        {
            daysLeft = tag.GetAsInt("daysLeft");
        }
        else
        {
            daysLeft = DayCount;
        }
    }

    public override void ClearWorld()
    {
        DayCount = 30;
        daysLeft = DayCount;
        cycleActive = true;
    }

    bool wasDay = true;
    public static bool cycleActive = true;

    public override void PostUpdateTime()
    {
        if (!cycleActive)
        {
            return;
        }
        if (Main.dayTime && !wasDay)
        {
            daysLeft--;
        }
        wasDay = Main.dayTime;
        if (LastDay && Utils.GetDayTimeAs24FloatStartingFromMidnight() >= doomsdayTime)
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                npc.StrikeInstantKill();
            }
            SoundEngine.PlaySound(new SoundStyle("SolarDoomsday/Assets/sunsplosion"));
            WorldGen.clearWorld();
            Main.worldSurface = Main.UnderworldLayer - 10;
            Main.rockLayer = Main.UnderworldLayer - 6;
            Main.remixWorld = true;
            cycleActive = false;
            foreach (Player player in Main.ActivePlayers)
            {
                var deathReason = new PlayerDeathReason();
                int message = Main.rand.Next(0, 3);
                deathReason.CustomReason = Language.GetText($"SolarDoomsday.DeathReasons.Sun{message}").WithFormatArgs(player.name).ToNetworkText();
                player.KillMe(deathReason, 999999, 0);
            }
        }
    }

    public override void PostUpdateEverything()
    {
        if (PercentTimeLeft() <= 0.5f && (Main.LocalPlayer.ZoneOverworldHeight || Main.LocalPlayer.ZoneSkyHeight || Main.LocalPlayer.ZoneDirtLayerHeight))
        {
            Main.LocalPlayer.ManageSpecialBiomeVisuals("HeatDistortion", Main.UseHeatDistortion && Main.IsItDay());
            Filters.Scene["HeatDistortion"].GetShader().UseIntensity(5f - PercentTimeLeft() * 2);
        }
    }

    public static float PercentTimeLeft()
    {
        return (float)daysLeft / (float)DayCount;
    }

    public static bool TimeLeftInRange(int denominator, int numerator = 1)
    {
        return PercentTimeLeft() <= ((float)numerator / (float)denominator);
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!Main.IsItDay() && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19f)
        {
            return;
        }
        tileColor = Color.Lerp(tileColor, new Color(222, 123, 20).MultiplyRGB(tileColor), Utils.GetLerpValue(DayCount, 0, daysLeft, true));
        backgroundColor = Color.Lerp(backgroundColor, new Color(242, 123, 20).MultiplyRGB(tileColor), Utils.GetLerpValue(DayCount, 0, daysLeft, true));
        if (LastDay)
        {
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() <= doomsdayTime / 2)
            {
                tileColor = Color.Lerp(tileColor, Color.Red, Utils.GetLerpValue(4.5f, doomsdayTime / 2, Utils.GetDayTimeAs24FloatStartingFromMidnight()));
            }
            else
            {
                tileColor = Color.Lerp(Color.Red, Color.Red * 0.05f, Utils.GetLerpValue(doomsdayTime / 2, doomsdayTime, Utils.GetDayTimeAs24FloatStartingFromMidnight()));
            }
            backgroundColor = Color.Lerp(backgroundColor, backgroundColor * 0f, Utils.GetLerpValue(4.5f, doomsdayTime, Utils.GetDayTimeAs24FloatStartingFromMidnight()));
        }
    }

    private static void IL_BiggerSun(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            int scaleIndex = 0;

            //Change the sun size depending on the current day and hour
            c.GotoNext(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.ForcedMinimumZoom))));
            c.GotoNext(MoveType.After, i => i.MatchStloc(out scaleIndex));
            c.Emit(Ldloc, scaleIndex);
            c.EmitDelegate<Func<float>>(() =>
            {
                if (DoomsdayClock.LastDay)
                {
                    return Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 4.5f, DoomsdayClock.doomsdayTime, 25, 100);
                }
                return Utils.Remap(daysLeft, DayCount, 0, 1, 25);
            });
            c.Emit(Mul);
            c.Emit(Stloc, scaleIndex);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<SolarDoomsday>(), il);
        }
    }
}

public class SetCounter : ModCommand
{
    public override CommandType Type => CommandType.World;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (Int32.TryParse(args[0], out int set))
        {
            DoomsdayClock.daysLeft = set;
            Main.NewText("Set counter to " + set);
        }
    }

    public override string Command => "setcounter";
}

public class MakeAtmosphereHellish : ModSceneEffect
{
    public override int Music
    {
        get
        {
            if (DoomsdayClock.LastDay)
            {
                return MusicID.Boss2;
            }
            if (DoomsdayClock.PercentTimeLeft() <= (1f / 3f))
            {
                return MusicID.Hell;
            }
            else if (DoomsdayClock.PercentTimeLeft() <= (2f / 3f))
            {
                return MusicID.Jungle;
            }
            return -1;
        }
    }

    public override SceneEffectPriority Priority => DoomsdayClock.LastDay ? SceneEffectPriority.Event : SceneEffectPriority.BiomeLow;

    public override bool IsSceneEffectActive(Player player)
    {
        return (player.ZoneOverworldHeight || player.ZoneSkyHeight) && Main.IsItDay();
    }
}

public class BigScaryFlashShader : ScreenShaderData
{
    public BigScaryFlashShader(string passName) : base(passName)
    { }

    public override void Update(GameTime gameTime)
    {
        UseColor(3000, 2000, 1000);
    }
}

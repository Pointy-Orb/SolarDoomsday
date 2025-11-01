using Terraria;
using System;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SolarDoomsday;

public class DoomsdayClock : ModSystem
{
    public static int DayCount { get; private set; } = 30;

    public static int daysLeft = DayCount;

    public static DayText dayText;

    public const float doomsdayTime = 16.5f;


    public static bool LastDay => daysLeft == 1 && DoomsdayManager.worldEndChoice != DoomsdayOptions.Stagnation && !DoomsdayManager.sunDied;

    public static void SetDayCount(int proposal)
    {
        DayCount = proposal;
        daysLeft = proposal;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["daysLeft"] = daysLeft;
        if (DayCount != 30)
        {
            tag["DayCount"] = DayCount;
        }
        if (!counterActive)
        {
            tag["counterActive"] = counterActive;
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
        if (tag.ContainsKey("counterActive"))
        {
            counterActive = tag.GetBool("counterActive");
        }
    }

    public override void SaveWorldHeader(TagCompound tag)
    {
        tag["daysLeft"] = daysLeft;
        if (DayCount != 30)
        {
            tag["DayCount"] = DayCount;
        }
        if (DoomsdayManager.worldEndChoice != DoomsdayOptions.Dissipation)
        {
            tag["worldEndChoice"] = (int)DoomsdayManager.worldEndChoice;
        }
    }

    public override void ClearWorld()
    {
        DayCount = 30;
        daysLeft = DayCount;
        counterActive = true;
    }

    bool wasDay = true;
    public static bool counterActive = true;

    public override void PostUpdateTime()
    {
        if (Main.dayTime && !wasDay)
        {
            daysLeft--;
        }
        wasDay = Main.dayTime;
        if (!counterActive)
        {
            return;
        }
        if (LastDay && Utils.GetDayTimeAs24FloatStartingFromMidnight() >= doomsdayTime)
        {
            DoomsdayManager.DestroyWorldAccordingToChoice();
            counterActive = false;
        }
    }

    public static float PercentTimeLeft()
    {
        return float.Max(0, (float)daysLeft / (float)DayCount);
    }

    public static bool TimeLeftInRange(int denominator, int numerator = 1)
    {
        return PercentTimeLeft() <= ((float)numerator / (float)denominator);
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

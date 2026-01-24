using Terraria;
using Terraria.Audio;
using Terraria.ID;
using System;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.IO;

namespace SolarDoomsday;

public class DoomsdayClock : ModSystem
{
    public static int DayCount { get; private set; } = 30;

    public static int daysLeft = DayCount;

    public static DayText dayText;

    public const float doomsdayTime = 16.5f;

    public static bool Ongoing => !DoomsdayManager.sunDied && !DoomsdayManager.savedEverybody;

    public static bool LastDay => daysLeft == 1 && DoomsdayManager.worldEndChoice != DoomsdayOptions.Stagnation && Ongoing;

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
        if (DoomsdayManager.savedEverybody)
        {
            tag["savedEverybody"] = DoomsdayManager.savedEverybody;
        }
    }

    public override void ClearWorld()
    {
        DayCount = 30;
        daysLeft = DayCount;
        counterActive = true;
    }

    static bool wasDay = true;
    public static bool counterActive = true;

    public override void PostUpdateTime()
    {
        if (DoomsdayManager.savedEverybody)
        {
            return;
        }
        if (Main.dayTime && !wasDay)
        {
            daysLeft--;
            if (!Main.dedServ && Ongoing && DoomsdayManager.worldEndChoice != DoomsdayOptions.Stagnation)
            {
                SoundEngine.PlaySound(new SoundStyle("SolarDoomsday/Assets/bell"));
            }
        }
        wasDay = Main.dayTime;
        if (!counterActive)
        {
            return;
        }
        if (LastDay && Utils.GetDayTimeAs24FloatStartingFromMidnight() >= doomsdayTime && Main.netMode != NetmodeID.MultiplayerClient)
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

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write((ushort)DayCount);
        writer.Write((ushort)daysLeft);
        writer.Write(counterActive);
        writer.Write((byte)DoomsdayManager.worldEndChoice);
    }

    public override void NetReceive(BinaryReader reader)
    {
        DayCount = (int)reader.ReadUInt16();
        daysLeft = (int)reader.ReadUInt16();
        counterActive = reader.ReadBoolean();
        DoomsdayManager.worldEndChoice = (DoomsdayOptions)reader.ReadByte();
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
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    public override string Command => "setcounter";
}

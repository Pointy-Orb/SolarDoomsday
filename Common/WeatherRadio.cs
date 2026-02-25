using Terraria;
using System;
using System.Globalization;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace SolarDoomsday;

public class WeatherRadio : GlobalInfoDisplay
{
    static bool fahrenheit = true;

    public override void SetStaticDefaults()
    {
        var region = RegionInfo.CurrentRegion;
        if (region.IsMetric)
        {
            fahrenheit = false;
        }
    }

    private static int _currentTemperature = 80;
    private static int _targetTemperature = 80;

    public static int CurrentTemperature
    {
        get
        {
            return fahrenheit ? _currentTemperature : (int)MathF.Round((float)(_currentTemperature - 32) * (5f / 9f));
        }
        set
        {
            _targetTemperature = value;
        }
    }

    public static void ForceCurrentTemp(int temp)
    {
        _currentTemperature = temp;
    }

    public override void ModifyDisplayParameters(InfoDisplay currentDisplay, ref string displayValue, ref string displayName, ref Color displayColor, ref Color displayShadowColor)
    {
        if (currentDisplay != InfoDisplay.WeatherRadio)
        {
            return;
        }
        if (DoomsdayManager.thisWorldNeverSawTerror)
        {
            return;
        }
        if (_currentTemperature != _targetTemperature)
        {
            if (Main.rand.NextBool(Int32.Clamp(Math.Abs(_currentTemperature - _targetTemperature) / 2, 1, 120), 120))
            {
                var negative = _currentTemperature < _targetTemperature ? 1 : -1;
                _currentTemperature += Main.rand.Next(1, 2) * negative;
            }
        }
        else
        {
            if (Main.rand.NextBool(180))
            {
                _currentTemperature += Main.rand.Next(1, 4) * (Main.rand.NextBool() ? -1 : 1);
            }
        }
        displayValue = displayValue.Insert(0, $"{CurrentTemperature}°{(fahrenheit ? "F" : "C")} ");
    }
}

public class TemperatureSystem : ModSystem
{
    private int dayVariance = 3;
    bool wasDay = false;
    public override void PostUpdateTime()
    {
        if (Main.dayTime != wasDay)
        {
            dayVariance = Main.rand.Next(-8, 9);
        }
        int rainDiff = DoomsdayManager.RainingAndSafe ? -20 : 0;
        if (Main.IsItDay())
        {
            if (!DoomsdayClock.TimeLeftInRange(3, 2))
            {
                WeatherRadio.CurrentTemperature = (int)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1, 2f / 3f, 80, 135) + dayVariance + rainDiff;
            }
            else if (!DoomsdayClock.TimeLeftInRange(3))
            {
                WeatherRadio.CurrentTemperature = (int)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 2f / 3f, 1f / 3f, 135, 190) + dayVariance + rainDiff;
            }
            else if (!DoomsdayClock.LastDay)
            {
                WeatherRadio.CurrentTemperature = (int)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, 190, 535) + dayVariance + rainDiff;
            }
            else
            {
                WeatherRadio.CurrentTemperature = (int)Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 4.5f, 16.5f, 535, 1783) + dayVariance + rainDiff;
            }
            if (!DoomsdayClock.Ongoing)
            {
                WeatherRadio.CurrentTemperature = 80 + dayVariance * 2;
            }
        }
        else
        {
            WeatherRadio.CurrentTemperature = 60 + dayVariance;
        }
        wasDay = Main.dayTime;
    }
}

public class SetTempOnWorldEnter : ModPlayer
{
    public override void OnEnterWorld()
    {
        if (Player != Main.LocalPlayer)
        {
            return;
        }
        var temp = 80;
        if (Main.IsItDay())
        {
            if (!DoomsdayClock.TimeLeftInRange(3, 2))
            {
                temp = (int)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1, 2f / 3f, 80, 135);
            }
            else if (!DoomsdayClock.TimeLeftInRange(3))
            {
                temp = (int)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 2f / 3f, 1f / 3f, 135, 190);
            }
            else if (!DoomsdayClock.LastDay)
            {
                temp = (int)Utils.Remap(DoomsdayClock.PercentTimeLeft(), 1f / 3f, 0, 190, 535);
            }
            else
            {
                temp = (int)Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 4.5f, 16.5f, 535, 1783);
            }
        }
        else
        {
            temp = 60;
        }
        WeatherRadio.ForceCurrentTemp(temp);
    }
}

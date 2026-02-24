using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace SolarDoomsday;

public class StagnationWatch : GlobalInfoDisplay
{
    public override void ModifyDisplayParameters(InfoDisplay currentDisplay, ref string displayValue, ref string displayName, ref Color displayColor, ref Color displayShadowColor)
    {
        if (currentDisplay.Type != InfoDisplay.Watches.Type)
        {
            return;
        }
        if (DoomsdayManager.thisWorldNeverSawTerror || DoomsdayManager.savedEverybody)
        {
            return;
        }
        if (DoomsdayManager.worldEndChoice != DoomsdayOptions.Stagnation && !DoomsdayManager.sunDied)
        {
            return;
        }
        int dayNumber = DoomsdayClock.DayCount - DoomsdayClock.daysLeft + 1;
        displayValue = displayValue.Insert(0, Language.GetTextValue("Mods.SolarDoomsday.StagnationWatchPrefix", dayNumber));
    }
}

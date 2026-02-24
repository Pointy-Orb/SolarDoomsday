using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;

namespace SolarDoomsday;

public enum Enabling
{
    e,
    no
}

public class WorldCreationVars
{
    public UIWorldCreation self;
    public UIElement container;
    public float accumulatedHeight;
    public float useableWidthPercent;
    public string tagGroup;
}

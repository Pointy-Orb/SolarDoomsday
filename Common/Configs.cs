using System.ComponentModel;
using Terraria.GameContent;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Terraria.ModLoader.Config;

namespace SolarDoomsday;

public class ServerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(false)]
    public bool RainGivesSafety { get; set; }
}

public enum DayCounterDisplay
{
    Fancy,
    Plain,
    Minimal,
    None
}

public class ClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(1f)]
    public float ScreenShakeStrength { get; set; }

    [DefaultValue(DayCounterDisplay.Fancy)]
    [Dropdown]
    public DayCounterDisplay dayCounterDisplay { get; set; }
}

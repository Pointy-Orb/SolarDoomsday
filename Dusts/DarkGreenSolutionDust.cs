using Terraria.ID;
using Terraria.ModLoader;

namespace SolarDoomsday.Dusts;

public class DarkGreenSolutionDust : ModDust
{
    public override void SetStaticDefaults()
    {
        UpdateType = DustID.PureSpray;
    }
}

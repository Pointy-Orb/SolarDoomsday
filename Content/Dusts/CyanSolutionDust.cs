using Terraria.ID;
using Terraria.ModLoader;

namespace SolarDoomsday.Content.Dusts;

public class CyanSolutionDust : ModDust
{
    public override void SetStaticDefaults()
    {
        UpdateType = DustID.PureSpray;
    }
}

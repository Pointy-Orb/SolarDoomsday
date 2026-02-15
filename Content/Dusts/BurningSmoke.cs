using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Dusts;

public class BurningSmoke : ModDust
{
    public override bool MidUpdate(Dust dust)
    {
        dust.velocity.Y = -1;
        return true;
    }
}

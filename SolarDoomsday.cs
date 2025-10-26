using System;
using Terraria;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace SolarDoomsday
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class SolarDoomsday : Mod
    {
        public override void Load()
        {
            if (!Main.dedServ)
            {
                Filters.Scene["MajorasMaskTribute:BigScaryFlashShader"] = new Filter(new BigScaryFlashShader("FilterBloodMoon"), EffectPriority.VeryHigh);
            }
        }
    }
}

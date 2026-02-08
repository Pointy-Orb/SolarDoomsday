using System;
using Terraria;
using System.Collections.Generic;
using SolarDoomsday.Tiles;
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
        public static Mod mod;

        internal static List<int> extraFlammables = new();

        public override void Load()
        {
            mod = this;
            if (!Main.dedServ)
            {
                Filters.Scene["SolarDoomsday:BigScaryFlashShader"] = new Filter(new BigScaryFlashShader("FilterBloodMoon"), EffectPriority.VeryHigh);
            }
        }

        /*
        public override object Call(params object[] args)
        {
            Array.Resize(ref args, 2);
            try
            {
                if (!(args[1] is int type))
                {
                    return false;
                }
                string message = args[0] as string;
                if (message == "MakeFlammable")
                {
                    extraFlammables.Add(type);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Call Error: {e.StackTrace} {e.Message}");
            }
            return false;
        }
		*/
    }
}

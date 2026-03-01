using System;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using System.Collections.Generic;
using SolarDoomsday.Content.Tiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using System.IO;

namespace SolarDoomsday
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class SolarDoomsday : Mod
    {
        public static Mod mod;

        internal static List<int> extraFlammables = new();

        internal enum MessageType
        {
            SetFire,
            PutOutFire,
            FireBurnEffects,
            WaterFireSfx
        }

        public override void Load()
        {
            mod = this;
            if (!Main.dedServ)
            {
                Filters.Scene["SolarDoomsday:BigScaryFlashShader"] = new Filter(new BigScaryFlashShader("FilterBloodMoon"), EffectPriority.VeryHigh);
            }
            IL_Main.UpdateTime_StartDay += IL_StopEclipse;
        }

        public override void Unload()
        {
            IL_Main.UpdateTime_StartDay -= IL_StopEclipse;
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            var messageType = (MessageType)reader.ReadByte();
            int i = 0;
            int j = 0;
            switch (messageType)
            {
                case MessageType.SetFire:
                    i = reader.ReadInt32();
                    j = reader.ReadInt32();
                    Fire.SetOnFire(i, j);
                    //Fire.ReframeFire(i, j);
                    if (!Main.dedServ)
                    {
                        break;
                    }
                    RemoteSetFire(i, j);
                    break;
                case MessageType.PutOutFire:
                    i = reader.ReadInt32();
                    j = reader.ReadInt32();
                    Fire.PutOutFire(i, j);
                    //Fire.ReframeFire(i, j);
                    if (!Main.dedServ)
                    {
                        break;
                    }
                    RemoteDelFire(i, j);
                    break;
                case MessageType.FireBurnEffects:
                    i = reader.ReadInt32();
                    j = reader.ReadInt32();
                    Fire.BurnAudioVisual(i, j);
                    break;
                case MessageType.WaterFireSfx:
                    i = reader.ReadInt32();
                    j = reader.ReadInt32();
                    SoundEngine.PlaySound(SoundID.LiquidsWaterLava, new Point(i, j).ToWorldCoordinates());
                    break;
            }
        }

        public static void RemoteSetFire(int i, int j)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                return;
            }
            var packet = mod.GetPacket();
            packet.Write((byte)MessageType.SetFire);
            packet.Write(i);
            packet.Write(j);
            packet.Send();
        }

        public static void RemoteDelFire(int i, int j)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                return;
            }
            var packet = mod.GetPacket();
            packet.Write((byte)MessageType.PutOutFire);
            packet.Write(i);
            packet.Write(j);
            packet.Send();
        }

        public static void FireBurnEffects(int i, int j)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                return;
            }
            var packet = mod.GetPacket();
            packet.Write((byte)MessageType.FireBurnEffects);
            packet.Write(i);
            packet.Write(j);
            packet.Send();
        }

        public static void WaterFireSfx(int i, int j)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                return;
            }
            var packet = mod.GetPacket();
            packet.Write((byte)MessageType.WaterFireSfx);
            packet.Write(i);
            packet.Write(j);
            packet.Send();
        }

        private static void IL_StopEclipse(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);
                var jumpLabel = il.DefineLabel();
                c.GotoNext(i => i.MatchStsfld(typeof(Main).GetField(nameof(Main.eclipse))));
                c.GotoPrev(MoveType.After, i => i.MatchBrtrue(out jumpLabel));
                c.EmitDelegate<Func<bool>>(() =>
                {
                    return !DoomsdayClock.LastDay;
                });
                c.Emit(Brfalse_S, jumpLabel);
            }
            catch
            {
                MonoModHooks.DumpIL(mod, il);
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

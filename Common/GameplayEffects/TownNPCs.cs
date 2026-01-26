using Terraria;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.OpCodes;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.GameplayEffects;

public class TownNPCManagement : GlobalNPC
{
    public override void Load()
    {
        IL_NPC.AI_007_TownEntities += IL_KeepNPCsInside;
    }

    private static void IL_KeepNPCsInside(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            int desirableIndex = 1;
            var usurperLabel = il.DefineLabel();
            c.GotoNext(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.raining))));
            c.GotoNext(MoveType.After, i => i.MatchStloc(out desirableIndex));
            c.Emit(Ldarg_0);
            c.Emit(Ldfld, typeof(NPC).GetField(nameof(NPC.position)));
            c.Emit(Ldloc, desirableIndex);
            c.EmitDelegate<Func<Vector2, bool, bool>>((position, flag) =>
            {
                if (!DoomsdayClock.TimeLeftInRange(3))
                {
                    return flag;
                }
                var tilePosition = position.ToTileCoordinates();
                if (tilePosition.Y > Main.worldSurface)
                {
                    return flag;
                }
                return true;
            });
            c.Emit(Stloc, desirableIndex);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<SolarDoomsday>(), il);
        }
    }
}

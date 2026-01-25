
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using Microsoft.Xna.Framework;

namespace SolarDoomsday;

public class PostApocalypseSystem : ModSystem
{
    private static bool enoughTiles = false;

    public static bool InPostApocalypse(Player player)
    {
        if (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight || player.ZoneUnderworldHeight)
        {
            return false;
        }
        if (!enoughTiles)
        {
            return false;
        }
        if (!DoomsdayManager.savedEverybody)
        {
            return false;
        }
        return true;
    }

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        enoughTiles = tileCounts[TileID.Ash] > 100;
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!InPostApocalypse(Main.LocalPlayer))
        {
            return;
        }
    }
}

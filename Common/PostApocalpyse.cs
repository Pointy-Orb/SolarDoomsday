
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;

namespace SolarDoomsday;

public class PostApocalypse : ModSystem
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
}

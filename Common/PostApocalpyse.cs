using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using Microsoft.Xna.Framework;

namespace SolarDoomsday;

public class PostApocalypseSystem : ModSystem
{
    private static bool enoughTiles = false;

    private static bool enoughAsh = false;

    private float transitionAmount = 0f;

    public static bool InPostApocalypse(Player player)
    {
        if (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight || player.ZoneUnderworldHeight)
        {
            return false;
        }
        if (!player.ZonePurity)
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
        enoughTiles = tileCounts[TileID.Grass] < 80;
        enoughAsh = tileCounts[TileID.Ash] > 100;
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        var modifiedColor = backgroundColor.MultiplyRGB(Color.Lerp(Color.White, Color.DarkOrange, enoughAsh ? 0.8f : 0.6f));
        backgroundColor = Color.Lerp(backgroundColor, modifiedColor, transitionAmount);
    }

    public override void PostUpdateWorld()
    {
        if (InPostApocalypse(Main.LocalPlayer) && transitionAmount < 1f)
        {
            transitionAmount += 0.02f;
        }
        if (!InPostApocalypse(Main.LocalPlayer) && transitionAmount > 0f)
        {
            transitionAmount -= 0.02f;
        }
    }
}

public class PostApocalypseScene : ModSceneEffect
{
    public override int Music => !Main.swapMusic == Main.drunkWorld && !Main.remixWorld ? MusicID.OtherworldlySpace : MusicID.OtherworldlySpace;

    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;

    public override bool IsSceneEffectActive(Player player)
    {
        return PostApocalypseSystem.InPostApocalypse(player);
    }
}

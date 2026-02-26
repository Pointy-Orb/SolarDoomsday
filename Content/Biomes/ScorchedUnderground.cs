using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using Microsoft.Xna.Framework;

namespace SolarDoomsday.Content.Biomes;

public class ScorchedUnderground : ModBiome
{
    public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<ScorchedUndergroundBackgroundStyle>();

    public override Color? BackgroundColor => Color.DarkSlateGray;

    public override int Music => MusicID.OtherworldlyDungeon;

    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

    public override bool IsBiomeActive(Player player)
    {
        return player.ZoneDirtLayerHeight && ModContent.GetInstance<ScorchedTileCount>().scorchedTileCount >= 500;
    }
}

public class DisplayScorchedBackground : ModSceneEffect
{
    public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<ScorchedUndergroundBackgroundStyle>();

    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

    public override bool IsSceneEffectActive(Player player)
    {
        return ModContent.GetInstance<ScorchedTileCount>().scorchedTileCount >= 400;
    }
}

public class ScorchedTileCount : ModSystem
{
    public int scorchedTileCount;

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        scorchedTileCount = tileCounts[ModContent.TileType<Tiles.Hornfels>()];
    }
}

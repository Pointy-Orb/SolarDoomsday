using Terraria.ModLoader;

namespace SolarDoomsday.Content.Biomes;

public class ScorchedUndergroundBackgroundStyle : ModUndergroundBackgroundStyle
{
    public override void FillTextureArray(int[] textureSlots)
    {
        for (int i = 0; i < 4; i++)
        {
            textureSlots[i] = BackgroundTextureLoader.GetBackgroundSlot(Mod, $"Assets/Backgrounds/ScorchedUnderground{i}");
        }
    }
}

using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using System.Reflection;
using Terraria.UI;
using Terraria.ModLoader.UI;
using Terraria.ID;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SolarDoomsday;

public class WorldFile : ModSystem
{
    public override void Load()
    {
        On_UIWorldListItem.DrawSelf += WriteDaysLeft;
    }

    private static void WriteDaysLeft(On_UIWorldListItem.orig_DrawSelf orig, UIWorldListItem self, SpriteBatch spriteBatch)
    {
        orig(self, spriteBatch);
        bool data = self.Data.TryGetHeaderData(ModContent.GetInstance<DoomsdayClock>(), out var _data);
        int daysLeft = 30;
        int DayCount = 30;
        bool savedEverybody = false;
        DoomsdayOptions worldEndChoice = DoomsdayOptions.Dissipation;
        if (data)
        {
            daysLeft = _data.GetInt("daysLeft");
            savedEverybody = _data.GetBool("savedEverybody");
            if (_data.ContainsKey("DayCount"))
            {
                DayCount = _data.GetInt("DayCount");
            }
            if (_data.ContainsKey("worldEndChoice"))
            {
                worldEndChoice = (DoomsdayOptions)_data.GetInt("worldEndChoice");
            }
        }
        var displayMaxDays = !(daysLeft <= 0 || worldEndChoice == DoomsdayOptions.Stagnation || savedEverybody);
        var text = Language.GetTextValue("Mods.SolarDoomsday.WorldFileLabel", (DayCount - daysLeft + 1).ToString() + (displayMaxDays ? $"/{DayCount}" : ""));
        UIElement WorldIcon = (UIElement)typeof(UIWorldListItem).GetField("_worldIcon", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self);
        var dimensions = WorldIcon.GetDimensions();
        var innerDimensions = self.GetInnerDimensions();
        var vector = new Vector2(innerDimensions.X + innerDimensions.Width - 240f, innerDimensions.Y + 58f);
        var DrawPanel = typeof(UIWorldListItem).GetMethod("DrawPanel", BindingFlags.Instance | BindingFlags.NonPublic);
        float width = 160f;
        DrawPanel.Invoke(self, new object[] { spriteBatch, vector, width });
        spriteBatch.Draw(WorldGenPage.icons[savedEverybody ? 3 : (int)worldEndChoice].Value, vector + new Vector2(0f, -2f), Color.White);
        vector.X += 10f + WorldGenPage.icons[(int)worldEndChoice].Width();
        //float x = FontAssets.MouseText.Value.MeasureString(text).X;
        //float x2 = width * 0.5f - x * 0.5f;
        Utils.DrawBorderString(spriteBatch, text, vector + new Vector2(0, 3f), Color.White);
    }
}

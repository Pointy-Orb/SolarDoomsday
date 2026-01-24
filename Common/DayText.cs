using Terraria;
using ReLogic.Content;
using Terraria.GameContent;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SolarDoomsday;

public class DayText : UIElement
{
    public int time { get; private set; } = 0;

    public static Asset<Texture2D> texture;

    private Vector2 Position => new((float)Main.screenWidth * HAlign, (float)Main.screenHeight * VAlign);

    private static Rectangle NumberRect(int rNum)
    {
        int num = Int32.Clamp(rNum, 0, 9);
        return new Rectangle((width + spacer) * num, 0, width, height);
    }

    private Rectangle DaysLeft => new Rectangle(DoomsdayClock.daysLeft == 1 ? 158 : 0, 68, 156, 40);

    const int width = 48;
    const int height = 64;
    const int spacer = 4;

    public DayText()
    {
        HAlign = 0.85f;
        VAlign = 0.8f;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (DoomsdayManager.savedEverybody)
        {
            return;
        }
        if (!DoomsdayClock.counterActive)
        {
            return;
        }
        if (DoomsdayManager.worldEndChoice == DoomsdayOptions.Stagnation)
        {
            return;
        }
        var pos = Position;
        for (int i = 1; i < digits.Count; i++)
        {
            pos.X -= width / 2;
        }
        var color = Color.Wheat;
        if (DoomsdayManager.worldEndChoice == DoomsdayOptions.Nova)
        {
            color = Color.DarkGoldenrod;
        }
        for (int i = 0; i < digits.Count; i++)
        {
            spriteBatch.Draw(texture.Value, new Vector2(pos.X + (width * i), pos.Y), NumberRect(digits[i]), color);
        }
        if (!DoomsdayClock.LastDay || DoomsdayClock.doomsdayTime - Utils.GetDayTimeAs24FloatStartingFromMidnight() >= 1f)
        {
            spriteBatch.Draw(texture.Value, new Vector2(Position.X + DaysLeft.Width / 8, Position.Y + height), DaysLeft, color, 0f, new Vector2(DaysLeft.Width / 2, 0), 1f, SpriteEffects.None, 0f);
        }
    }

    public override void Update(GameTime gameTime)
    {
        digits.Clear();
        var numString = DoomsdayClock.daysLeft.ToString();
        if (DoomsdayClock.daysLeft == 1)
        {
            numString = Math.Ceiling(DoomsdayClock.doomsdayTime - Utils.GetDayTimeAs24FloatStartingFromMidnight()).ToString();
        }
        if (DoomsdayClock.daysLeft == 1 && DoomsdayClock.doomsdayTime - Utils.GetDayTimeAs24FloatStartingFromMidnight() < 1f)
        {
            float timeLeft = DoomsdayClock.doomsdayTime - Utils.GetDayTimeAs24FloatStartingFromMidnight();
            numString = Math.Ceiling(timeLeft * 60).ToString();
        }
        foreach (char num in numString)
        {
            digits.Add(num - '0');
        }
    }

    List<int> digits = new();
}

public class DayDisplay : UIState
{
    public DayText dayText;

    public override void OnInitialize()
    {
        dayText = new DayText();
        Append(dayText);
    }
}

[Autoload(Side = ModSide.Client)]
public class DaySystem : ModSystem
{
    internal DayDisplay dayDisplay;

    private UserInterface _dayDisplay;

    public override void Load()
    {
        DayText.texture = ModContent.Request<Texture2D>("SolarDoomsday/Assets/sunpocalypsenumbers");
        dayDisplay = new();
        dayDisplay.Activate();
        _dayDisplay = new UserInterface();
        _dayDisplay.SetState(dayDisplay);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        _dayDisplay?.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "SolarDoomsday: Day Display",
                delegate
                {
                    _dayDisplay.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}



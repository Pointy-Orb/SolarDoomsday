using Terraria;
using ReLogic.Content;
using Terraria.DataStructures;
using System;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Items;

public class SolarAccelerationPill : ModItem
{
    public override string Texture => "SolarDoomsday/Content/Items/SolarAccelerationPill";

    public DoomsdayOptions WorldEndMethod { get; private set; }
    public int DayCount { get; private set; }
    public Color frontColor;

    public static readonly Color[] auraColor = new Color[] { new Color(150, 0, 0, 30), new Color(250, 250, 255, 30), new Color(150, 110, 0, 30) };

    private static Asset<Texture2D> aura;
    private static Asset<Texture2D> front;

    public override string Name => $"{DayCount}Day{WorldEndMethod.ToString()}Pill";

    public override LocalizedText DisplayName => Language.GetText("Mods.SolarDoomsday.Items.SolarAccelerationPill.DisplayName").WithFormatArgs(WorldEndMethod.ToString(), DayCount);

    public override LocalizedText Tooltip => Language.GetText($"Mods.SolarDoomsday.Items.SolarAccelerationPill.Tooltip{WorldEndMethod.ToString()}");

    public SolarAccelerationPill(DoomsdayOptions worldEndMethod, int dayCount, Color frontColor)
    {
        WorldEndMethod = worldEndMethod;
        DayCount = dayCount;
        this.frontColor = frontColor;
    }

    private static LocalizedText apocalypseAgain;

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 5));
        ItemID.Sets.AnimatesAsSoul[Type] = true;

        apocalypseAgain = Language.GetOrRegister("Mods.SolarDoomsday.Announcements.ApocalypseAgain");
        aura = ModContent.Request<Texture2D>("SolarDoomsday/Content/Items/Pills/Aura");
        front = ModContent.Request<Texture2D>("SolarDoomsday/Content/Items/Pills/Front");
    }

    protected override bool CloneNewInstances => true;

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Master;
        Item.width = 20;
        Item.height = 18;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 60;
        Item.useAnimation = 60;
        Item.UseSound = SoundID.Roar;
        Item.consumable = true;
        Item.maxStack = Item.CommonMaxStack;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight || player.ZoneUnderworldHeight)
        {
            return false;
        }
        if (!Main.IsItDay())
        {
            return false;
        }
        if (DoomsdayClock.Ongoing)
        {
            return false;
        }
        return true;
    }

    public override bool? UseItem(Player player)
    {
        Item.consumable = !DoomsdayManager.sunDied;
        DoomsdayManager.savedEverybody = false;
        DoomsdayClock.SetDayCount(DayCount);
        DoomsdayClock.daysLeft = DoomsdayClock.DayCount;
        DoomsdayManager.spookyBackTime = 120;
        DoomsdayManager.worldEndChoice = WorldEndMethod;
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return true;
        }
        if (Main.dedServ)
        {
            ChatHelper.BroadcastChatMessage(apocalypseAgain.ToNetworkText(), new Color(50, 255, 130));
        }
        else
        {
            Main.NewText(apocalypseAgain.Value, 50, 255, 130);
        }
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.FragmentSolar, 20)
            .AddIngredient(ItemID.LunarOre, 5)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var texture = TextureAssets.Item[Type];

        float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
        globalTimeWrappedHourly %= 2f;
        if (globalTimeWrappedHourly < 0.5f)
        {
            globalTimeWrappedHourly = 2f - globalTimeWrappedHourly * 3;
        }
        globalTimeWrappedHourly *= 0.68f;
        //Vector2 drawOrigin = itemFrame.Size() / 2f;

        var drawPos = position;
        drawPos.X += Main.rand.NextFloat(-0.5f, 0.5f) * scale;
        drawPos.Y += Main.rand.NextFloat(-0.5f, 0.5f) * scale;
        spriteBatch.Draw(texture.Value, drawPos, frame, auraColor[(int)WorldEndMethod], 0, origin, globalTimeWrappedHourly * scale, SpriteEffects.None, 0f);
        /*
        for (float num8 = 0f; num8 < 1f; num8 += 0.25f)
        {
        }
        for (float num9 = 0f; num9 < 1f; num9 += 0.34f)
        {
            spriteBatch.Draw(texture.Value, drawPosition + new Vector2(0f, 4f).RotatedBy((num9 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, frame, new Color(140, 120, 255, 77), 0, origin, scale, SpriteEffects.None, 0f);
        }
		*/
        spriteBatch.Draw(texture.Value, drawPos, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0);
        spriteBatch.Draw(aura.Value, drawPos, frame, auraColor[(int)WorldEndMethod], 0f, origin, scale, SpriteEffects.None, 0);
        spriteBatch.Draw(front.Value, drawPos, frame, drawColor.MultiplyRGB(frontColor), 0f, origin, scale, SpriteEffects.None, 0);
        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        var texture = TextureAssets.Item[Type];

        float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
        globalTimeWrappedHourly %= 2f;
        if (globalTimeWrappedHourly < 0.5f)
        {
            globalTimeWrappedHourly = 2f - globalTimeWrappedHourly * 3;
        }
        globalTimeWrappedHourly *= 0.68f;

        Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
        Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);

        drawPosition.X += Main.rand.NextFloat(-0.5f, 0.5f) * scale;
        drawPosition.Y += Main.rand.NextFloat(-0.5f, 0.5f) * scale;

        spriteBatch.Draw(texture.Value, drawPosition, itemFrame, auraColor[(int)WorldEndMethod], 0, drawOrigin, globalTimeWrappedHourly * scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture.Value, drawPosition, itemFrame, Color.White, rotation, drawOrigin, scale, SpriteEffects.None, 0);
        spriteBatch.Draw(aura.Value, drawPosition, itemFrame, auraColor[(int)WorldEndMethod], rotation, drawOrigin, scale, SpriteEffects.None, 0);
        spriteBatch.Draw(front.Value, drawPosition, itemFrame, frontColor, rotation, drawOrigin, scale, SpriteEffects.None, 0);
        return false;
    }
}

public class PillLoader : ILoadable
{
    private static readonly int[] dayCounts = new int[] { 12, 30, 60, 100 };
    private static readonly Color[] dayColors = new Color[] { Color.Magenta, Color.LimeGreen, Color.LightBlue, Color.Gray };

    public void Load(Mod mod)
    {
        foreach (DoomsdayOptions option in Enum.GetValues(typeof(DoomsdayOptions)))
        {
            for (int i = 0; i < dayCounts.Length; i++)
            {
                mod.AddContent(new SolarAccelerationPill(option, dayCounts[i], dayColors[i]));
            }
        }
    }

    public void Unload()
    {
    }
}

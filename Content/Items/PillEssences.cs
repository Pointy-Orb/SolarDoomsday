using Terraria;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;

namespace SolarDoomsday.Content.Items;

public class PillEssences : ModItem
{
    protected override bool CloneNewInstances => true;

    public override string Texture => "SolarDoomsday/Content/Items/PillEssences";

    public DoomsdayOptions WorldEndMethod { get; private set; }

    public PillEssences(DoomsdayOptions WorldEndMethod)
    {
        this.WorldEndMethod = WorldEndMethod;
    }

    public override LocalizedText DisplayName => Language.GetOrRegister("Mods.SolarDoomsday.Items.PillEssences.DisplayName").WithFormatArgs(Language.GetTextValue($"Mods.SolarDoomsday.WorldEndSettings.{WorldEndMethod.ToString()}"));

    public override string Name => $"Solar{WorldEndMethod.ToString()}Essence";

    public override LocalizedText Tooltip => Language.GetOrRegister("Mods.SolarDoomsday.Items.PillEssences.Tooltip").WithFormatArgs(Language.GetTextValue($"Mods.SolarDoomsday.Items.SolarAccelerationPill.Tooltip{WorldEndMethod.ToString()}Inner"));

    private Color color => SolarAccelerationPill.auraColor[(int)WorldEndMethod];

    public override void SetDefaults()
    {
        Item.color = color;
        Item.rare = ItemRarityID.Red;
        Item.width = 26;
        Item.height = 26;
    }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemIconPulse[Type] = true;
        ItemID.Sets.ItemNoGravity[Type] = true;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        Vector3 lightColor = new Vector3(color.R, color.G, color.B);
        lightColor /= 200;
        Lighting.AddLight(Item.position, lightColor);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddTile(TileID.DemonAltar)
            .Register();
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        var texture = TextureAssets.Item[Type];
        Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
        Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);
        spriteBatch.Draw(texture.Value, drawPosition, itemFrame, color, rotation, drawOrigin, scale, SpriteEffects.None, 0);
        return false;
    }
}

public class EssenceLoader : ILoadable
{
    public void Load(Mod mod)
    {
        foreach (DoomsdayOptions option in Enum.GetValues(typeof(DoomsdayOptions)))
        {
            mod.AddContent(new PillEssences(option));
        }
    }

    public void Unload()
    {
    }
}

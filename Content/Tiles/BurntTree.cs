using Terraria;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SolarDoomsday;

public class BurntTree : ModTree
{
    private Asset<Texture2D> texture;
    private Asset<Texture2D> branchesTexture;
    private Asset<Texture2D> topsTexture;

    public override Asset<Texture2D> GetTexture() => texture;
    public override Asset<Texture2D> GetTopTextures() => topsTexture;
    public override Asset<Texture2D> GetBranchTextures() => branchesTexture;

    public override void SetStaticDefaults()
    {
        List<int> dirts = new();
        for (int i = 0; i < TileLoader.TileCount; i++)
        {
            if (TileID.Sets.Dirt[i])
            {
                dirts.Add(i);
            }
        }
        GrowsOnTileId = dirts.ToArray();
        texture = ModContent.Request<Texture2D>("SolarDoomsday/Content/Tiles/BurntTree");
        branchesTexture = ModContent.Request<Texture2D>("SolarDoomsday/Content/Tiles/BurntTree_Branches");
        topsTexture = ModContent.Request<Texture2D>("SolarDoomsday/Content/Tiles/BurntTree_Tops");
    }

    public override bool CanDropAcorn()
    {
        return false;
    }

    public override int DropWood() => ItemID.Wood;

    public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };

    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
    {
    }
}

using Terraria;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Tiles;

public class SuperAliveFire : ModTile
{
    public override string Texture => "Terraria/Images/Tiles_336";

    private Vector3 LightColor = new(0.85f, 0.5f, 0.3f);

    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        TileID.Sets.CanPlaceNextToNonSolidTile[Type] = true;
        DustType = DustID.Torch;
        AnimationFrameHeight = 90;
        HitSound = SoundID.LiquidsWaterLava;
        AddMapEntry(new Color(LightColor));
        Flammable = new bool[TileLoader.TileCount];
        VanillaFallbackOnModDeletion = TileID.LivingFire;
        for (int i = 0; i < TileLoader.TileCount; i++)
        {
            if (FlammableCore.Contains(i))
            {
                Flammable[i] = true;
            }
            if (TileID.Sets.IsATreeTrunk[i])
            {
                Flammable[i] = true;
            }
        }
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        offsetY = 2;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = LightColor.X;
        g = LightColor.Y;
        b = LightColor.Z;
    }

    public override void AnimateTile(ref int frame, ref int frameCounter)
    {
        frame = Main.tileFrame[TileID.LivingFire];
    }

    public static bool[] Flammable;

    public static readonly int[] FlammableCore = new int[]
    {
        TileID.WoodBlock,
        TileID.LivingWood,
        TileID.Shadewood,
        TileID.Ebonwood,
        TileID.DynastyWood,
        TileID.RichMahogany,
        TileID.PalmWood,
        TileID.BorealWood,
        TileID.Pearlwood,
        TileID.LeafBlock,
        TileID.LivingMahoganyLeaves,
        TileID.Rope,
        TileID.Cobweb
    };
}

public class SpreadFire : GlobalTile
{
    public override void RandomUpdate(int i, int j, int type)
    {
        for (int k = i - 1; k <= i + 1; k++)
        {
            for (int l = j - 1; l <= j + 1; l++)
            {
                if (Main.tile[k, l].TileType == ModContent.TileType<SuperAliveFire>())
                {
                    if (AttemptSpread(k, l))
                    {
                        return;
                    }
                }
            }
        }
    }

    private static bool AttemptSpread(int i, int j)
    {
        bool spread = false;
        for (int k = i - 1; k <= i + 1; k++)
        {
            for (int l = j - 1; l <= j + 1; l++)
            {
                int targetType = Main.tile[k, l].TileType;
                if (WorldGen.InWorld(k, l - 1) && TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Main.tile[k, l - 1].TileType])
                {
                    continue;
                }
                if (SuperAliveFire.Flammable[targetType])
                {
                    spread = true;
                    WorldGen.KillTile(k, l, noItem: true);
                    WorldGen.PlaceTile(k, l, ModContent.TileType<SuperAliveFire>(), true);
                    WorldGen.KillWall(k, l);
                    WorldGen.Reframe(k, l);
                    NetMessage.SendTileSquare(-1, k, l, 1, 1);
                }
            }
        }
        if (!spread)
        {
            WorldGen.KillTile(i, j);
            NetMessage.SendTileSquare(-1, i, j, 1, 1);
        }
        return spread;
    }
}

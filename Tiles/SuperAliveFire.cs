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

    public static readonly int[] Flammable = new int[]
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
        if (type != ModContent.TileType<SuperAliveFire>())
        {
            return;
        }
        bool spread = false;
        for (int k = i - 1; k <= i + 1; k++)
        {
            for (int l = j - 1; l <= j + 1; l++)
            {
                int targetType = Main.tile[k, l].TileType;
                if (SuperAliveFire.Flammable.Contains(targetType))
                {
                    spread = true;
                    Main.tile[k, l].TileType = (ushort)ModContent.TileType<SuperAliveFire>();
                    Main.tile[k, l].WallType = 0;
                    WorldGen.Reframe(k, l);
                }
            }
        }
        if (!spread)
        {
            WorldGen.KillTile(i, j);
        }
    }
}

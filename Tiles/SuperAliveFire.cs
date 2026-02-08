using Terraria;
using Terraria.Audio;
using System.Collections.Generic;
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
        TileID.Sets.TouchDamageHot[Type] = true;

        DustType = DustID.Torch;
        AnimationFrameHeight = 90;
        HitSound = new SoundStyle("SolarDoomsday/Assets/silence");
        AddMapEntry(new Color(LightColor));

        Flammable = new bool[TileLoader.TileCount];
        FlammableWall = new bool[WallLoader.WallCount];
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
            if (TileID.Sets.IsVine[i])
            {
                Flammable[i] = true;
            }
        }
        for (int i = 0; i < WallLoader.WallCount; i++)
        {
            if (FlammableWallCore.Contains(i))
            {
                FlammableWall[i] = true;
                if (Main.wallBlend[i] > 0)
                {
                    FlammableWall[Main.wallBlend[i]] = true;
                }
            }
        }
    }

    public override bool CanDrop(int i, int j)
    {
        return false;
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

    public override bool IsTileDangerous(int i, int j, Player player)
    {
        return true;
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
        TileID.LivingMahogany,
        TileID.Rope,
        TileID.Cobweb,
        TileID.BorealBeam,
        TileID.RichMahoganyBeam,
        TileID.WoodenBeam
    };

    public static bool[] FlammableWall;

    public static readonly int[] FlammableWallCore = new int[]
    {
        WallID.Wood,
        WallID.LivingWood,
        WallID.LivingWoodUnsafe,
        WallID.Shadewood,
        WallID.Ebonwood,
        WallID.BlueDynasty,
        WallID.WhiteDynasty,
        WallID.RichMaogany,
        WallID.PalmWood,
        WallID.BorealWood,
        WallID.Pearlwood,
        WallID.LivingLeaf,
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
                if (!WorldGen.InWorld(k, l))
                {
                    continue;
                }
                if (Main.tile[k, l].TileType == ModContent.TileType<SuperAliveFire>())
                {
                    if (AttemptSpreadOuter(k, l))
                    {
                        return;
                    }
                }
            }
        }
    }

    private static bool AttemptSpreadOuter(int i, int j)
    {
        bool spread = false;
        for (int k = i - 1; k <= i + 1; k++)
        {
            for (int l = j - 1; l <= j + 1; l++)
            {
                if (!WorldGen.InWorld(k, l))
                {
                    continue;
                }
                if (Main.raining && Main.rand.NextBool() && l <= Main.worldSurface)
                {
                    continue;
                }
                spread |= AttemptSpread(k, l);
            }
        }
        if (!spread)
        {
            WorldGen.KillTile(i, j);
            NetMessage.SendTileSquare(-1, i, j, 1, 1);
            for (int k = i - 1; k <= i + 1; k++)
            {
                for (int l = j - 1; l <= j + 1; l++)
                {
                    if (!WorldGen.InWorld(k, l))
                    {
                        continue;
                    }
                    if (Main.tile[k, l].TileType == ModContent.TileType<SuperAliveFire>() && AttemptSpreadOuter(k, l))
                    {
                        return true;
                    }
                }
            }
        }
        return spread;
    }

    public static bool AttemptSpread(int i, int j)
    {
        var target = Main.tile[i, j];
        var spread = false;
        if (WorldGen.InWorld(i, j - 1) && ((TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Main.tile[i, j - 1].TileType] && !TileID.Sets.IsATreeTrunk[Main.tile[i, j - 1].TileType]) || TileID.Sets.BasicChest[Main.tile[i, j - 1].TileType]))
        {
            return false;
        }
        if (TileID.Sets.IsATreeTrunk[target.TileType] && (!WorldGen.InWorld(i, j - 1) || TileID.Sets.IsATreeTrunk[Main.tile[i, j - 1].TileType]))
        {
            var y = j;
            var markedTiles = new List<Point>();
            while (WorldGen.InWorld(i, y) && TileID.Sets.IsATreeTrunk[Main.tile[i, y].TileType])
            {
                for (int x = i - 1; x <= i + 1; x++)
                {
                    if (!WorldGen.InWorld(x, y) || !TileID.Sets.IsATreeTrunk[Main.tile[x, y].TileType] || !Main.tile[x, y].HasTile)
                    {
                        continue;
                    }
                    markedTiles.Add(new Point(x, y));
                }
                y--;
            }

            //Order the way the tiles are destroyed so that the top tree parts can actually get burnt
            markedTiles.Sort((left, right) => left.X == i ? 1 : 0);
            markedTiles.Sort((left, right) => left.Y > right.Y ? 1 : left.Y < right.Y ? -1 : 0);
            foreach (Point tile in markedTiles)
            {
                spread = true;
                WorldGen.KillTile(tile.X, tile.Y, noItem: true);
                WorldGen.PlaceTile(tile.X, tile.Y, ModContent.TileType<SuperAliveFire>(), true);
                WorldGen.KillWall(tile.X, tile.Y, true);
                WorldGen.ConvertWall(tile.X, tile.Y, 0);
                WorldGen.Reframe(tile.X, tile.Y);
                NetMessage.SendTileSquare(-1, tile.X, tile.Y, 1, 1);
            }
        }
        if (SuperAliveFire.Flammable[target.TileType])
        {
            spread = true;
            WorldGen.KillTile(i, j, noItem: true);
            WorldGen.PlaceTile(i, j, ModContent.TileType<SuperAliveFire>(), true);
            WorldGen.KillWall(i, j, true);
            WorldGen.ConvertWall(i, j, 0);
            WorldGen.Reframe(i, j);
            NetMessage.SendTileSquare(-1, i, j, 1, 1);
        }
        else if (SuperAliveFire.FlammableWall[target.WallType])
        {
            spread = true;
            WorldGen.KillWall(i, j, true);
            WorldGen.ConvertWall(i, j, 0);
            if (!Main.tile[i, j].HasTile)
            {
                WorldGen.PlaceTile(i, j, ModContent.TileType<SuperAliveFire>(), true);
            }
            WorldGen.Reframe(i, j);
            NetMessage.SendTileSquare(-1, i, j, 1, 1);
        }
        if (TileID.Sets.Grass[target.TileType] || TileID.Sets.Snow[target.TileType])
        {
            spread = true;
            WorldGen.ConvertTile(i, j, TileID.Dirt);
        }
        if (WallID.Sets.Conversion.Grass[target.WallType] || target.WallType == WallID.SnowWallUnsafe)
        {
            WorldGen.ConvertWall(i, j, WallID.DirtUnsafe);
        }
        if (target.TileType == TileID.Explosives)
        {
            WorldGen.KillTile(i, j, fail: false, effectOnly: false, noItem: true);
            NetMessage.SendTileSquare(-1, i, j);
            Projectile.NewProjectile(Wiring.GetProjectileSource(i, j), i * 16 + 8, j * 16 + 8, 0f, 0f, 108, 500, 10f);
        }
        return spread;
    }
}

public class BurnPlayers : ModPlayer
{
    public override void PreUpdateBuffs()
    {
        var tilePoint = Player.Center.ToTileCoordinates();
        if (!WorldGen.InWorld(tilePoint.X, tilePoint.Y))
        {
            return;
        }
        var tile = Main.tile[tilePoint.X, tilePoint.Y];
        if (tile.TileType == ModContent.TileType<SuperAliveFire>())
        {
            Player.AddBuff(BuffID.OnFire, 120);
        }
    }
}

public class BurnNPCs : GlobalNPC
{
    public override void PostAI(NPC npc)
    {
        var npcCoords = npc.Center.ToTileCoordinates();
        if (!WorldGen.InWorld(npcCoords.X, npcCoords.Y))
        {
            return;
        }
        if (npc.type == NPCID.OldMan || npc.immortal || npc.dontTakeDamage)
        {
            return;
        }
        var tile = Main.tile[npcCoords.X, npcCoords.Y];
        if (tile.TileType == ModContent.TileType<SuperAliveFire>())
        {
            npc.AddBuff(BuffID.OnFire, 120);
        }
    }
}

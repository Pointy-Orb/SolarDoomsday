using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Tiles;

/*
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

        VanillaFallbackOnModDeletion = TileID.LivingFire;
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
        TileID.Rope,
        TileID.Cobweb,
        TileID.BorealBeam,
        TileID.RichMahoganyBeam,
        TileID.WoodenBeam,
        TileID.Cactus,
        TileID.PumpkinBlock,
    };

    public static bool[] FlammableWall;

    public static readonly int[] FlammableWallCore = new int[]
    {
        WallID.Wood,
        WallID.WoodenFence,
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
                if (Main.tile[k, l].TileType != ModContent.TileType<SuperAliveFire>())
                {
                    continue;
                }
                var fireBody = new List<Point>();
                fireBody = GetFireBody(k, l, fireBody);
                DoSpread(fireBody);
                return;
            }
        }
    }

    private static void DoSpread(List<Point> fireBody)
    {
        List<bool> spreaded = new();
        //Actual spreading logic
        int edgeCount = 0;
        for (int i = 0; i < fireBody.Count; i++)
        {
            if (IsEdgeFire(fireBody[i].X, fireBody[i].Y))
            {
                edgeCount++;
            }
        }
        for (int i = 0; i < fireBody.Count; i++)
        {
            if (edgeCount < 6 || Main.rand.NextBool(edgeCount * 5 + 5, edgeCount * 6 + 5))
            {
                spreaded.Add(AttemptSpreadOuter(fireBody[i].X, fireBody[i].Y));
            }
            else
            {
                spreaded.Add(false);
            }
        }
        //Determining who gets to live
        List<Point> willDie = new();
        List<Point> willLive = new();
        bool anySpread = false;
        for (int i = 0; i < fireBody.Count; i++)
        {
            anySpread |= spreaded[i];
            if (!IsEdgeFire(fireBody[i].X, fireBody[i].Y))
            {
                continue;
            }
            for (int x = fireBody[i].X - 1; x <= fireBody[i].X + 1; x++)
            {
                for (int y = fireBody[i].Y - 1; y <= fireBody[i].Y + 1; y++)
                {
                    if (!WorldGen.InWorld(x, y))
                    {
                        continue;
                    }
                    if (Main.tile[x, y].TileType != ModContent.TileType<SuperAliveFire>())
                    {
                        continue;
                    }

                    if (spreaded[i])
                    {
                        willLive.Add(new Point(x, y));
                    }
                    else
                    {
                        willDie.Add(new Point(x, y));
                    }
                }
            }
        }
        //Murdering
        if (!anySpread)
        {
            foreach (Point fire in fireBody)
            {
                WorldGen.KillTile(fire.X, fire.Y);
                NetMessage.SendTileSquare(-1, fire.X, fire.Y, 1, 1);
            }
            return;
        }
        foreach (Point fire in willDie)
        {
            if (willLive.Contains(fire))
            {
                continue;
            }
            WorldGen.KillTile(fire.X, fire.Y);
            NetMessage.SendTileSquare(-1, fire.X, fire.Y, 1, 1);
        }
    }

    private static bool IsEdgeFire(int x, int y)
    {
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (!WorldGen.InWorld(i, j))
                {
                    continue;
                }
                if (Main.tile[i, j].TileType != ModContent.TileType<SuperAliveFire>())
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static List<Point> GetFireBody(int x, int y, List<Point> fireBody)
    {
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (!WorldGen.InWorld(i, j))
                {
                    continue;
                }
                var spot = new Point(i, j);
                if (fireBody.Contains(spot) || Main.tile[i, j].TileType != ModContent.TileType<SuperAliveFire>())
                {
                    continue;
                }
                fireBody.Add(spot);
                GetFireBody(i, j, fireBody);
            }
        }
        return fireBody;
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
                if (DoomsdayManager.RainingAndSafe && Main.rand.NextBool() && l <= Main.worldSurface)
                {
                    continue;
                }
                spread |= AttemptSpread(k, l);
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

public class FlammabilitySystem : ModSystem
{
    public static void MarkFlammability()
    {
        List<int> woods = GetWoods();
        if (woods.Contains(TileID.AshWood))
        {
            woods.Remove(TileID.AshWood);
        }

        SuperAliveFire.Flammable = new bool[TileLoader.TileCount];
        SuperAliveFire.FlammableWall = new bool[WallLoader.WallCount];
        for (int i = 0; i < TileLoader.TileCount; i++)
        {
            if (SuperAliveFire.FlammableCore.Contains(i) || woods.Contains(i))
            {
                SuperAliveFire.Flammable[i] = true;
            }
            if (TileID.Sets.IsATreeTrunk[i] ||
                    TileID.Sets.IsVine[i] ||
                    TileID.Sets.TouchDamageDestroyTile[i] ||
                    SolarDoomsday.extraFlammables.Contains(i)
                    )
            {
                SuperAliveFire.Flammable[i] = true;
            }
        }
        for (int i = 0; i < WallLoader.WallCount; i++)
        {
            if (SuperAliveFire.FlammableWallCore.Contains(i))
            {
                SuperAliveFire.FlammableWall[i] = true;
                if (Main.wallBlend[i] > 0)
                {
                    SuperAliveFire.FlammableWall[Main.wallBlend[i]] = true;
                }
            }
        }
    }

    private static List<int> GetWoods()
    {
        List<int> woods = new();
        List<int> woodItems = new();

        //Get wood items before adding them to tiles so that we can also get their variants
        if (RecipeGroup.recipeGroupIDs.ContainsKey("Wood"))
        {
            int index = RecipeGroup.recipeGroupIDs["Wood"];
            RecipeGroup group = RecipeGroup.recipeGroups[index];
            foreach (int itemType in group.ValidItems)
            {
                woodItems.Add(itemType);
            }
        }
        //Add woods that are woods because they become normal wood through Shimmer but can't be used for wood crafing, like Dynasty Wood
        for (int i = 0; i < ItemLoader.ItemCount; i++)
        {
            if (woods.Contains(i))
            {
                continue;
            }
            var item = new Item(i);
            if (ItemID.Sets.ShimmerTransformToItem[i] != ItemID.Wood)
            {
                continue;
            }
            if (item.createTile > -1 && !ItemID.Sets.IsLavaImmuneRegardlessOfRarity[i])
            {
                woods.Add(item.createTile);
            }
        }

        //Add woods created by wands
        for (int i = 0; i < ItemLoader.ItemCount; i++)
        {
            var item = new Item(i);
            if (!woodItems.Contains(item.tileWand))
            {
                continue;
            }
            if (item.createTile > -1)
            {
                woods.Add(item.createTile);
            }
        }
        //Add wood tiles
        foreach (int itemType in woodItems)
        {
            var item = new Item(itemType);
            if (item.createTile > -1 && !ItemID.Sets.IsLavaImmuneRegardlessOfRarity[itemType])
            {
                woods.Add(item.createTile);
            }
        }

        return woods;
    }
}


*/

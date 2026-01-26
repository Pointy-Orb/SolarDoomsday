using Terraria;
using System;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Items;

public class CyanSolution : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 99;
        ItemID.Sets.SortingPriorityTerraforming[Type] = 101;
    }

    public override void SetDefaults()
    {
        Item.DefaultToSolution(ModContent.ProjectileType<CyanSolutionProjectile>());
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Solutions;
    }
}

public class CyanSolutionProjectile : ModProjectile
{
    public static int ConversionType;

    public ref float Progress => ref Projectile.ai[0];
    public bool ShotFromTerraformer => Projectile.ai[1] == 1f;

    public override void SetStaticDefaults()
    {
        ConversionType = ModContent.GetInstance<CyanSolutionConversion>().Type;
    }

    public override void SetDefaults()
    {
        Projectile.DefaultToSpray();
        Projectile.aiStyle = 0;
    }

    public override bool? CanDamage() => false;

    public override void AI()
    {
        if (Projectile.timeLeft > 133)
            Projectile.timeLeft = 133;

        if (Projectile.owner == Main.myPlayer)
        {
            int size = ShotFromTerraformer ? 3 : 2;
            Point tileCenter = Projectile.Center.ToTileCoordinates();
            WorldGen.Convert(tileCenter.X, tileCenter.Y, ConversionType, size, true, true);
        }

        int spawnDustTreshold = 7;
        if (ShotFromTerraformer)
            spawnDustTreshold = 3;

        if (Progress > (float)spawnDustTreshold)
        {
            float dustScale = 1f;
            int dustType = ModContent.DustType<Dusts.CyanSolutionDust>();

            if (Progress == spawnDustTreshold + 1)
                dustScale = 0.2f;
            else if (Progress == spawnDustTreshold + 2)
                dustScale = 0.4f;
            else if (Progress == spawnDustTreshold + 3)
                dustScale = 0.6f;
            else if (Progress == spawnDustTreshold + 4)
                dustScale = 0.8f;

            int dustArea = 0;
            if (ShotFromTerraformer)
            {
                dustScale *= 1.2f;
                dustArea = (int)(12f * dustScale);
            }

            Dust sprayDust = Dust.NewDustDirect(new Vector2(Projectile.position.X - dustArea, Projectile.position.Y - dustArea), Projectile.width + dustArea * 2, Projectile.height + dustArea * 2, dustType, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 100);
            sprayDust.noGravity = true;
            sprayDust.scale *= 1.75f * dustScale;
        }

        Progress++;
        Projectile.rotation += 0.3f * Projectile.direction;
    }
}

public class CyanSolutionConversion : ModBiomeConversion
{
    public override void PostSetupContent()
    {
        TileLoader.RegisterConversion(TileID.Ash, Type, ConvertAsh);

        TileLoader.RegisterConversion(TileID.Dirt, Type, CheckStone);
        TileLoader.RegisterConversion(TileID.Stone, Type, CheckStone);

        TileLoader.RegisterConversion(TileID.Sand, Type, CheckStone);
        TileLoader.RegisterConversion(TileID.HardenedSand, Type, CheckStone);

        TileLoader.RegisterConversion(TileID.IceBlock, Type, CheckStone);
        TileLoader.RegisterConversion(TileID.SnowBlock, Type, CheckStone);

        TileLoader.RegisterConversion(TileID.Mud, Type, CheckStone);

        WallLoader.RegisterConversion(ModContent.WallType<Walls.AshWall>(), Type, ConvertAshWall);
        WallLoader.RegisterConversion(ModContent.WallType<Walls.AshWallSafe>(), Type, ConvertAshWall);

        WallLoader.RegisterConversion(0, Type, CheckStone);
    }

    public bool ConvertAsh(int i, int j, int type, int conversionType)
    {
        if (j > Math.Min(Main.rockLayer + 50, Main.UnderworldLayer))
        {
            return false;
        }
        WorldGen.ConvertTile(i, j, TileID.Dirt);
        CheckVicinityForLava(i, j);
        return false;
    }

    public bool ConvertAshWall(int i, int j, int type, int conversionType)
    {
        if (j > Math.Min(Main.rockLayer + 50, Main.UnderworldLayer))
        {
            return false;
        }
        WorldGen.ConvertWall(i, j, WallID.DirtUnsafe);
        return false;
    }

    public bool CheckStone(int i, int j, int type, int conversionType)
    {
        if (j > Math.Min(Main.rockLayer + 50, Main.UnderworldLayer))
        {
            return false;
        }
        CheckVicinityForLava(i, j);
        return false;
    }

    private bool CheckVicinityForLava(int x, int y)
    {
        bool foundIt = false;
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (!WorldGen.InWorld(i, j))
                {
                    continue;
                }
                Tile tile = Main.tile[i, j];
                if (tile.LiquidAmount <= 0 || tile.LiquidType != LiquidID.Lava)
                {
                    continue;
                }
                tile.LiquidAmount = 0;
                foundIt = true;
                if (!tile.HasTile)
                {
                    tile.ClearTile();
                    tile.HasTile = true;
                    tile.TileType = TileID.Stone;
                    WorldGen.Reframe(i, j);
                }
            }
        }
        return foundIt;
    }
}

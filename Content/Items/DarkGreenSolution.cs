
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Items;

public class DarkGreenSolution : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 99;
        ItemID.Sets.SortingPriorityTerraforming[Type] = 101;
    }

    public override void SetDefaults()
    {
        Item.DefaultToSolution(ModContent.ProjectileType<DarkGreenSolutionProjectile>());
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Solutions;
    }
}

public class DarkGreenSolutionProjectile : ModProjectile
{
    public static int ConversionType;

    public ref float Progress => ref Projectile.ai[0];
    public bool ShotFromTerraformer => Projectile.ai[1] == 1f;

    public override void SetStaticDefaults()
    {
        ConversionType = ModContent.GetInstance<DarkGreenSolutionConversion>().Type;
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
            int dustType = ModContent.DustType<Dusts.DarkGreenSolutionDust>();

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

public class DarkGreenSolutionConversion : ModBiomeConversion
{
    public override void PostSetupContent()
    {
        TileLoader.RegisterConversionFallback(TileID.Sand, TileID.Dirt);
        TileLoader.RegisterConversionFallback(TileID.SnowBlock, TileID.Dirt);
        TileLoader.RegisterConversion(TileID.Dirt, Type, TileID.Mud);
        TileLoader.RegisterConversion(TileID.Grass, Type, TileID.JungleGrass);

        WallLoader.RegisterConversion(WallID.DirtUnsafe, Type, ConvertJungleWall);
        WallLoader.RegisterConversionFallback(WallID.Dirt, WallID.DirtUnsafe);
        WallLoader.RegisterConversionFallback(WallID.HardenedSand, WallID.DirtUnsafe);
    }

    public bool ConvertJungleWall(int i, int j, int type, int conversionType)
    {
        var aboveGround = j < Main.worldSurface;
        if (aboveGround)
        {
            WorldGen.ConvertWall(i, j, WallID.MudUnsafe);
        }
        else
        {
            WorldGen.ConvertWall(i, j, WallID.JungleUnsafe);
        }
        return false;
    }
}

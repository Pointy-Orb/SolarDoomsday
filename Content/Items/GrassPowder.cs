using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Items;

public class GrassPowder : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 99;
    }

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.PurificationPowder);
        Item.shoot = ModContent.ProjectileType<GrassPowderProjectile>();
    }
}

public class GrassPowderProjectile : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_10";

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.alpha = 255;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        bool flag23 = Projectile.type == 1019;
        Projectile.velocity *= 0.95f;
        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] == 180f)
        {
            Projectile.Kill();
        }
        if (Projectile.ai[1] == 0f)
        {
            Projectile.ai[1] = 1f;
            int num966 = 30;
            if (Projectile.type == 463)
            {
            }
            if (flag23)
            {
                num966 = 40;
            }
            for (int num977 = 0; num977 < num966; num977++)
            {
                Dust dust209 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 20, Projectile.velocity.X, Projectile.velocity.Y, 50, Color.Green)];
                if (flag23)
                {
                    dust209.noGravity = num977 % 3 != 0;
                    if (!dust209.noGravity)
                    {
                        Dust dust47 = dust209;
                        Dust dust212 = dust47;
                        dust212.scale *= 1.25f;
                        dust47 = dust209;
                        dust212 = dust47;
                        dust212.velocity /= 2f;
                        dust209.velocity.Y -= 2.2f;
                    }
                    else
                    {
                        Dust dust49 = dust209;
                        Dust dust212 = dust49;
                        dust212.scale *= 1.75f;
                        dust49 = dust209;
                        dust212 = dust49;
                        dust212.velocity += Projectile.velocity * 0.65f;
                    }
                }
            }
        }
        bool flag34 = Main.myPlayer == Projectile.owner;
        if (flag23)
        {
            flag34 = Main.netMode != 1;
        }
        if (flag34)
        {
            int num988 = (int)(Projectile.position.X / 16f) - 1;
            int num999 = (int)((Projectile.position.X + (float)Projectile.width) / 16f) + 2;
            int num1010 = (int)(Projectile.position.Y / 16f) - 1;
            int num1021 = (int)((Projectile.position.Y + (float)Projectile.height) / 16f) + 2;
            if (num988 < 0)
            {
                num988 = 0;
            }
            if (num999 > Main.maxTilesX)
            {
                num999 = Main.maxTilesX;
            }
            if (num1010 < 0)
            {
                num1010 = 0;
            }
            if (num1021 > Main.maxTilesY)
            {
                num1021 = Main.maxTilesY;
            }
            Vector2 vector57 = default(Vector2);
            for (int num1032 = num988; num1032 < num999; num1032++)
            {
                for (int num1043 = num1010; num1043 < num1021; num1043++)
                {
                    vector57.X = num1032 * 16;
                    vector57.Y = num1043 * 16;
                    if (!(Projectile.position.X + (float)Projectile.width > vector57.X) || !(Projectile.position.X < vector57.X + 16f) || !(Projectile.position.Y + (float)Projectile.height > vector57.Y) || !(Projectile.position.Y < vector57.Y + 16f))
                    {
                        continue;
                    }
                    if (Main.tile[num1032, num1043].WallType == WallID.DirtUnsafe || Main.tile[num1032, num1043].WallType == WallID.Dirt)
                    {
                        Main.tile[num1032, num1043].WallType = WallID.GrassUnsafe;
                        WorldGen.SquareTileFrame(num1032, num1043);
                        if (Main.netMode == 1)
                        {
                            NetMessage.SendTileSquare(-1, num1032, num1043);
                        }
                    }
                    if (!Main.tile[num1032, num1043].HasTile)
                    {
                        continue;
                    }
                    if (Main.tile[num1032, num1043].TileType == TileID.Dirt && WorldGen.TileIsExposedToAir(num1032, num1043))
                    {
                        Main.tile[num1032, num1043].TileType = TileID.Grass;
                        WorldGen.SquareTileFrame(num1032, num1043);
                        if (Main.netMode == 1)
                        {
                            NetMessage.SendTileSquare(-1, num1032, num1043);
                        }
                    }
                    if (Main.tile[num1032, num1043].TileType == TileID.Mud && WorldGen.TileIsExposedToAir(num1032, num1043))
                    {
                        Main.tile[num1032, num1043].TileType = TileID.JungleGrass;
                        WorldGen.SquareTileFrame(num1032, num1043);
                        if (Main.netMode == 1)
                        {
                            NetMessage.SendTileSquare(-1, num1032, num1043);
                        }
                    }
                    if (Main.tile[num1032, num1043].TileType == ModContent.TileType<Tiles.SuperAliveFire>())
                    {
                        WorldGen.KillTile(num1032, num1043);
                        WorldGen.SquareTileFrame(num1032, num1043);
                        if (Main.netMode == 1)
                        {
                            NetMessage.SendTileSquare(-1, num1032, num1043);
                        }
                    }
                }
            }
        }
        if (flag23 && Projectile.velocity.Length() < 0.5f)
        {
            Projectile.Kill();
        }
    }
}

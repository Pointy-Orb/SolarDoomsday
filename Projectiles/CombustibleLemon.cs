using Terraria;
using SolarDoomsday.Tiles;
using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Projectiles;

public class CombustibleLemon : ModProjectile
{
    private const int DefaultWidthHeight = 16;
    private const int ExplosionWidthHeight = 32;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.Explosive[Type] = true;
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = DefaultWidthHeight;
        Projectile.height = DefaultWidthHeight;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.Ranged;

        Projectile.timeLeft = 180;
    }

    public override void AI()
    {
        if (Projectile.owner == Main.myPlayer && Projectile.wet)
        {
            Projectile.timeLeft = 1;
        }
        DoArson(Projectile.Center);
        var dust = Dust.NewDustPerfect(Projectile.Center, DustID.Flare, Vector2.Zero);
        dust.noGravity = true;
        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
        {
            Projectile.PrepareBombToBlow();
        }
        Projectile.ai[0] += 1f;
        if (Projectile.ai[1] == 1)
        {
            Projectile.ai[0] = 0;
        }
        if (Projectile.ai[0] > 10f && Projectile.ai[1] != 1)
        {
            Projectile.ai[0] = 10f;
            // Roll speed dampening.
            if (Projectile.velocity.Y == 0f && Projectile.velocity.X != 0f)
            {
                Projectile.velocity.X = Projectile.velocity.X * 0.96f;

                if (Projectile.velocity.X > -0.01 && Projectile.velocity.X < 0.01)
                {
                    Projectile.velocity.X = 0f;
                    Projectile.netUpdate = true;
                }
            }
            // Delayed gravity
            Projectile.velocity.Y = Projectile.velocity.Y + 0.4f;
        }
        // Rotation increased by velocity.X
        Projectile.rotation += Projectile.velocity.X * 0.1f;
    }

    public override void PrepareBombToBlow()
    {
        Projectile.tileCollide = false;
        Projectile.alpha = 255;

        Projectile.Resize(ExplosionWidthHeight, ExplosionWidthHeight);
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity *= 0f;
        Projectile.timeLeft = 3;
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        if (Projectile.wet)
        {
            return;
        }
        SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

        for (int i = 0; i < 80; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
            dust.noGravity = true;
            dust.velocity *= 5f;
            dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
            dust.velocity *= 3f;
        }
        for (int g = 0; g < 2; g++)
        {
            var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);
            Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y -= 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y -= 1.5f;
        }
        Projectile.Resize(DefaultWidthHeight, DefaultWidthHeight);
        if (Projectile.owner == Main.myPlayer)
        {
            int explosionRadius = 2;
            int minTileX = (int)(Projectile.Center.X / 16f - explosionRadius);
            int maxTileX = (int)(Projectile.Center.X / 16f + explosionRadius);
            int minTileY = (int)(Projectile.Center.Y / 16f - explosionRadius);
            int maxTileY = (int)(Projectile.Center.Y / 16f + explosionRadius);

            Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);
            TotallyNotJustExplodeTilesRatherBurnThem(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY);
        }
    }

    private void TotallyNotJustExplodeTilesRatherBurnThem(Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ)
    {
        for (int i = minI; i <= maxI; i++)
        {
            for (int j = minJ; j <= maxJ; j++)
            {
                float num3 = Math.Abs((float)i - compareSpot.X / 16f);
                float num2 = Math.Abs((float)j - compareSpot.Y / 16f);
                if (!(Math.Sqrt(num3 * num3 + num2 * num2) < (double)radius))
                {
                    continue;
                }
                if (Main.tile[i, j] == null)
                {
                    continue;
                }
                if (Main.tile[i, j].LiquidAmount > 0 && Main.tile[i, j].LiquidType == LiquidID.Water)
                {
                    continue;
                }
                SpreadFire.AttemptSpread(i, j);
                if (!Main.tile[i, j].HasTile)
                {
                    WorldGen.PlaceTile(i, j, ModContent.TileType<SuperAliveFire>(), true);
                }
                /*
                if (!Main.tile[i, j].HasTile || Main.tileCut[Main.tile[i, j].TileType] || SuperAliveFire.Flammable[Main.tile[i, j].TileType])
                {
                    if (Main.tile[i, j].HasTile)
                    {
                        WorldGen.KillTile(i, j, noItem: true);
                    }
                    WorldGen.PlaceTile(i, j, ModContent.TileType<SuperAliveFire>(), true);
                    if (Main.netMode != 0)
                    {
                        NetMessage.SendData(17, -1, -1, null, 0, i, j);
                    }
                }
                if (TileID.Sets.Grass[Main.tile[i, j].TileType] || TileID.Sets.Snow[Main.tile[i, j].TileType])
                {
                    WorldGen.ConvertTile(i, j, TileID.Dirt);
                }
                for (int k = i; k <= i; k++)
                {
                    for (int l = j; l <= j; l++)
                    {
                        if (Main.tile[k, l] != null && SuperAliveFire.FlammableWall[Main.tile[k, l].WallType])
                        {
                            WorldGen.KillWall(k, l);
                            if (Main.netMode != 0)
                            {
                                NetMessage.SendData(17, -1, -1, null, 2, k, l);
                            }
                        }
                    }
                }
				*/
            }
        }
    }

    private void DoArson(Vector2 compareSpot)
    {
        var arsonPoint = compareSpot.ToTileCoordinates();
        if (!WorldGen.InWorld(arsonPoint.X, arsonPoint.Y))
        {
            return;
        }
        SpreadFire.AttemptSpread(arsonPoint.X, arsonPoint.Y);
    }
}

using Terraria;
using SolarDoomsday.Content.Tiles;
using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;

namespace SolarDoomsday.Content.Projectiles;

public enum LemonAIs
{
    Thrown,
    Rocket,
    Grenade,
    Mine,
    Snowman,
    Celeb2
}

// AI 0: Used as timer before applying gravity when thrown 
// AI 1: Used to determine the subtype of AI the lemon should use.
// AI 2: When set to 1, the rocket won't be able to damage players. 
// 	Gets its own AI so that the Celebration Mk2 can fire multiple types and have them all not hurt players.
public class CombustibleLemon : ModProjectile
{
    private const int DefaultWidthHeight = 16;
    private const int ExplosionWidthHeight = 64;

    public LemonAIs LemonType
    {
        get
        {
            return (LemonAIs)Projectile.ai[1];
        }
        set
        {
            Projectile.ai[1] = (float)value;
        }
    }

    public override void Load()
    {
        On_Projectile.BombsHurtPlayers += SnowLemonsDontHurtPlayers;
    }

    private static void SnowLemonsDontHurtPlayers(On_Projectile.orig_BombsHurtPlayers orig, Projectile self, Rectangle projRectangle, int j)
    {
        if (self.type == ModContent.ProjectileType<CombustibleLemon>() && self.ai[2] == 1)
        {
            return;
        }
        orig(self, projRectangle, j);
    }

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
        DrawOriginOffsetY = -5;
    }

    public override void AI()
    {
        if (Projectile.owner == Main.myPlayer && Projectile.wet)
        {
            Projectile.timeLeft = 1;
        }

        if (!(Projectile.velocity.X > -0.2f && Projectile.velocity.X < 0.2f && Projectile.velocity.Y > -0.2f && Projectile.velocity.Y < 0.2f))
        {
            DoArson(Projectile.Center);
        }

        if (Projectile.alpha < 5)
        {
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Flare);
            dust.noGravity = true;
        }

        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
        {
            Projectile.PrepareBombToBlow();
        }
        else
        {
            switch (LemonType)
            {
                case LemonAIs.Thrown:
                case LemonAIs.Grenade:
                    ThrownPhysics();
                    break;
                case LemonAIs.Rocket:
                    //Rockets don't have any special movement code
                    break;
                case LemonAIs.Mine:
                    MinePhysics();
                    break;
                case LemonAIs.Snowman:
                    SnowmanPhysics();
                    break;
            }
        }
        // Rotation increased by velocity.X
        Projectile.rotation += Projectile.velocity.X * 0.1f;
    }

    private void ThrownPhysics()
    {
        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] > 10f)
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
    }

    private void MinePhysics()
    {
        if (Projectile.velocity.X > -0.2f && Projectile.velocity.X < 0.2f && Projectile.velocity.Y > -0.2f && Projectile.velocity.Y < 0.2f)
        {
            Projectile.alpha += 2;
            if (Projectile.alpha > 200)
            {
                Projectile.alpha = 200;
            }
        }
        else
        {
            Projectile.alpha = 0;
        }

        Projectile.velocity.Y += 0.2f; // Make it fall down. Remember, positive Y is down.
        Projectile.velocity *= 0.97f; // Make it slow down.

        // If the mine is moving very slowly, just make it stop entirely.
        if (Projectile.velocity.X > -0.1f && Projectile.velocity.X < 0.1f)
        {
            Projectile.velocity.X = 0f;
        }

        if (Projectile.velocity.Y > -0.1f && Projectile.velocity.Y < 0.1f)
        {
            Projectile.velocity.Y = 0f;
        }
    }

    private void SnowmanPhysics()
    {
        Projectile.localAI[1]++;

        if (Projectile.localAI[1] > 6f)
        {
            Projectile.alpha = 0;
        }
        else
        {
            Projectile.alpha = (int)(255f - 42f * Projectile.localAI[1]) + 100;
            if (Projectile.alpha > 255)
            {
                Projectile.alpha = 255;
            }
        }

        float projDestinationX = Projectile.position.X;
        float projDestinationY = Projectile.position.Y;
        float maxHomingDistance = 600f;

        bool isHoming = false;
        Projectile.ai[0]++;

        if (Projectile.ai[0] > 15f)
        {
            Projectile.ai[0] = 15f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC victim = Main.npc[i];
                if (!victim.CanBeChasedBy(this) || victim.wet)
                {
                    continue;
                }
                float distanceFromProjToTarget = Math.Abs(Projectile.Center.X - victim.Center.X) + Math.Abs(Projectile.Center.Y - victim.Center.Y);
                if (distanceFromProjToTarget < maxHomingDistance && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, victim.position, victim.width, victim.height))
                {
                    maxHomingDistance = distanceFromProjToTarget;
                    projDestinationX = victim.Center.X;
                    projDestinationY = victim.Center.Y;
                    isHoming = true;
                }
            }
        }

        if (!isHoming)
        {
            projDestinationX = Projectile.Center.X + Projectile.velocity.X * 100f;
            projDestinationY = Projectile.Center.Y + Projectile.velocity.Y * 100f;
        }

        float speed = 16f;

        Vector2 finalVelocity = (new Vector2(projDestinationX, projDestinationY) - Projectile.Center).SafeNormalize(-Vector2.UnitY) * speed;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, finalVelocity, 1f / 12f);
    }

    public override void PrepareBombToBlow()
    {
        Projectile.tileCollide = false;
        Projectile.alpha = 255;

        Projectile.Resize(ExplosionWidthHeight, ExplosionWidthHeight);
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (LemonType == LemonAIs.Grenade)
        {
            Projectile.velocity *= 0.77f;
        }
        if (LemonType == LemonAIs.Mine)
        {
            Projectile.timeLeft = 3600;
            Projectile.velocity *= 0.5f;
        }
        if (LemonType == LemonAIs.Snowman)
        {
            Projectile.ai[2] = 1;
            Projectile.scale = 0.9f;
        }
        if (LemonType == LemonAIs.Celeb2)
        {
            LemonType = LemonAIs.Thrown;
            Projectile.ai[2] = 1;
            Projectile.localAI[0] = 6f;
            Projectile.scale *= Main.rand.NextFloat(0.8f, 1.25f);
            if (Main.rand.NextBool())
            {
                Projectile.velocity.X *= Main.rand.NextFloat(0.5f, 2f);
                Projectile.velocity.Y *= Main.rand.NextFloat(0.5f, 2f);
            }
            else
            {
                LemonType = LemonAIs.Snowman;
                Projectile.velocity.X *= Main.rand.NextFloat(0.2f, 5f);
                Projectile.velocity.Y *= Main.rand.NextFloat(0.2f, 5f);
            }
            if (Projectile.velocity.Length() > 12f)
            {
                Projectile.velocity /= 2;
                Projectile.extraUpdates++;
            }
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if ((LemonType == LemonAIs.Rocket || LemonType == LemonAIs.Snowman) && Projectile.timeLeft > 3)
        {
            modifiers.SourceDamage *= 2f;
        }
        if (LemonType == LemonAIs.Mine && Projectile.velocity.Length() < 0.5f)
        {
            modifiers.SourceDamage *= 3f;
        }
        if (LemonType == LemonAIs.Thrown && Main.expertMode)
        {
            if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail)
            {
                modifiers.FinalDamage /= 5f;
            }
        }
        if (LemonType == LemonAIs.Snowman && target.type == NPCID.CultistBoss)
        {
            modifiers.FinalDamage *= 0.75f;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (LemonType == LemonAIs.Grenade || LemonType == LemonAIs.Mine)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = oldVelocity.X * -0.4f;
            }
            if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 0.7f)
            {
                Projectile.velocity.Y = oldVelocity.Y * -0.4f;
            }
            return false;
        }
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
            int explosionRadius = 4;
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
                if (SuperAliveFire.Flammable[Main.tile[i, j].TileType] || Main.tileCut[Main.tile[i, j].TileType] || TileID.Sets.BreakableWhenPlacing[Main.tile[i, j].TileType])
                {
                    WorldGen.KillTile(i, j, noItem: true);
                }
                if (!Main.tile[i, j].HasTile)
                {
                    WorldGen.PlaceTile(i, j, ModContent.TileType<SuperAliveFire>(), true);
                }
            }
        }
    }

    private void DoArson(Vector2 compareSpot)
    {
        var arsonPoint = compareSpot.ToTileCoordinates();
        for (int i = arsonPoint.X - 1; i <= arsonPoint.X + 1; i++)
        {
            for (int j = arsonPoint.Y - 1; j <= arsonPoint.Y + 1; j++)
            {
                if (!WorldGen.InWorld(i, j))
                {
                    continue;
                }
                if (i != arsonPoint.X && j != arsonPoint.Y)
                {
                    continue;
                }
                SpreadFire.AttemptSpread(i, j);
            }
        }
    }
}

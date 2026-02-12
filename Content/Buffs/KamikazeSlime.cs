using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Buffs;

public class KamikazeSlime : ModBuff
{
    public override string Texture => "Terraria/Images/Buff_24";

    private static int fireDebuff => BuffID.OnFire3;

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffDoubleApply[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex)
    {
        if (!npc.HasBuff(fireDebuff) && (!npc.wet || npc.lavaWet) && Main.dayTime)
        {
            npc.AddBuff(fireDebuff, npc.buffTime[buffIndex]);
        }
    }
}

public class BlowUpSlime : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public int blowCooldown = 0;

    public override bool CheckDead(NPC npc)
    {
        if (npc.GetGlobalNPC<BlowUpSlime>().blowCooldown <= 0 && (!npc.wet || npc.lavaWet) && npc.HasBuff(ModContent.BuffType<KamikazeSlime>()) && Main.dayTime)
        {
            var bomb = Projectile.NewProjectileDirect(npc.GetSource_Death(), npc.Center, Vector2.Zero, ModContent.ProjectileType<SlimeBomb>(), npc.damage * 2, 7f);
            bomb.scale = npc.scale;
            bomb.width = (int)(bomb.width * bomb.scale);
            bomb.height = (int)(bomb.height * bomb.scale);
        }
        return true;
    }

    //CanBeHit is used instead of OnHit because for some reason OnHit is called after the kill checking methods
    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        npc.GetGlobalNPC<BlowUpSlime>().blowCooldown = 4;
        return null;
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        npc.GetGlobalNPC<BlowUpSlime>().blowCooldown = 4;
        return null;
    }

    public override void PostAI(NPC npc)
    {
        npc.GetGlobalNPC<BlowUpSlime>().blowCooldown--;
        npc.buffImmune[ModContent.BuffType<KamikazeSlime>()] = Main.raining;
    }
}

public class SlimeBomb : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_28";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.Explosive[Type] = true;
    }

    private int ExplodeRadius => DoomsdayClock.TimeLeftInRange(3) ? 6 : 4;

    public override void SetDefaults()
    {
        Projectile.alpha = 255;
        Projectile.penetrate = -1;
        Projectile.width = ExplodeRadius * 16;
        Projectile.height = ExplodeRadius * 16;

        Projectile.timeLeft = 3;
        Projectile.tileCollide = false;
        Projectile.friendly = false;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
        for (int i = 0; i < 50; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, 0f, 0f, 100, default, 2f);
            dust.velocity *= 1.4f;
        }

        for (int i = 0; i < 80; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
            dust.noGravity = true;
            dust.velocity *= 5f;
            dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
            dust.velocity *= 3f;
        }

        if (Projectile.owner == Main.myPlayer)
        {
            int explosionRadius = (int)(ExplodeRadius * Projectile.scale);
            int minTileX = (int)(Projectile.Center.X / 16f - explosionRadius);
            int maxTileX = (int)(Projectile.Center.X / 16f + explosionRadius);
            int minTileY = (int)(Projectile.Center.Y / 16f - explosionRadius);
            int maxTileY = (int)(Projectile.Center.Y / 16f + explosionRadius);

            Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);

            bool explodeWalls = Projectile.ShouldWallExplode(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY);
            Projectile.ExplodeTiles(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY, explodeWalls);
        }
    }
}

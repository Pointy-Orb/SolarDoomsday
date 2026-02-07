using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;

namespace SolarDoomsday.Projectiles;

public class CombustibleLemonRocket : ModProjectile
{
    public override string Texture => base.Texture.Replace("Rocket", "");

    public override void OnSpawn(IEntitySource source)
    {
        var lemon = Projectile.NewProjectileDirect(source, Projectile.position, Projectile.velocity, ModContent.ProjectileType<CombustibleLemon>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: 1);
        Projectile.Kill();
    }
}

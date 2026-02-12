using Terraria;
using System;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;

namespace SolarDoomsday.Content.Projectiles;

public class AutoloadedLemonRocket : ModProjectile
{
    public override string Texture => ModContent.GetInstance<Items.CombustibleLemon>().Texture;
    private int variant;

    public override string Name => $"CombustibleLemon{((LemonAIs)variant).ToString()}";

    protected override bool CloneNewInstances => true;

    public AutoloadedLemonRocket(int variant)
    {
        this.variant = variant;
    }

    public override void OnSpawn(IEntitySource source)
    {
        var lemon = Projectile.NewProjectileDirect(source, Projectile.position, Projectile.velocity, ModContent.ProjectileType<CombustibleLemon>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: variant);
        Projectile.Kill();
    }
}

public class LemonRocketLoader : ILoadable
{
    public void Load(Mod mod)
    {
        foreach (int lemonAI in Enum.GetValues(typeof(LemonAIs)))
        {
            if (lemonAI == 0)
            {
                continue;
            }
            mod.AddContent(new AutoloadedLemonRocket(lemonAI));
        }
    }

    public void Unload()
    {

    }
}

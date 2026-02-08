using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Items;

public class CombustibleLemon : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
        AmmoID.Sets.IsSpecialist[Type] = true;
        AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher].Add(Type, Mod.Find<ModProjectile>("CombustibleLemonRocket").Type);
        AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.GrenadeLauncher].Add(Type, Mod.Find<ModProjectile>("CombustibleLemonGrenade").Type);
        AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.ProximityMineLauncher].Add(Type, Mod.Find<ModProjectile>("CombustibleLemonMine").Type);
        AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.SnowmanCannon].Add(Type, Mod.Find<ModProjectile>("CombustibleLemonSnowman").Type);
        AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.Celeb2].Add(Type, Mod.Find<ModProjectile>("CombustibleLemonCeleb2").Type);
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.shootSpeed = 12f;
        Item.shoot = ModContent.ProjectileType<Projectiles.CombustibleLemon>();
        Item.width = 20;
        Item.height = 26;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.UseSound = SoundID.Item1;
        Item.useAnimation = 40;
        Item.useTime = 40;
        Item.damage = 60;
        Item.knockBack = 8f;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.value = Item.buyPrice(copper: 75);
        Item.rare = ItemRarityID.Green;
        Item.ammo = AmmoID.Rocket;
        Item.DamageType = DamageClass.Ranged;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Lemon)
            .AddIngredient(ModContent.ItemType<Items.Match>(), 5)
            .DisableDecraft()
            .Register();
    }
}

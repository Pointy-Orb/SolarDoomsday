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
        AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher].Add(Type, ModContent.ProjectileType<Projectiles.CombustibleLemonRocket>());
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
        Item.damage = 80;
        Item.knockBack = 8f;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.value = Item.sellPrice(silver: 22, copper: 50);
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

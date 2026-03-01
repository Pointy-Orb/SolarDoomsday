using Terraria;
using System.Linq;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday;

public class FlammabilitySystem : ModSystem
{
    public static int[] Flammability;
    public static int[] FlammabilityWall;
    public static readonly int[] FlammabilityCore = new int[]
    {
        TileID.Rope,
        TileID.Cobweb,
        TileID.BorealBeam,
        TileID.RichMahoganyBeam,
        TileID.WoodenBeam,
        TileID.Cactus,
        TileID.CactusBlock,
        TileID.PumpkinBlock,
        TileID.Hive,
        TileID.Larva,
        TileID.Torches,
        TileID.HoneyBlock,
        TileID.HayBlock
    };
    private static readonly int[] FlameResistantTiles = new int[]
    {
        TileID.ObsidianBrick,
        TileID.Obsidian,
        TileID.AncientObsidianBrick,
        TileID.TreeAsh
    };
    public static readonly int[] FlammabilityWallCore = new int[]
    {
        WallID.LivingWoodUnsafe,
        WallID.HiveUnsafe,
        WallID.Hive,
        WallID.SpiderUnsafe,
        WallID.SpiderEcho,
        WallID.Cactus
    };

    public static List<int> FlammablePlatformStyles = new();

    public static void MarkFlammability()
    {
        List<int> flammableItems = GetWoodItems();
        List<int> woods = GetWoods(flammableItems);
        if (woods.Contains(TileID.AshWood))
        {
            woods.Remove(TileID.AshWood);
        }

        Flammability = new int[TileLoader.TileCount];
        FlammabilityWall = new int[WallLoader.WallCount];
        for (int i = 0; i < TileLoader.TileCount; i++)
        {
            Flammability[i] = -1;
            if (FlammabilityCore.Contains(i) || woods.Contains(i) || TileID.Sets.IsATreeTrunk[i] && !(i >= TileID.TreeTopaz && i <= TileID.GemSaplings))
            {
                Flammability[i] = 1;
            }
            if (TileID.Sets.IsVine[i] || TileID.Sets.TouchDamageDestroyTile[i] || i == TileID.LivingMahoganyLeaves || i == TileID.LeafBlock)
            {
                Flammability[i] = 2;
            }
            if (FlameResistantTiles.Contains(i))
            {
                Flammability[i] = -2;
            }
            //;)
            if (i == TileID.Explosives)
            {
                Flammability[i] = 5;
            }
        }

        List<int> woodWalls = GetWoodWalls(flammableItems);
        for (int i = 0; i < WallLoader.WallCount; i++)
        {
            FlammabilityWall[i] = -2;
            if (FlammabilityWallCore.Contains(i) || woodWalls.Contains(i))
            {
                FlammabilityWall[i] = 1;
            }
        }

        FlammablePlatformStyles.Clear();
        GetFlammablePlatforms(flammableItems);
    }

    private static List<int> GetWoodItems()
    {
        List<int> flammableItems = new();

        if (RecipeGroup.recipeGroupIDs.ContainsKey("Wood"))
        {
            int index = RecipeGroup.recipeGroupIDs["Wood"];
            RecipeGroup group = RecipeGroup.recipeGroups[index];
            foreach (int itemType in group.ValidItems)
            {
                flammableItems.Add(itemType);
            }
        }

        for (int i = 0; i < ItemLoader.ItemCount; i++)
        {
            if (flammableItems.Contains(i))
            {
                continue;
            }
            var item = new Item(i);
            if (ItemID.Sets.ShimmerTransformToItem[i] != ItemID.Wood)
            {
                continue;
            }
            if (item.createTile > -1 && !ItemID.Sets.IsLavaImmuneRegardlessOfRarity[i])
            {
                flammableItems.Add(i);
            }
        }

        return flammableItems;
    }

    private static void GetFlammablePlatforms(List<int> flammableItems)
    {
        for (int i = 0; i < Recipe.numRecipes; i++)
        {
            var recipe = Main.recipe[i];
            if (recipe.createItem.createTile != TileID.Platforms)
            {
                continue;
            }
            bool usesWood = false;
            foreach (int woodItem in flammableItems)
            {
                if (recipe.HasIngredient(woodItem))
                {
                    usesWood = true;
                    break;
                }
            }
            if (!usesWood)
            {
                continue;
            }
            FlammablePlatformStyles.Add(recipe.createItem.placeStyle);
        }
    }

    private static List<int> GetWoods(List<int> flammableItems)
    {
        List<int> woods = new();

        //Add woods created by wands
        for (int i = 0; i < ItemLoader.ItemCount; i++)
        {
            var item = new Item(i);
            if (!flammableItems.Contains(item.tileWand))
            {
                continue;
            }
            if (item.createTile > -1)
            {
                woods.Add(item.createTile);
            }
        }
        //Add wood tiles
        foreach (int itemType in flammableItems)
        {
            var item = new Item(itemType);
            if (item.createTile > -1 && !ItemID.Sets.IsLavaImmuneRegardlessOfRarity[itemType])
            {
                woods.Add(item.createTile);
            }
        }

        return woods;
    }

    private static List<int> GetWoodWalls(List<int> flammableItems)
    {
        List<int> woodWalls = new();

        for (int i = 0; i < Recipe.numRecipes; i++)
        {
            var recipe = Main.recipe[i];
            if (recipe.createItem.createWall <= -1)
            {
                continue;
            }
            bool usesWood = false;
            foreach (int woodItem in flammableItems)
            {
                if (recipe.HasIngredient(woodItem))
                {
                    usesWood = true;
                    break;
                }
            }
            if (!usesWood)
            {
                continue;
            }
            woodWalls.Add(recipe.createItem.createWall);
            if (ItemID.Sets.ShimmerTransformToItem[recipe.createItem.type] <= 0)
            {
                continue;
            }
            var item = new Item(ItemID.Sets.ShimmerTransformToItem[recipe.createItem.type]);
            if (item.createWall <= -1)
            {
                continue;
            }
            woodWalls.Add(item.createWall);
        }

        return woodWalls;
    }
}

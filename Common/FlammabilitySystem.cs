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
        WallID.Wood,
        WallID.WoodenFence,
        WallID.LivingWood,
        WallID.LivingWoodUnsafe,
        WallID.Shadewood,
        WallID.Ebonwood,
        WallID.BlueDynasty,
        WallID.WhiteDynasty,
        WallID.RichMaogany,
        WallID.PalmWood,
        WallID.BorealWood,
        WallID.Pearlwood,
        WallID.LivingLeaf,
        WallID.HiveUnsafe,
        WallID.Hive
    };

    public static void MarkFlammability()
    {
        List<int> woods = GetWoods();
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
        for (int i = 0; i < WallLoader.WallCount; i++)
        {
            FlammabilityWall[i] = -2;
            if (FlammabilityWallCore.Contains(i))
            {
                FlammabilityWall[i] = 1;
                if (Main.wallBlend[i] > 0)
                {
                    FlammabilityWall[Main.wallBlend[i]] = 1;
                }
            }
        }
    }

    private static List<int> GetWoods()
    {
        List<int> woods = new();
        List<int> woodItems = new();

        //Get wood items before adding them to tiles so that we can also get their variants
        if (RecipeGroup.recipeGroupIDs.ContainsKey("Wood"))
        {
            int index = RecipeGroup.recipeGroupIDs["Wood"];
            RecipeGroup group = RecipeGroup.recipeGroups[index];
            foreach (int itemType in group.ValidItems)
            {
                woodItems.Add(itemType);
            }
        }
        //Add woods that are woods because they become normal wood through Shimmer but can't be used for wood crafing, like Dynasty Wood
        for (int i = 0; i < ItemLoader.ItemCount; i++)
        {
            if (woods.Contains(i))
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
                woods.Add(item.createTile);
            }
        }

        //Add woods created by wands
        for (int i = 0; i < ItemLoader.ItemCount; i++)
        {
            var item = new Item(i);
            if (!woodItems.Contains(item.tileWand))
            {
                continue;
            }
            if (item.createTile > -1)
            {
                woods.Add(item.createTile);
            }
        }
        //Add wood tiles
        foreach (int itemType in woodItems)
        {
            var item = new Item(itemType);
            if (item.createTile > -1 && !ItemID.Sets.IsLavaImmuneRegardlessOfRarity[itemType])
            {
                woods.Add(item.createTile);
            }
        }

        return woods;
    }
}

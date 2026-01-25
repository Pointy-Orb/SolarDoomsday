using Terraria;
using SolarDoomsday.Items;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday;

public class SoldByDryad : GlobalNPC
{
    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.Dryad)
        {
            ModifyDryadShop(shop);
        }
        if (shop.NpcType == NPCID.Steampunker)
        {
            ModifySteampunkerShop(shop);
        }
    }

    private void ModifyDryadShop(NPCShop shop)
    {
        if (shop.TryGetEntry(ItemID.GrassSeeds, out NPCShop.Entry powder))
        {
            shop.InsertBefore(ItemID.GrassSeeds, ModContent.ItemType<GrassPowder>(), new Condition("Mods.SolarDoomsday.Conditions.WhenApocalypseOver", () => DoomsdayManager.savedEverybody));
        }
        else
        {
            shop.Add(ModContent.ItemType<GrassPowder>(), new Condition("Mods.SolarDoomsday.Conditions.WhenApocalypseOver", () => DoomsdayManager.savedEverybody));
        }
    }

    private void ModifySteampunkerShop(NPCShop shop)
    {

    }
}

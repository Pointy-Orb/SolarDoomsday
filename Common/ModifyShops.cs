using Terraria;
using Terraria.Localization;
using SolarDoomsday.Items;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday;

public class ModifyShops : GlobalNPC
{
    public static Condition SavedEverybodyCondition = new Condition("Mods.SolarDoomsday.Conditions.WhenApocalypseOver", () => DoomsdayManager.savedEverybody);

    private static LocalizedText TerraformingKitMessage;

    public override void SetStaticDefaults()
    {
        TerraformingKitMessage = Language.GetOrRegister("Mods.SolarDoomsday.TerraformingKitMessage");
    }

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
        if (shop.NpcType == NPCID.Demolitionist || shop.NpcType == NPCID.Cyborg)
        {
            LEMONS(shop);
        }
    }

    private void ModifyDryadShop(NPCShop shop)
    {
        if (shop.TryGetEntry(ItemID.GrassSeeds, out NPCShop.Entry powder))
        {
            shop.InsertBefore(ItemID.GrassSeeds, ModContent.ItemType<GrassPowder>(), SavedEverybodyCondition);
        }
        else
        {
            shop.Add(ModContent.ItemType<GrassPowder>(), SavedEverybodyCondition);
        }
    }

    private void ModifySteampunkerShop(NPCShop shop)
    {
        var insertedModSolutions = false;
        if (shop.TryGetEntry(ItemID.DirtSolution, out NPCShop.Entry dirtSolution))
        {
            var otherDirt = new Item(ItemID.DirtSolution);
            shop.InsertBefore(dirtSolution, otherDirt, SavedEverybodyCondition);
            shop.InsertBefore(ItemID.DirtSolution, ModContent.ItemType<DarkGreenSolution>(), SavedEverybodyCondition);
            shop.InsertBefore(ItemID.DirtSolution, ModContent.ItemType<CyanSolution>(), SavedEverybodyCondition);
            insertedModSolutions = true;
            dirtSolution.Disable();
        }
        if (shop.TryGetEntry(ItemID.SandSolution, out NPCShop.Entry sandSolution))
        {
            var otherSand = new Item(ItemID.SandSolution);
            shop.InsertBefore(sandSolution, otherSand, SavedEverybodyCondition);
            if (!insertedModSolutions)
            {
                shop.InsertBefore(ItemID.SandSolution, ModContent.ItemType<DarkGreenSolution>(), SavedEverybodyCondition);
                shop.InsertBefore(ItemID.SandSolution, ModContent.ItemType<CyanSolution>(), SavedEverybodyCondition);
                insertedModSolutions = true;
            }
            sandSolution.Disable();
        }
        if (shop.TryGetEntry(ItemID.SnowSolution, out NPCShop.Entry snowSolution))
        {
            var otherSnow = new Item(ItemID.SnowSolution);
            shop.InsertBefore(snowSolution, otherSnow, SavedEverybodyCondition);
            if (!insertedModSolutions)
            {
                shop.InsertBefore(ItemID.SnowSolution, ModContent.ItemType<DarkGreenSolution>(), SavedEverybodyCondition);
                shop.InsertBefore(ItemID.SnowSolution, ModContent.ItemType<CyanSolution>(), SavedEverybodyCondition);
                insertedModSolutions = true;
            }
            snowSolution.Disable();
        }
        if (!insertedModSolutions)
        {
            shop.Add(ModContent.ItemType<DarkGreenSolution>(), SavedEverybodyCondition);
            shop.Add(ModContent.ItemType<CyanSolution>(), SavedEverybodyCondition);
        }
    }

    private void LEMONS(NPCShop shop)
    {
        shop.Add(ModContent.ItemType<CombustibleLemon>(), Condition.PlayerCarriesItem(ModContent.ItemType<CombustibleLemon>()));
    }

    public override void GetChat(NPC npc, ref string chat)
    {
        if (npc.type != NPCID.Steampunker)
        {
            return;
        }
        if (!DoomsdayClock.Ongoing)
        {
            return;
        }
        if (!DoomsdayClock.TimeLeftInRange(3))
        {
            return;
        }
        if (!Main.rand.NextBool(5))
        {
            return;
        }
        chat = TerraformingKitMessage.Value;
    }
}


using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SolarDoomsday;

public class UpdateObsidianSkin : GlobalItem
{
    private static LocalizedText solarTooltip;

    public override void SetStaticDefaults()
    {
        solarTooltip = Language.GetText("Mods.SolarDoomsday.ObsidianSkinSolar");
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.type != ItemID.ObsidianSkinPotion)
        {
            return;
        }
        int lastTooltipLine = 0;
        int tooltipLineCount = 0;
        for (int i = 0; i < tooltips.Count; i++)
        {
            if (tooltips[i].Name.Contains("Tooltip"))
            {
                tooltipLineCount++;
                lastTooltipLine = i;
            }
        }
        tooltips.Insert(lastTooltipLine + 1, new TooltipLine(Mod, $"Tooltip{tooltipLineCount}", solarTooltip.Value));
    }
}

public class UpdateObsidianSkinBuff : GlobalBuff
{
    private static LocalizedText solarTooltip;

    public override void SetStaticDefaults()
    {
        solarTooltip = Language.GetText("Mods.SolarDoomsday.ObsidianSkinSolarBuff");
    }

    public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare)
    {
        if (type != BuffID.ObsidianSkin)
        {
            return;
        }
        tip += $"\n{solarTooltip.Value}";
    }
}

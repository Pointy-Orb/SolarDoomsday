using Terraria;
using System;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday.Content.Items;

public class CosmicSunblock : ModItem
{
    static LocalizedText apocalypseOver;
    static LocalizedText postApocalypse;
    static LocalizedText tooLate;

    public override void SetStaticDefaults()
    {
        apocalypseOver = Language.GetOrRegister("Mods.SolarDoomsday.Announcements.ApocalypseEnd");
        postApocalypse = Language.GetOrRegister("Mods.SolarDoomsday.Announcements.ApocalypseEndLate");
        tooLate = Language.GetOrRegister("Mods.SolarDoomsday.Announcements.TooLate");
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 28;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 60;
        Item.useAnimation = 60;
        Item.UseSound = SoundID.Item120;
        Item.consumable = true;
        Item.rare = ItemRarityID.Purple;
        Item.maxStack = Item.CommonMaxStack;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight || player.ZoneUnderworldHeight)
        {
            return false;
        }
        if (!Main.dayTime)
        {
            return false;
        }
        if (DoomsdayManager.savedEverybody)
        {
            return false;
        }
        return true;
    }

    public override void UpdateInventory(Player player)
    {
        Item.consumable = true;
        if (DoomsdayManager.sunDied || DoomsdayManager.savedEverybody)
        {
            Item.UseSound = SoundID.Item1;
            Item.useTime = 10;
            Item.useAnimation = 10;
        }
        else
        {
            Item.UseSound = SoundID.Item120;
            Item.useTime = 60;
            Item.useAnimation = 60;
        }
    }

    public override bool? UseItem(Player player)
    {
        Item.consumable = !DoomsdayManager.sunDied;
        DoomsdayManager.savedEverybody = true;
        DoomsdayManager.shaderTime = 120;
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return true;
        }
        if (DoomsdayManager.sunDied)
        {
            if (DoomsdayManager.sentTheMessage)
            {
                return true;
            }
            if (Main.dedServ)
            {
                ChatHelper.BroadcastChatMessage(tooLate.ToNetworkText(), new Color(50, 255, 130));
            }
            else
            {
                Main.NewText(tooLate.Value, 50, 255, 130);
            }
            DoomsdayManager.sentTheMessage = true;
            return true;
        }
        if (Main.dedServ)
        {
            ChatHelper.BroadcastChatMessage(apocalypseOver.ToNetworkText(), new Color(50, 255, 130));
            if (DoomsdayClock.TimeLeftInRange(3))
            {
                ChatHelper.BroadcastChatMessage(postApocalypse.ToNetworkText(), new Color(50, 255, 130));
            }
        }
        else
        {
            Main.NewText(apocalypseOver.Value, 50, 255, 130);
            if (DoomsdayClock.TimeLeftInRange(3))
            {
                Main.NewText(postApocalypse.Value, 50, 255, 130);
            }
        }
        return true;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var texture = TextureAssets.Item[Type];

        float num7 = (float)Main.GlobalTimeWrappedHourly * 0.16f;
        float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
        globalTimeWrappedHourly %= 4f;
        globalTimeWrappedHourly /= 2f;
        if (globalTimeWrappedHourly >= 1f)
        {
            globalTimeWrappedHourly = 2f - globalTimeWrappedHourly;
        }
        globalTimeWrappedHourly = globalTimeWrappedHourly * 0.5f + 0.5f;
        //Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = position;

        for (float num8 = 0f; num8 < 1f; num8 += 0.25f)
        {
            spriteBatch.Draw(texture.Value, drawPosition + new Vector2(0f, 4f).RotatedBy((num8 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, frame, new Color(90, 70, 255, 50), 0, origin, scale, SpriteEffects.None, 0f);
        }
        for (float num9 = 0f; num9 < 1f; num9 += 0.34f)
        {
            spriteBatch.Draw(texture.Value, drawPosition + new Vector2(0f, 2f).RotatedBy((num9 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, frame, new Color(140, 120, 255, 77), 0, origin, scale, SpriteEffects.None, 0f);
        }
        return true;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        var texture = TextureAssets.Item[Type];

        float num7 = (float)Main.GlobalTimeWrappedHourly * 0.16f;
        float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
        globalTimeWrappedHourly %= 4f;
        globalTimeWrappedHourly /= 2f;
        if (globalTimeWrappedHourly >= 1f)
        {
            globalTimeWrappedHourly = 2f - globalTimeWrappedHourly;
        }
        globalTimeWrappedHourly = globalTimeWrappedHourly * 0.5f + 0.5f;
        Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
        Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);

        for (float num8 = 0f; num8 < 1f; num8 += 0.25f)
        {
            spriteBatch.Draw(texture.Value, drawPosition + new Vector2(0f, 4f).RotatedBy((num8 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, itemFrame, new Color(90, 70, 255, 50), rotation, drawOrigin, scale, SpriteEffects.None, 0f);
        }
        for (float num9 = 0f; num9 < 1f; num9 += 0.34f)
        {
            spriteBatch.Draw(texture.Value, drawPosition + new Vector2(0f, 2f).RotatedBy((num9 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, itemFrame, new Color(140, 120, 255, 77), rotation, drawOrigin, scale, SpriteEffects.None, 0f);
        }
        spriteBatch.Draw(texture.Value, drawPosition, itemFrame, Color.White, rotation, drawOrigin, scale, SpriteEffects.None, 0);
        return false;
    }
}

public class SunblockDropping : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type != NPCID.MoonLordCore)
        {
            return;
        }
        npcLoot.Add(ItemDropRule.ByCondition(new WorldNotSavedCondition(), ModContent.ItemType<CosmicSunblock>()));
    }

    public class WorldNotSavedCondition : IItemDropRuleCondition
    {
        private static LocalizedText Desc;

        public WorldNotSavedCondition()
        {
            Desc ??= Language.GetOrRegister("Mods.SolarDoomsday.DropConditions.WorldNotSavedCondition");
        }

        public bool CanDrop(DropAttemptInfo info)
        {
            return DoomsdayClock.Ongoing;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return Desc.Value;
        }
    }
}

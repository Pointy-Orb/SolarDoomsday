using Terraria;
using Terraria.ID;
using System;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.IO;
using Microsoft.Xna.Framework;

namespace SolarDoomsday;

public enum DoomsdayOptions
{
    Stagnation,
    Dissipation,
    Nova
}

public class DoomsdayManager : ModSystem
{
    public bool ApocalypseEnabledMenu = false;
    public static bool thisWorldNeverSawTerror = true;

    public static bool RainingAndSafe => Main.raining && ModContent.GetInstance<ServerConfig>().RainGivesSafety;

    public DoomsdayOptions SelectedDoomsdayOption { get; set; } = DoomsdayOptions.Stagnation;
    public static DoomsdayOptions worldEndChoice = DoomsdayOptions.Dissipation;
    public static int chosenDayNumber = 30;

    public static bool sunDied = false;
    public static bool savedEverybody = false;

    public static bool sentTheMessage = false;

    public static int shaderTime = 0;
    public static int novaTime { get; private set; } = 0;

    public static int spookyBackTime = 0;
    private static float effectTransition = 0;

    public static bool ScreenShakeActive { get; private set; } = false;

    public override void Load()
    {
        On_Main.IsItDay += NotDayWhenSunIsDead;
    }

    public override void ClearWorld()
    {
        worldEndChoice = DoomsdayOptions.Dissipation;
        sunDied = false;
        savedEverybody = false;
        sentTheMessage = false;
        thisWorldNeverSawTerror = true;
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        bool spooky = spookyBackTime > 0;
        if (spooky)
        {
            spookyBackTime--;
            effectTransition += 0.05f;
        }
        else
        {
            effectTransition -= 0.05f;
        }

        if (effectTransition > 1)
        {
            effectTransition = 1;
        }
        if (effectTransition < 0)
        {
            effectTransition = 0;
        }

        tileColor = Color.Lerp(tileColor, tileColor.MultiplyRGB(Color.Orange), effectTransition);
        backgroundColor = Color.Lerp(backgroundColor, backgroundColor.MultiplyRGB(Color.DarkRed), effectTransition);
    }

    public override void PostUpdateTime()
    {
        if (shaderTime > 0)
        {
            shaderTime--;
        }
        if (Main.dedServ)
        {
            return;
        }
        Main.LocalPlayer.ManageSpecialBiomeVisuals("SolarDoomsday:BigScaryFlashShader", shaderTime > 60 && (Main.LocalPlayer.ZoneOverworldHeight || Main.LocalPlayer.ZoneSkyHeight || worldEndChoice == DoomsdayOptions.Nova));
        if (!ScreenShakeActive && ScreenShake.ScreenShakeConditions)
        {
            Main.instance.CameraModifiers.Add(new ScreenShake(10, FullName));
            ScreenShakeActive = true;
        }
        if (!ScreenShake.ScreenShakeConditions)
        {
            ScreenShakeActive = false;
        }
    }

    public override void PostUpdateEverything()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }
        if (novaTime > 0)
        {
            novaTime--;
            if (novaTime <= 0)
            {
                DestroyWorldNova();
            }
        }
        if (DoomsdayClock.TimeLeftInRange(6) && Main.spawnTileY < Main.worldSurface + 30 && !sunDied)
        {
            while (Main.spawnTileY < Main.worldSurface + 10 || (Main.spawnTileY < Main.rockLayer && (Main.tile[Main.spawnTileX, Main.spawnTileY - 1].HasTile || !Main.tile[Main.spawnTileX, Main.spawnTileY].HasTile)))
            {
                Main.spawnTileY++;
            }
            for (int i = Main.spawnTileX - 5; i <= Main.spawnTileX + 5; i++)
            {
                for (int j = Main.spawnTileY - 5; j <= Main.spawnTileY + 5; j++)
                {
                    if (!WorldGen.InWorld(i, j))
                    {
                        continue;
                    }
                    Tile tile = Main.tile[i, j];
                    if (AshConversion.CanMelt[tile.TileType])
                    {
                        WorldGen.ConvertTile(i, j, TileID.Dirt);
                    }
                    if (tile.TileType == ModContent.TileType<Content.Tiles.Hornfels>())
                    {
                        WorldGen.ConvertTile(i, j, TileID.Ash);
                    }
                    tile.LiquidAmount = 0;
                }
            }
            for (int i = Main.spawnTileX - 2; i <= Main.spawnTileX + 2; i++)
            {
                for (int j = Main.spawnTileY - 3; j < Main.spawnTileY; j++)
                {
                    WorldGen.KillTile(i, j, noItem: true);
                }
            }
        }
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (worldEndChoice != DoomsdayOptions.Dissipation)
        {
            tag["worldEndChoice"] = (int)worldEndChoice;
        }
        if (sunDied)
        {
            tag["sunDied"] = sunDied;
        }
        if (savedEverybody)
        {
            tag["savedEverybody"] = savedEverybody;
        }
        if (!thisWorldNeverSawTerror)
        {
            tag["knowsTrueFear"] = true;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.ContainsKey("worldEndChoice"))
        {
            worldEndChoice = (DoomsdayOptions)tag.GetAsInt("worldEndChoice");
        }
        sunDied = tag.GetBool("sunDied");
        savedEverybody = tag.GetBool("savedEverybody");
        thisWorldNeverSawTerror = !tag.GetBool("knowsTrueFear");
    }

    public override void PreWorldGen()
    {
        thisWorldNeverSawTerror = !ApocalypseEnabledMenu;
        worldEndChoice = SelectedDoomsdayOption;
        DoomsdayClock.SetDayCount(chosenDayNumber);
    }

    public static void DestroyWorldAccordingToChoice()
    {
        switch (worldEndChoice)
        {
            case DoomsdayOptions.Stagnation:
                break;
            case DoomsdayOptions.Dissipation:
                //SoundEngine.PlaySound(new SoundStyle("SolarDoomsday/Assets/sunsplosion"));
                shaderTime = 440;
                sunDied = true;
                break;
            case DoomsdayOptions.Nova:
                SoundEngine.PlaySound(new SoundStyle("SolarDoomsday/Assets/sunsplosion"));
                sunDied = true;
                novaTime = 400;
                shaderTime = 440;
                break;
        }
    }

    private static void DestroyWorldNova()
    {
        foreach (NPC npc in Main.ActiveNPCs)
        {
            npc.StrikeInstantKill();
        }
        SoundEngine.PlaySound(new SoundStyle("SolarDoomsday/Assets/sunsplosion"));
        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesY - 80 + WorldGen.genRand.Next(-5, 6); j++)
            {
                Tile tile = Main.tile[i, j];
                tile.HasTile = false;
                tile.WallType = 0;
                tile.LiquidAmount = 0;
            }
        }
        Liquid.numLiquid = 0;
        WorldGen.WaterCheck();
        Main.worldSurface = Math.Max(Main.UnderworldLayer - 300, Main.worldSurface);
        Main.rockLayer = Math.Max(Main.UnderworldLayer - 200, Main.rockLayer);
        if (Main.dedServ)
        {
            var chunkSize = 50;
            for (int i = 0; i < Main.maxTilesX; i += chunkSize)
            {
                for (int j = 0; j < Main.maxTilesY; j += chunkSize)
                {
                    var rangeX = chunkSize;
                    var rangeY = chunkSize;
                    if (!WorldGen.InWorld(i + rangeX, j))
                    {
                        rangeX = Main.maxTilesX - i;
                    }
                    if (!WorldGen.InWorld(i, j + rangeY))
                    {
                        rangeY = Main.maxTilesY - j;
                    }
                    NetMessage.SendTileSquare(-1, i, j, rangeX, rangeY);
                }
            }
            NetMessage.SendData(MessageID.WorldData);
        }
        sunDied = true;
        foreach (Player player in Main.ActivePlayers)
        {
            var deathReason = new PlayerDeathReason();
            int message = Main.rand.Next(0, 3);
            deathReason.CustomReason = Language.GetText($"Mods.SolarDoomsday.DeathReasons.Sun{message}").WithFormatArgs(player.name).ToNetworkText();
            player.creativeGodMode = false;
            player.KillMe(deathReason, 999999, 0);
            if (Main.dedServ)
            {
                NetMessage.SendPlayerDeath(player.whoAmI, deathReason, 999999, 0, false);
            }
        }
        var cloudY = Main.spawnTileY;
        while (!Main.tile[Main.spawnTileX, cloudY + 5].HasTile && Main.tile[Main.spawnTileX, cloudY + 5].LiquidAmount <= 0 && cloudY + 5 < Main.maxTilesY)
        {
            cloudY++;
        }
        WorldGen.PlaceTile(Main.spawnTileX, cloudY, TileID.Cloud);
        DoomsdayClock.counterActive = false;
    }

    private static bool NotDayWhenSunIsDead(On_Main.orig_IsItDay orig)
    {
        if (sunDied || Main.eclipse)
        {
            return false;
        }
        return orig();
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(sunDied);
        writer.Write(savedEverybody);
        writer.Write((byte)worldEndChoice);
    }

    public override void NetReceive(BinaryReader reader)
    {
        sunDied = reader.ReadBoolean();
        savedEverybody = reader.ReadBoolean();
        worldEndChoice = (DoomsdayOptions)reader.ReadByte();
    }
}

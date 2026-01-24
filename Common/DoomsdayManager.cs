using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SolarDoomsday;

public enum DoomsdayOptions
{
    Stagnation,
    Dissipation,
    Nova
}

public class DoomsdayManager : ModSystem
{
    public DoomsdayOptions SelectedDoomsdayOption { get; set; } = DoomsdayOptions.Stagnation;
    public static DoomsdayOptions worldEndChoice = DoomsdayOptions.Dissipation;
    public static int chosenDayNumber = 30;

    public static bool sunDied = false;
    public static bool savedEverybody = false;

    public static bool sentTheMessage = false;

    public static int shaderTime = 0;
    private static int novaTime = 0;

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
    }

    public override void PostUpdateTime()
    {
        if (shaderTime > 0)
        {
            shaderTime--;
        }
        if (!Main.dedServ)
        {
            Main.LocalPlayer.ManageSpecialBiomeVisuals("SolarDoomsday:BigScaryFlashShader", shaderTime > 60 && (Main.LocalPlayer.ZoneOverworldHeight || Main.LocalPlayer.ZoneSkyHeight || worldEndChoice == DoomsdayOptions.Nova));
        }
    }

    public override void PostUpdateEverything()
    {
        if (novaTime > 0)
        {
            novaTime--;
            if (novaTime <= 0)
            {
                DestroyWorldNova();
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
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.ContainsKey("worldEndChoice"))
        {
            worldEndChoice = (DoomsdayOptions)tag.GetAsInt("worldEndChoice");
        }
        sunDied = tag.GetBool("sunDied");
        savedEverybody = tag.GetBool("savedEverybody");
    }

    public override void PreWorldGen()
    {
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
                SoundEngine.PlaySound(new SoundStyle("SolarDoomsday/Assets/sunsplosion"));
                shaderTime = 440;
                sunDied = true;
                break;
            case DoomsdayOptions.Nova:
                SoundEngine.PlaySound(new SoundStyle("SolarDoomsday/Assets/sunsplosion"));
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
        Main.worldSurface = Main.UnderworldLayer - 300;
        Main.rockLayer = Main.UnderworldLayer - 200;
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
                RemoteClient.CheckSection(player.whoAmI, player.Center, 50);
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
}

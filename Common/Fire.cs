using Terraria;
using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday;

public struct FireTileData : ITileData
{
    public byte fireAmount;

    public FireTileData()
    {
        fireAmount = 0;
    }
}

public class Fire : ModType
{
    protected sealed override void Register()
    {
        ModTypeLookup<Fire>.Register(this);
    }

    internal const int maxFires = 20000;

    public static Fire[] fires = new Fire[maxFires];
    public static int numFire { get; private set; } = 0;

    private static int updateTimer = 0;

    public int x { get; private set; }
    public int y { get; private set; }
    public int frameX = 162;
    public int frameY = 54;
    public int styleRand = 0;
    public byte FireLevel
    {
        get { return Main.tile[x, y].Get<FireTileData>().fireAmount; }
        set { Main.tile[x, y].Get<FireTileData>().fireAmount = (byte)Math.Max(0, Math.Min((int)byte.MaxValue, (int)value)); }
    }
    public bool kill = false;

    public sealed override void SetupContent()
    {
        SetStaticDefaults();
    }

    public override void SetStaticDefaults()
    {
        for (int i = 0; i < maxFires; i++)
        {
            fires[i] = new Fire();
        }
    }

    public static void FireCheck()
    {
        numFire = 0;
        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesY; j++)
            {
                Tile tile = Main.tile[i, j];
                AddFire(i, j);
            }
        }
    }

    public static void SetOnFire(int i, int j)
    {
        for (int k = i - 1; k <= i + 1; k++)
        {
            for (int l = j - 1; l <= j + 1; l++)
            {
                if (!WorldGen.InWorld(k, l))
                {
                    continue;
                }
                if (Main.tile[k, l].LiquidAmount > 0 && Main.tile[k, l].LiquidType != LiquidID.Lava)
                {
                    return;
                }
            }
        }
        Tile tile = Main.tile[i, j];
        Main.tile[i, j].Get<FireTileData>().fireAmount = 100;
        AddFire(i, j);
    }

    public static void PutOutFire(int x, int y)
    {
        for (int i = 0; i < numFire; i++)
        {
            if (fires[i].x == x && fires[i].y == y)
            {
                DelFire(i);
                return;
            }
        }
    }

    public static void ReframeFire(int x, int y)
    {
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (!WorldGen.InWorld(i, j))
                {
                    continue;
                }
                Tile tile = Main.tile[i, j];
                if (tile.Get<FireTileData>().fireAmount <= 0)
                {
                    continue;
                }
                FireFraming.ReframeFireInner(i, j);
            }
        }
    }

    private static void AddFire(int i, int j)
    {
        if (Main.tile[i, j].Get<FireTileData>().fireAmount <= 0)
        {
            return;
        }
        //TODO: Add a fire buffer
        if (numFire >= maxFires)
        {
            return;
        }
        fires[numFire].x = i;
        fires[numFire].y = j;
        fires[numFire].kill = false;
        fires[numFire].styleRand = Main.rand.Next(3);
        numFire++;
        ReframeFire(i, j);
    }

    private static void DelFire(int f)
    {
        numFire--;
        for (int i = 0; i < 5; i++)
        {
            Dust.NewDust(new Vector2(fires[f].x * 16, fires[f].y * 16), 16, 16, DustID.Torch);
        }
        ReframeFire(fires[f].x, fires[f].y);
        fires[f].x = fires[numFire].x;
        fires[f].y = fires[numFire].y;
        fires[f].kill = false;
    }

    public static void UpdateFire()
    {
        if (updateTimer < 3)
        {
            updateTimer++;
            return;
        }
        updateTimer = 0;

        for (int i = 0; i < numFire; i++)
        {
            fires[i].Update();
            if (fires[i].FireLevel <= 0)
            {
                fires[i].kill = true;
            }
        }
        for (int i = 0; i < numFire;)
        {
            if (fires[i].kill)
            {
                DelFire(i);
            }
            else
            {
                i++;
            }
        }
    }

    private Vector3 LightColor = new(0.85f, 0.5f, 0.3f);
    public void Update()
    {
        Tile tile = Main.tile[x, y];
        Lighting.AddLight(new Point(x, y).ToWorldCoordinates(), LightColor);
        for (int k = x - 1; k <= x + 1; k++)
        {
            for (int l = y - 1; l <= y + 1; l++)
            {
                if (!WorldGen.InWorld(k, l))
                {
                    continue;
                }
                if (Main.tile[k, l].LiquidAmount > 0 && Main.tile[k, l].LiquidType != LiquidID.Lava)
                {
                    FireLevel = 0;
                    SoundEngine.PlaySound(SoundID.LiquidsWaterLava, new Point(x, y).ToWorldCoordinates());
                    return;
                }
            }
        }
        //FireFraming.ReframeFireInner(x, y);
        int fireChangeRate = Math.Max(FlammabilitySystem.Flammability[tile.TileType], FlammabilitySystem.FlammabilityWall[tile.WallType]);
        if (!tile.HasTile && tile.WallType == 0)
        {
            fireChangeRate = -2;
        }

        if ((int)FireLevel + (int)fireChangeRate > 255)
        {
            FireLevel = 255;
        }
        else if ((int)FireLevel + (int)fireChangeRate < 0)
        {
            FireLevel = 0;
        }
        else
        {
            FireLevel += (byte)fireChangeRate;
        }
        if (FireLevel == 255)
        {
            AttemptSpread(x, y);
        }

        if (!Main.rand.NextBool((int)((float)FireLevel / 255f * 16f), 160))
        {
            return;
        }
        int i;
        int j;
        bool fail = false;
        int tries = 0;
        do
        {
            tries++;
            i = Main.rand.Next(-1, 2) + x;
            j = Main.rand.Next(-1, 2) + y;
            if (tries > 40)
            {
                break;
            }
            if (!WorldGen.InWorld(i, j))
            {
                continue;
            }
            if (Main.tile[i, j].Get<FireTileData>().fireAmount > 0)
            {
                fail = true;
            }
        } while ((i == x && j == y) || fail);
        if (!WorldGen.InWorld(i, j) || Main.tile[i, j].Get<FireTileData>().fireAmount > 0)
        {
            return;
        }
        Tile victim = Main.tile[i, j];
        var flammability = Math.Max(FlammabilitySystem.Flammability[victim.TileType], FlammabilitySystem.FlammabilityWall[victim.WallType]);
        if (flammability < 1 || (!victim.HasTile && victim.WallType == 0))
        {
            return;
        }
        SetOnFire(i, j);
    }

    public static bool AttemptSpread(int i, int j)
    {
        var target = Main.tile[i, j];
        var spread = false;
        if (FlammabilitySystem.FlammabilityWall[target.WallType] > 0)
        {
            spread = true;
            WorldGen.KillWall(i, j, true);
            WorldGen.ConvertWall(i, j, 0);
            WorldGen.Reframe(i, j);
            NetMessage.SendTileSquare(-1, i, j, 1, 1);
        }
        bool safeMode = false;
        if (WorldGen.InWorld(i, j - 1) && ((TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Main.tile[i, j - 1].TileType] && !TileID.Sets.IsATreeTrunk[Main.tile[i, j - 1].TileType]) || TileID.Sets.BasicChest[Main.tile[i, j - 1].TileType]))
        {
            safeMode = true;
        }
        if (target.TileType == TileID.Explosives)
        {
            WorldGen.KillTile(i, j, fail: false, effectOnly: false, noItem: true);
            NetMessage.SendTileSquare(-1, i, j);
            Projectile.NewProjectile(Wiring.GetProjectileSource(i, j), i * 16 + 8, j * 16 + 8, 0f, 0f, 108, 500, 10f);
        }
        if (FlammabilitySystem.Flammability[target.TileType] > 0)
        {
            spread = true;
            if (safeMode)
            {
                target.TileType = TileID.Ash;
            }
            else
            {
                WorldGen.KillTile(i, j, noItem: true);
            }
            WorldGen.Reframe(i, j);
            NetMessage.SendTileSquare(-1, i, j, 1, 1);
        }
        if (TileID.Sets.Grass[target.TileType] || TileID.Sets.Snow[target.TileType])
        {
            spread = true;
            WorldGen.ConvertTile(i, j, TileID.Dirt);
        }
        if (WallID.Sets.Conversion.Grass[target.WallType] || target.WallType == WallID.SnowWallUnsafe)
        {
            WorldGen.ConvertWall(i, j, WallID.DirtUnsafe);
        }
        return spread;
    }
}

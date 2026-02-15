using Terraria;
using System.IO;
using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday;

//If it weren't for Lion8Cake I wouldn't have even known this was an option
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
        for (int i = 0; i < numFire; i++)
        {
            ReframeFire(fires[i].x, fires[i].y);
        }
    }

    public static void SetOnFire(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        for (int k = i - 1; k <= i + 1; k++)
        {
            for (int l = j - 1; l <= j + 1; l++)
            {
                if (!WorldGen.InWorld(k, l))
                {
                    continue;
                }
                if (Main.tile[k, l].LiquidAmount > 0 && Main.tile[k, l].LiquidType != LiquidID.Lava && Main.tile[k, l].LiquidType != LiquidID.Honey)
                {
                    return;
                }
            }
        }
        bool igniting = tile.Get<FireTileData>().fireAmount <= 0;
        tile.Get<FireTileData>().fireAmount = (byte)Math.Max(tile.Get<FireTileData>().fireAmount, (byte)100);
        if (igniting)
        {
            AddFire(i, j);
            SolarDoomsday.RemoteSetFire(i, j);
        }
    }

    public static void PutOutFire(int x, int y)
    {
        for (int i = 0; i < numFire; i++)
        {
            if (fires[i].x == x && fires[i].y == y)
            {
                Main.tile[x, y].Get<FireTileData>().fireAmount = 0;
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
    }

    private static void DelFire(int f)
    {
        numFire--;
        if (Main.dedServ)
        {
            SolarDoomsday.RemoteDelFire(fires[f].x, fires[f].y);
        }
        for (int i = 0; i < 5; i++)
        {
            Dust.NewDust(new Vector2(fires[f].x * 16, fires[f].y * 16), 16, 16, DustID.Torch);
        }
        fires[f].x = fires[numFire].x;
        fires[f].y = fires[numFire].y;
        fires[f].kill = false;
    }

    private static int prevNumFire;
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
            Lighting.AddLight(new Point(fires[i].x, fires[i].y).ToWorldCoordinates(), LightColor);
        }
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
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
        if (numFire == prevNumFire)
        {
            prevNumFire = numFire;
            return;
        }
        for (int i = 0; i < numFire; i++)
        {
            FireFraming.ReframeFireInner(fires[i].x, fires[i].y);
        }
        prevNumFire = numFire;
    }

    private static readonly Vector3 LightColor = new(0.85f, 0.5f, 0.3f);
    public void Update()
    {
        Tile tile = Main.tile[x, y];
        for (int k = x - 1; k <= x + 1; k++)
        {
            for (int l = y - 1; l <= y + 1; l++)
            {
                if (!WorldGen.InWorld(k, l))
                {
                    continue;
                }
                if (Main.tile[k, l].LiquidAmount > 0 && Main.tile[k, l].LiquidType != LiquidID.Lava && Main.tile[k, l].LiquidType != LiquidID.Honey)
                {
                    FireLevel = 0;
                    SoundEngine.PlaySound(SoundID.LiquidsWaterLava, new Point(x, y).ToWorldCoordinates());
                    return;
                }
            }
        }
        if (tile.TileType == TileID.Larva)
        {
            tile.TileType = TileID.HoneyBlock;
            NetMessage.SendTileSquare(-1, x, y);
        }
        //FireFraming.ReframeFireInner(x, y);
        int tileFireChangeRate = tile.HasTile ? FlammabilitySystem.Flammability[tile.TileType] : -10;
        int fireChangeRate = Math.Max(tileFireChangeRate, FlammabilitySystem.FlammabilityWall[tile.WallType]);
        if (!tile.HasTile && tile.WallType == 0)
        {
            fireChangeRate = -2;
        }
        if (!tile.HasTile && tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Honey)
        {
            fireChangeRate = Math.Max(1, fireChangeRate);
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
        bool charred = CharTile(x, y);
        if (!WorldGen.InWorld(i, j) || Main.tile[i, j].Get<FireTileData>().fireAmount > 0)
        {
            return;
        }
        Tile victim = Main.tile[i, j];
        charred |= CharTile(i, j);
        if (charred)
        {
            FireLevel = (byte)Math.Min((int)byte.MaxValue, (int)FireLevel + 30);
        }
        int tileFlammability = victim.HasTile ? FlammabilitySystem.Flammability[victim.TileType] : -10;
        var flammability = Math.Max(tileFlammability, FlammabilitySystem.FlammabilityWall[victim.WallType]);
        bool tileHoney = victim.LiquidAmount > 0 && victim.LiquidType == LiquidID.Honey;
        flammability = Math.Max(flammability, tileHoney ? 1 : flammability);
        if (flammability < 1 || (!victim.HasTile && victim.WallType == 0 && !tileHoney))
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
            WorldGen.ConvertWall(i, j, 0);
            WorldGen.Reframe(i, j);
        }
        if (!target.HasTile && target.LiquidAmount > 0 && target.LiquidType == LiquidID.Honey)
        {
            spread = true;
            WorldGen.PlaceTile(i, j, TileID.CrispyHoneyBlock, true);
            target.LiquidAmount = 0;
        }
        if (target.HasTile && target.TileType == TileID.HoneyBlock)
        {
            spread = true;
            target.TileType = TileID.CrispyHoneyBlock;
            WorldGen.Reframe(i, j);
        }
        bool safeMode = false;
        if (WorldGen.InWorld(i, j - 1) && ((TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Main.tile[i, j - 1].TileType] && !TileID.Sets.IsATreeTrunk[Main.tile[i, j - 1].TileType]) || TileID.Sets.BasicChest[Main.tile[i, j - 1].TileType]))
        {
            safeMode = true;
        }
        if (target.TileType == TileID.Explosives)
        {
            spread = true;
            WorldGen.KillTile(i, j, fail: false, effectOnly: false, noItem: true);
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
                target.HasTile = false;
                for (int k = i - 1; k <= i + 1; k++)
                {
                    for (int l = j - 1; l <= j + 1; l++)
                    {
                        if (!WorldGen.InWorld(k, l))
                        {
                            continue;
                        }
                        WorldGen.Reframe(k, l);
                    }
                }
            }
        }
        if (spread)
        {
            BurnAudioVisual(i, j);
            NetMessage.SendTileSquare(-1, i, j, 1, 1);
        }
        return spread;
    }

    public static void BurnAudioVisual(int i, int j)
    {
        if (Main.dedServ)
        {
            SolarDoomsday.FireBurnEffects(i, j);
            return;
        }
        Tile target = Main.tile[i, j];
        SoundEngine.PlaySound(SoundID.Item45, new Point(i, j).ToWorldCoordinates());
        float horiSpeed = 0;
        if (j < Main.worldSurface && target.WallType == 0)
        {
            horiSpeed = Main.windSpeedCurrent * 3;
        }
        for (int l = 0; l < 6; l++)
        {
            var smoke = Dust.NewDustDirect(new Vector2(i, j) * 16, 16, 16, ModContent.DustType<Content.Dusts.BurningSmoke>(), horiSpeed, -1f, 100, Color.Gray);
            smoke.velocity.X = horiSpeed;
            smoke.velocity.Y = -1;
            smoke.noGravity = true;
        }
    }

    public static bool CharTile(int i, int j)
    {
        if (!WorldGen.InWorld(i, j))
        {
            return false;
        }
        var target = Main.tile[i, j];
        bool charred = false;
        if (TileID.Sets.Grass[target.TileType] || TileID.Sets.Snow[target.TileType])
        {
            WorldGen.ConvertTile(i, j, TileID.Dirt);
            charred = true;
        }
        if (WallID.Sets.Conversion.Grass[target.WallType] || target.WallType == WallID.SnowWallUnsafe)
        {
            WorldGen.ConvertWall(i, j, WallID.DirtUnsafe);
            charred = true;
        }
        if (charred)
        {
            NetMessage.SendTileSquare(-1, i, j, 1, 1);
        }
        return charred;
    }

    //Syncs data at ever tile position in a chunk
    //LIMIT THIS TO VERY SMALL AMOUNTS OF DATA, THE MORE DATA THE LONGER THE CHUNK TAKES TO LOAD
    public static void SendTileData(Tile tile, BinaryWriter writer)
    {
        writer.Write(tile.Get<FireTileData>().fireAmount);
    }

    public static void RecieveTileData(Tile tile, BinaryReader reader, int i, int j)
    {
        bool wasOnFire = tile.Get<FireTileData>().fireAmount > 0;
        tile.Get<FireTileData>().fireAmount = reader.ReadByte();
        if (tile.Get<FireTileData>().fireAmount > 0 && !wasOnFire)
        {
            AddFire(i, j);
        }
    }
}

using Terraria;
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

    public int x { get; private set; }
    public int y { get; private set; }
    public byte FireLevel
    {
        get { return Main.tile[x, y].Get<FireTileData>().fireAmount; }
        set { Main.tile[x, y].Get<FireTileData>().fireAmount = value; }
    }
    public bool kill = false;

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
        Tile tile = Main.tile[i, j];
        Main.tile[i, j].Get<FireTileData>().fireAmount = 255;
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
        numFire++;
    }

    private static void DelFire(int f)
    {
        numFire--;
        for (int i = 0; i < 5; i++)
        {
            Dust.NewDust(new Vector2(fires[f].x * 16, fires[f].y * 16), 16, 16, DustID.Torch);
        }
        fires[f].x = fires[numFire].x;
        fires[f].y = fires[numFire].y;
    }

    public static void UpdateFire()
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

    public void Update()
    {
        Tile tile = Main.tile[x, y];
        if (!tile.HasTile && tile.WallType == 0)
        {
            FireLevel = 0;
        }
        if (tile.LiquidAmount > 0)
        {
            FireLevel = 0;
            SoundEngine.PlaySound(SoundID.LiquidsWaterLava, new Point(x, y).ToWorldCoordinates());
        }
        if (FireLevel <= 0)
        {
            return;
        }
        Main.NewText(FireLevel);
    }
}

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SolarDoomsday;

//I get the feeling that people are going to compare me to YandereDev after seeing this script...
public class FireFraming : ModSystem
{
    private static Fire[] fires => Fire.fires;

    internal static void ReframeFireInner(int x, int y)
    {
        int thisFire = -1;
        for (int i = 0; i < Fire.numFire; i++)
        {
            var fire = fires[i];
            if (fire.x == x && fire.y == y)
            {
                thisFire = i;
                break;
            }
        }
        if (thisFire < 0)
        {
            return;
        }
        bool[,] fireNeighbors = new bool[3, 3];
        //-1 and not zero because the center tile will count as a neighbor and it should be offset.
        int neighborCount = -1;
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (!WorldGen.InWorld(i, j))
                {
                    continue;
                }
                Tile neighbor = Main.tile[i, j];
                if (neighbor.Get<FireTileData>().fireAmount <= 0)
                {
                    continue;
                }
                fireNeighbors[i - x + 1, j - y + 1] = true;
                neighborCount++;
            }
        }
        Tile tile = Main.tile[x, y];
        switch (neighborCount)
        {
            case 0:
                FrameFireSingle(thisFire);
                break;
            case 1:
                FramingSingleNeighbor(fireNeighbors, thisFire);
                break;
            case 2:
                FramingTwoNeighbor(fireNeighbors, thisFire);
                break;
            case 3:
                FramingThreeNeighbor(fireNeighbors, thisFire);
                break;
            case 4:
                FramingFourNeighbor(fireNeighbors, thisFire);
                break;
            case 5:
            case 6:
                FramingFiveNeighbor(fireNeighbors, thisFire);
                break;
            case 7:
                FramingFiveNeighbor(fireNeighbors, thisFire);
                break;
            case 8:
                FrameFireSurrounded(thisFire);
                break;
        }
    }

    private static void FramingSingleNeighbor(bool[,] fireNeighbors, int thisFire)
    {
        if (fireNeighbors[0, 1])
        {
            FireFrameNub(thisFire, Direction.Right);
        }
        else if (fireNeighbors[1, 0])
        {
            FireFrameNub(thisFire, Direction.Down);
        }
        else if (fireNeighbors[1, 2])
        {
            FireFrameNub(thisFire, Direction.Up);
        }
        else if (fireNeighbors[2, 1])
        {
            FireFrameNub(thisFire, Direction.Left);
        }
        else
        {
            FrameFireSingle(thisFire);
        }
    }

    private static void FramingTwoNeighbor(bool[,] fireNeighbors, int thisFire)
    {
        Tile tile = Main.tile[fires[thisFire].x, fires[thisFire].y];
        if (fireNeighbors[1, 0] && fireNeighbors[1, 2])
        {
            fires[thisFire].frameX = 90;
            fires[thisFire].frameY = 18 * fires[thisFire].styleRand;
        }
        else if (fireNeighbors[0, 1] && fireNeighbors[2, 1])
        {
            fires[thisFire].frameX = 108 + 18 * fires[thisFire].styleRand;
            fires[thisFire].frameY = 72;
        }
        else if (fireNeighbors[1, 0] && fireNeighbors[2, 1])
        {
            FireFrameCorner(thisFire, Corner.BottomLeft);
        }
        else if (fireNeighbors[1, 0] && fireNeighbors[0, 1])
        {
            FireFrameCorner(thisFire, Corner.BottomRight);
        }
        else if (fireNeighbors[1, 2] && fireNeighbors[2, 1])
        {
            FireFrameCorner(thisFire, Corner.TopLeft);
        }
        else if (fireNeighbors[1, 2] && fireNeighbors[0, 1])
        {
            FireFrameCorner(thisFire, Corner.TopRight);
        }
        else
        {
            FramingSingleNeighbor(fireNeighbors, thisFire);
        }
    }

    private static void FramingThreeNeighbor(bool[,] fireNeighbors, int thisFire)
    {
        if (fireNeighbors[1, 0] && fireNeighbors[1, 2] && fireNeighbors[0, 1])
        {
            FireFrameSide(thisFire, Direction.Left);
        }
        else if (fireNeighbors[1, 0] && fireNeighbors[1, 2] && fireNeighbors[2, 1])
        {
            FireFrameSide(thisFire, Direction.Right);
        }
        else if (fireNeighbors[1, 0] && fireNeighbors[0, 1] && fireNeighbors[2, 1])
        {
            FireFrameSide(thisFire, Direction.Down);
        }
        else if (fireNeighbors[1, 2] && fireNeighbors[0, 1] && fireNeighbors[2, 1])
        {
            FireFrameSide(thisFire, Direction.Up);
        }
        else if (fireNeighbors[0, 0] && fireNeighbors[0, 1] && fireNeighbors[0, 2])
        {
            FireFrameNub(thisFire, Direction.Right);
        }
        else if (fireNeighbors[2, 0] && fireNeighbors[2, 1] && fireNeighbors[2, 2])
        {
            FireFrameNub(thisFire, Direction.Left);
        }
        else if (fireNeighbors[0, 0] && fireNeighbors[1, 0] && fireNeighbors[2, 0])
        {
            FireFrameNub(thisFire, Direction.Down);
        }
        else if (fireNeighbors[0, 2] && fireNeighbors[1, 2] && fireNeighbors[2, 2])
        {
            FireFrameNub(thisFire, Direction.Up);
        }
        else
        {
            FramingTwoNeighbor(fireNeighbors, thisFire);
        }
    }

    private static void FramingFourNeighbor(bool[,] fireNeighbors, int thisFire)
    {
        if (fireNeighbors[1, 0] && fireNeighbors[1, 2] && fireNeighbors[0, 1] && fireNeighbors[2, 1])
        {
            FrameFireSurrounded(thisFire);
        }
        else if (fireNeighbors[0, 0] && fireNeighbors[0, 1] && fireNeighbors[0, 2] && fireNeighbors[1, 2])
        {
            FireFrameCorner(thisFire, Corner.TopRight);
        }
        else if (fireNeighbors[0, 0] && fireNeighbors[0, 1] && fireNeighbors[0, 2] && fireNeighbors[1, 0])
        {
            FireFrameCorner(thisFire, Corner.BottomRight);
        }
        else if (fireNeighbors[2, 0] && fireNeighbors[2, 1] && fireNeighbors[2, 2] && fireNeighbors[1, 0])
        {
            FireFrameCorner(thisFire, Corner.BottomLeft);
        }
        else if (fireNeighbors[2, 0] && fireNeighbors[2, 1] && fireNeighbors[2, 2] && fireNeighbors[1, 2])
        {
            FireFrameCorner(thisFire, Corner.TopLeft);
        }
        else
        {
            FramingThreeNeighbor(fireNeighbors, thisFire);
        }
    }

    private static void FramingFiveNeighbor(bool[,] fireNeighbors, int thisFire)
    {
        if (!fireNeighbors[0, 0] && !fireNeighbors[0, 1] && !fireNeighbors[0, 2])
        {
            FireFrameSide(thisFire, Direction.Right);
        }
        else if (!fireNeighbors[2, 0] && !fireNeighbors[2, 1] && !fireNeighbors[2, 2])
        {
            FireFrameSide(thisFire, Direction.Left);
        }
        else if (!fireNeighbors[0, 0] && !fireNeighbors[1, 0] && !fireNeighbors[2, 0])
        {
            FireFrameSide(thisFire, Direction.Up);
        }
        else if (!fireNeighbors[0, 2] && !fireNeighbors[1, 2] && !fireNeighbors[2, 2])
        {
            FireFrameSide(thisFire, Direction.Down);
        }
        else
        {
            FramingFourNeighbor(fireNeighbors, thisFire);
        }
    }

    private static void FrameFireSingle(int thisFire)
    {
        fires[thisFire].frameX = 162 + 18 * fires[thisFire].styleRand;
        fires[thisFire].frameY = 54;
    }

    private static void FrameFireSurrounded(int thisFire)
    {
        fires[thisFire].frameX = 18 + 18 * fires[thisFire].styleRand;
        fires[thisFire].frameY = 18;
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private enum Corner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private static void FireFrameNub(int thisFire, Direction type)
    {
        Tile tile = Main.tile[fires[thisFire].x, fires[thisFire].y];
        switch (type)
        {
            case Direction.Down:
                fires[thisFire].frameX = 108 + 18 * fires[thisFire].styleRand;
                fires[thisFire].frameY = 54;
                break;
            case Direction.Right:
                fires[thisFire].frameX = 216;
                fires[thisFire].frameY = 18 * fires[thisFire].styleRand;
                break;
            case Direction.Up:
                fires[thisFire].frameX = 108 + 18 * fires[thisFire].styleRand;
                fires[thisFire].frameY = 0;
                break;
            case Direction.Left:
                fires[thisFire].frameX = 162;
                fires[thisFire].frameY = 18 * fires[thisFire].styleRand;
                break;
        }
    }

    private static void FireFrameCorner(int thisFire, Corner type)
    {
        Tile tile = Main.tile[fires[thisFire].x, fires[thisFire].y];
        fires[thisFire].frameX = 36 * fires[thisFire].styleRand;
        fires[thisFire].frameY = 54;
        if (type == Corner.TopRight || type == Corner.BottomRight)
        {
            fires[thisFire].frameX += 18;
        }
        if (type == Corner.BottomLeft || type == Corner.BottomRight)
        {
            fires[thisFire].frameY += 18;
        }
    }

    private static void FireFrameSide(int thisFire, Direction type)
    {
        switch (type)
        {
            case Direction.Left:
                fires[thisFire].frameX = 72;
                break;
            case Direction.Right:
                fires[thisFire].frameX = 0;
                break;
            case Direction.Up:
            case Direction.Down:
                fires[thisFire].frameX = 18 + 18 * fires[thisFire].styleRand;
                break;
        }
        switch (type)
        {
            case Direction.Left:
            case Direction.Right:
                fires[thisFire].frameY = 18 * fires[thisFire].styleRand;
                break;
            case Direction.Up:
                fires[thisFire].frameY = 0;
                break;
            case Direction.Down:
                fires[thisFire].frameY = 36;
                break;
        }
    }
}

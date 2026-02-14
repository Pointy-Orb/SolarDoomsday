using Terraria;
using Terraria.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace SolarDoomsday;

public class FireManager : ModSystem
{
    public override void PostUpdateWorld()
    {
        Fire.UpdateFire();
    }

    public override void PostDrawTiles()
    {
        Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
        for (int i = 0; i < Fire.numFire; i++)
        {
            DrawFire(Fire.fires[i].x, Fire.fires[i].y, Fire.fires[i], Main.spriteBatch);
        }
        Main.spriteBatch.End();
    }

    private Vector3 LightColor = new(0.85f, 0.5f, 0.3f);
    private void DrawFire(int i, int j, Fire fire, SpriteBatch spriteBatch)
    {
        var pos = new Vector2(i, j) * 16 - Main.screenPosition;
        if (pos.X > Main.screenWidth || pos.Y > Main.screenHeight)
        {
            return;
        }
        if (!TextureAssets.Tile[TileID.LivingFire].IsLoaded)
        {
            Main.instance.LoadTiles(TileID.LivingFire);
        }
        Texture2D texture = TextureAssets.Tile[TileID.LivingFire].Value;
        var rect = new Rectangle(fire.frameX, fire.frameY + Main.tileFrame[TileID.LivingFire] * 90, 16, 16);
        spriteBatch.Draw(texture, pos, rect, Color.White * 0.6f);
    }
}

/*
public class FireVisuals : GlobalTile
{
    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        if (tile.Get<FireTileData>().fireAmount <= 0)
        {
            return;
        }
        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
        if (!TextureAssets.Tile[TileID.LivingFire].IsLoaded)
        {
            Main.instance.LoadTiles(TileID.LivingFire);
        }
        Texture2D texture = TextureAssets.Tile[TileID.LivingFire].Value;
        int thisFire = -1;
        for (int l = 0; l < Fire.numFire; l++)
        {
            if (Fire.fires[l].x != i || Fire.fires[l].y != j)
            {
                continue;
            }
            thisFire = l;
            break;
        }
        var rect = new Rectangle(Fire.frameX + 18 * Fire.fires[thisFire].styleRand, Fire.frameY + Main.tileFrame[TileID.LivingFire] * 90, 16, 16);
        spriteBatch.Draw(texture, (new Vector2(i, j) * 16) - Main.screenPosition + zero, rect, Color.White);
    }
}
*/

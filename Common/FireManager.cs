using Terraria;
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
}

public class FireVisuals : GlobalTile
{
    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        if (tile.Get<FireTileData>().fireAmount <= 0)
        {
            return;
        }
        if (!TextureAssets.Tile[TileID.LivingFire].IsLoaded)
        {
            Main.instance.LoadTiles(TileID.LivingFire);
        }
        Texture2D texture = TextureAssets.Tile[TileID.LivingFire].Value;
        //TODO: Replace this with proper fire framing
        var rect = new Rectangle(tile.TileFrameX, tile.TileFrameY + Main.tileFrame[TileID.LivingFire] * 38, 16, 16);
        spriteBatch.Draw(texture, new Vector2(i, j) * 16, rect, Color.White);
    }
}

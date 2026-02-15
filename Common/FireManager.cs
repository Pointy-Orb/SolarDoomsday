using Terraria;
using System.IO;
using System;
using Terraria.Map;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.IO;

namespace SolarDoomsday;

public class FireManager : ModSystem
{
    public override void Load()
    {
        IL_MapHelper.CreateMapTile += MapEdit;
        IL_NetMessage.CompressTileBlock_Inner += SendTileData;
        IL_NetMessage.DecompressTileBlock_Inner += RecieveTileData;
    }

    public override void PostUpdateTime()
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

    private void MapEdit(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            int mapBaseTypeIndex = 3;
            c.GotoNext(i => i.MatchLdsfld("Terraria.Map.MapHelper", "hellPosition"));
            c.GotoNext(i => i.MatchStloc(out mapBaseTypeIndex));
            c.GotoNext(MoveType.After, i => i.MatchLdloc(mapBaseTypeIndex));
            c.Emit(Ldarg_0);
            c.Emit(Ldarg_1);
            c.EmitDelegate<Func<int, int, int, int>>((int mapBaseType, int i, int j) =>
            {
                Tile tile = Main.tile[i, j];
                if (tile.Get<FireTileData>().fireAmount > 0)
                {
                    return MapHelper.tileLookup[TileID.LivingFire];
                }
                return mapBaseType;
            });
        }
        catch
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    public override void SaveWorldData(TagCompound tag)
    {
        SaveUnsafeTileData(tag);
    }
    public override void LoadWorldData(TagCompound tag)
    {
        LoadUnsafeTileData(tag);
        Fire.FireCheck();
    }

    public static bool failedUnsafeLoad = false;
    private unsafe void SaveUnsafeTileData(TagCompound tag) //"Copied from Starlight River's AuroraWaterSystem.cs because I havent worked with pointers much" > Copied from AvalonWorld because I also haven't worked with pointers much
    {
        if (failedUnsafeLoad)
        {
            Mod.Logger.Info("Did not save SolarDoomsday:TileData as it previously failed to load.");
            failedUnsafeLoad = false;
            return;
        }

        FireTileData[] myData = Main.tile.GetData<FireTileData>();
        byte[] data = new byte[myData.Length];

        fixed (FireTileData* ptr = myData)
        {
            byte* bytePtr = (byte*)ptr;
            var span = new Span<byte>(bytePtr, myData.Length);
            var target = new Span<byte>(data);
            span.CopyTo(target);
        }
        tag.Add("SolarDoomsday:TileData", data);
    }
    private unsafe void LoadUnsafeTileData(TagCompound tag) //"Copied from Starlight River's AuroraWaterSystem.cs because I havent worked with pointers much" > Copied from AvalonWorld because I also haven't worked with pointers much
    {
        FireTileData[] targetData = Main.tile.GetData<FireTileData>();
        byte[] data = tag.GetByteArray("SolarDoomsday:TileData");

        if (targetData.Length != data.Length)
        {
            Mod.Logger.Error($"Failed to load SolarDoomsday:TileData raw data, saved data was of incorrect size. Loaded data was {data.Length}, expected {targetData.Length}. SolarDoomsday:TileData will not be loaded.");
            failedUnsafeLoad = true;
            return;
        }

        fixed (FireTileData* ptr = targetData)
        {
            byte* bytePtr = (byte*)ptr;
            var span = new Span<byte>(bytePtr, targetData.Length);
            var target = new Span<byte>(data);
            target.CopyTo(span);
        }
    }

    //This method and the one below it were both taken from Avalon with permission from Lion8Cake (thx Lion8Cake)
    private void RecieveTileData(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoNext(MoveType.Before, i => i.MatchLdsfld<Main>(nameof(Main.sectionManager)));
        c.EmitLdarg(0);
        c.EmitLdarg(1);
        c.EmitLdarg(2);
        c.EmitLdarg(3);
        c.EmitLdarg(4);
        c.EmitDelegate((BinaryReader reader, int xStart, int yStart, int width, int height) =>
        {
            for (int i = yStart; i < yStart + height; i++)
            {
                for (int j = xStart; j < xStart + width; j++)
                {
                    Tile tile = Main.tile[j, i];
                    Fire.RecieveTileData(tile, reader, j, i);
                }
            }
        });
    }

    private void SendTileData(ILContext il)
    {
        ILCursor c = new(il);
        while (c.TryGotoNext(MoveType.Before, i => i.MatchRet())) ;
        c.EmitLdarg(0);
        c.EmitLdarg(1);
        c.EmitLdarg(2);
        c.EmitLdarg(3);
        c.EmitLdarg(4);
        c.EmitDelegate((System.IO.BinaryWriter writer, int xStart, int yStart, int width, int height) =>
        {
            for (int i = yStart; i < yStart + height; i++)
            {
                for (int j = xStart; j < xStart + width; j++)
                {
                    Tile tile2 = Main.tile[j, i];
                    Fire.SendTileData(tile2, writer);
                }
            }
        });
    }
}

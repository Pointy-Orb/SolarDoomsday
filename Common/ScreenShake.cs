using Terraria;
using Microsoft.Xna.Framework;
using System;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;

namespace SolarDoomsday;

public class ScreenShake : ICameraModifier
{
    private float _shakeStrengthMax = 0;
    private float _shakeStrength = 5;
    private const int framesTotal = 200;
    private int framesElapsed;
    //const int durationMultiplier = 3;

    public static bool ScreenShakeConditions => !Main.gameMenu && DoomsdayManager.worldEndChoice == DoomsdayOptions.Nova && (DoomsdayClock.LastDay || DoomsdayManager.novaTime > 0);

    public string UniqueIdentity { get; private set; }
    public bool Finished { get; private set; }

    public void Update(ref CameraInfo cameraInfo)
    {
        if (_shakeStrength <= 0 || Main.gameMenu)
        {
            Finished = true;
            return;
        }
        if (framesElapsed >= framesTotal)
        {
            framesElapsed = 0;
        }
        if (ScreenShakeConditions)
        {
            _shakeStrength = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 4.5f, 16.5f, 1, _shakeStrengthMax);
            var yLevel = Main.LocalPlayer.position.ToTileCoordinates().Y;
            _shakeStrength = Utils.Remap(yLevel, 0, Main.maxTilesY, _shakeStrength, 0);
        }
        float progress = Utils.GetLerpValue(0, framesTotal, framesElapsed);
        progress -= (float)((int)(progress / 0.025f)) * 0.025f;
        float lerpAmount = Utils.Remap(progress, 0, 0.025f, -1, 1);
        var targetPos = new Vector2(cameraInfo.CameraPosition.X, cameraInfo.CameraPosition.Y + _shakeStrength);
        cameraInfo.CameraPosition = Vector2.Lerp(cameraInfo.CameraPosition, targetPos, lerpAmount * ModContent.GetInstance<ClientConfig>().ScreenShakeStrength);
        if (!Main.gameInactive && !Main.gamePaused)
        {
            framesElapsed++;
            if (!ScreenShakeConditions)
            {
                _shakeStrength--;
            }
        }
    }

    public ScreenShake(int shakeStrength, string uniqueIdentity = null)
    {
        _shakeStrengthMax = shakeStrength;
        UniqueIdentity = uniqueIdentity;
    }
}


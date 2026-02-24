using System;
using Terraria.ID;
using Terraria.Audio;
using Terraria;
using Terraria.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using Terraria.UI;
using Terraria.UI.Gamepad;
using static Mono.Cecil.Cil.OpCodes;

namespace SolarDoomsday;

//To anyone reading this, a lot of this was only possible because some people much smarter than me already did it, and I was able to follow their example.
//Check out the Depths and Confection mods if you haven't already.
//TODO: Add IL gamepad support
public class WorldGenPage : ModSystem
{
    internal static WorldCreationVars vars = new();

    static UIState worldCreationState;

    static UICharacterNameButton characterNameButton;

    private static readonly GroupOptionButton<DoomsdayOptions>[] DoomsdayButtons =
        new GroupOptionButton<DoomsdayOptions>[Enum.GetValues<DoomsdayOptions>().Length];

    private static GroupOptionButton<Enabling> enableButton;

    public override void Load()
    {
        IL_UIWorldCreation.BuildPage += BiggerBuildPage;
        IL_UIWorldCreation.MakeInfoMenu += CustomInfoMenu;
        IL_UIWorldCreation.ShowOptionDescription += ShowModOptionDescription;
        //IL_UIWorldCreation.SetupGamepadPoints += ModSetUpGamepadPoints;

        //On_UIWorldListItem.DrawSelf += DrawWorldSelectIcon;
        On_UIWorldCreation.SetDefaultOptions += OnSetDefaults;
    }

    public override void Unload()
    {
        //On_UIWorldListItem.DrawSelf -= DrawWorldSelectIcon;
    }

    private static void BiggerBuildPage(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(i => i.MatchStloc0());
        c.Emit(Ldc_I4, 48);
        c.Emit(Add);
    }

    private static void CustomInfoMenu(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext((i => i.MatchLdstr("evil")));
        c.GotoNext(i => i.MatchLdloc1());
        int spacingStart = c.Index;
        c.GotoNext(i => i.MatchCall(out _));
        int spacingEnd = c.Index + 1;

        c.Index = c.Instrs.Count - 1;
        c.GotoPrev(i => i.MatchLdcR4(48));
        c.GotoNext(i => i.MatchCall(out _));
        c.Index++;

        c.Emit(Ldarg_0);
        c.Emit(Ldloc_0);
        c.Emit(Ldloc_1);
        c.Emit(Ldloc, 10);
        c.EmitDelegate(
            (
                UIWorldCreation self,
                UIElement container,
                float accumulatedHeight,
                float usableWidthPercent
            ) =>
                AddEnableButton(
                    self,
                    container,
                    accumulatedHeight,
                    ClickEnableButton,
                    "doomsdayEnable",
                    usableWidthPercent
                )
        );

        c.Instrs.InsertRange(c.Index, c.Instrs.ToArray()[spacingStart..spacingEnd]);
    }

    private static void ShowModOptionDescription(ILContext il)
    {
        var c = new ILCursor(il);

        c.Index = c.Instrs.Count - 1;
        c.GotoPrev(i => i.MatchBrfalse(out _));

        c.Emit(Pop);
        c.Emit(Ldloc_0);
        c.Emit(Ldarg_2);
        c.EmitDelegate(
            (LocalizedText localizedText, UIElement listeningElement) =>
                listeningElement is not GroupOptionButton<DoomsdayOptions> doomsdayButton
                    ? (listeningElement is not GroupOptionButton<Enabling> enabling ? localizedText : enabling.Description)
                    : doomsdayButton.Description
        );
        c.Emit(Stloc_0);
        c.Emit(Ldloc_0);
    }

    //With the other ones I at least tried to understand what made them tick so they could work more to my tastes
    //Not with this method. By this point I was just done with it. Enjoy some brainlessly retyped code!
    private static void ModSetUpGamepadPoints(ILContext il)
    {
        var c = new ILCursor(il);
        List<SnapPoint> snapGroupInfectFirst = null;
        UILinkPoint[] arrayIF = null;

        c.GotoNext(i => i.MatchLdarg0());
        c.GotoNext(i => i.MatchLdloc1());
        c.GotoNext(i => i.MatchLdstr("evil"));
        c.GotoNext(i => i.MatchCall<UIWorldCreation>("GetSnapGroup"));
        c.GotoNext(MoveType.After, i => i.MatchStloc(10));

        c.Emit(Ldloc_1);
        c.EmitDelegate(
            (List<SnapPoint> snapPoints) =>
            {
                snapGroupInfectFirst = GetSnapGroup(snapPoints, "infectfirst");
            }
        );
        c.GotoNext(i => i.MatchLdloc(26));
        c.GotoNext(i => i.MatchLdloc(10));
        c.GotoNext(i => i.MatchCallvirt<List<SnapPoint>>("get_Count"), i => i.MatchBlt(out _));
        c.Emit(Ldloc_0);
        c.Emit(Ldloc, 12);
        c.EmitDelegate(
            (int num, UILinkPoint uiLinkPoint) =>
            {
                arrayIF = new UILinkPoint[snapGroupInfectFirst.Count];
                for (int l = 0; l < snapGroupInfectFirst.Count; l++)
                {
                    UILinkPointNavigator.SetPosition(num, snapGroupInfectFirst[l].Position);
                    uiLinkPoint = UILinkPointNavigator.Points[num];
                    uiLinkPoint.Unlink();
                    arrayIF[l] = uiLinkPoint;
                    num++;
                }
            }
        );
        c.GotoNext(i => i.MatchLdloc(28));
        c.GotoNext(i => i.MatchLdloc((20)));
        c.GotoNext(i => i.MatchLdlen());
        c.GotoNext(i => i.MatchConvI4());
        c.GotoNext(MoveType.After, i => i.MatchBlt(out _));

        c.Emit(Ldloc, 20);
        c.Emit(Ldloc, 12);
        c.EmitDelegate(
            (UILinkPoint[] array3, UILinkPoint uiLinkPoint2) =>
            {
                LoopHorizontalLineLinks(arrayIF);
                EstablishUpDownRelationship(array3, arrayIF);
                for (int n = 0; n < arrayIF.Length; n++)
                {
                    arrayIF[n].Down = uiLinkPoint2.ID;
                }
            }
        );
        c.GotoNext(i => i.MatchLdloc(12));
        c.GotoNext(i => i.MatchLdloc(20));
        c.GotoNext(i => i.MatchLdcI4(0));
        c.GotoNext(i => i.MatchLdelemRef());
        c.GotoNext(i => i.MatchLdfld<UILinkPoint>("ID"));
        c.GotoNext(MoveType.After, i => i.MatchStfld<UILinkPoint>("Up"));

        c.Emit(Ldloc, 20);
        c.Emit(Ldloc, 13);
        c.Emit(Ldloc, 12);
        c.EmitDelegate(
            (UILinkPoint[] array3, UILinkPoint uiLinkPoint3, UILinkPoint uiLinkPoint2) =>
            {
                array3[^1].Down = arrayIF[^1].ID;
                arrayIF[^1].Down = uiLinkPoint3.ID;
                uiLinkPoint3.Up = arrayIF[^1].ID;
                uiLinkPoint2.Up = arrayIF[0].ID;
            }
        );
    }

    private static List<SnapPoint> GetSnapGroup(List<SnapPoint> ptsOnPage, string groupName) //Should just reflect the UIWorldCreation.GetSnapGroup method tbh
    {
        List<SnapPoint> list = ptsOnPage.Where((SnapPoint a) => a.Name == groupName).ToList();
        list.Sort(SortPoints);
        return list;
    }

    private static int SortPoints(SnapPoint a, SnapPoint b)
    {
        return a.Id.CompareTo(b.Id);
    }

    private static void LoopHorizontalLineLinks(UILinkPoint[] pointsLine)
    {
        for (int i = 1; i < pointsLine.Length - 1; i++)
        {
            pointsLine[i - 1].Right = pointsLine[i].ID;
            pointsLine[i].Left = pointsLine[i - 1].ID;
            pointsLine[i].Right = pointsLine[i + 1].ID;
            pointsLine[i + 1].Left = pointsLine[i].ID;
        }
    }

    private static void EstablishUpDownRelationship(UILinkPoint[] topSide, UILinkPoint[] bottomSide)
    {
        int num = Math.Max(topSide.Length, bottomSide.Length);
        for (int i = 0; i < num; i++)
        {
            int num2 = Math.Min(i, topSide.Length - 1);
            int num3 = Math.Min(i, bottomSide.Length - 1);
            topSide[num2].Down = bottomSide[num3].ID;
            bottomSide[num3].Up = topSide[num2].ID;
        }
    }

    public void OnSetDefaults(On_UIWorldCreation.orig_SetDefaultOptions orig, UIWorldCreation self)
    {
        orig.Invoke(self);
        ModContent.GetInstance<DoomsdayManager>().SelectedDoomsdayOption = DoomsdayOptions.Stagnation;
        ModContent.GetInstance<DoomsdayManager>().ApocalypseEnabledMenu = false;
        enableButton.SetCurrentOption(Enabling.no);
    }

    public static LocalizedText[] titles =
    {
        Language.GetText(
            SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.Stagnation.Title")
        ),
        Language.GetText(
            SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.Dissipation.Title")
        ),
        Language.GetText(
            SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.Nova.Title")
        ),
        Language.GetText(
            SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.Enabling.Title")
        ),
    };


    public static Asset<Texture2D>[] icons =
    {
        ModContent.Request<Texture2D>("SolarDoomsday/Common/IconStagnation"),
        ModContent.Request<Texture2D>("SolarDoomsday/Common/IconDissipation"),
        ModContent.Request<Texture2D>("SolarDoomsday/Common/IconNova"),
        ModContent.Request<Texture2D>("SolarDoomsday/Common/IconPeaceful"),
        ModContent.Request<Texture2D>("SolarDoomsday/Common/IconGeneral")
    };

    private static void AddDoomsdayOptions(
        UIWorldCreation self,
        UIElement container,
        float accumulatedHeight,
        UIElement.MouseEvent clickEvent,
        string tagGroup,
        float usableWidthPercent
    )
    {
        LocalizedText[] descriptions =
        {
            Language.GetText(
                SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.Stagnation.Description")
            ),
            Language.GetText(
                SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.Dissipation.Description")
            ),
            Language.GetText(
                SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.Nova.Description")
            ),
        };
        Color[] colors = { Color.DarkRed, Color.AliceBlue, Color.Orange };
        characterNameButton = new UICharacterNameButton(
                Language.GetText(SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.DaysLeft.Title")),
                Language.GetText(SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.DaysLeft.Empty")),
                Language.GetText(SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.DaysLeft.Description")))
        {
            Width = StyleDimension.FromPixelsAndPercent(-1 * (DoomsdayButtons.Length), 1f / (float)(DoomsdayButtons.Length + 1) * usableWidthPercent),
            Left = StyleDimension.FromPercent(1f - usableWidthPercent),
            HAlign = 0,
        };
        characterNameButton.Top.Set(accumulatedHeight - 3, 0f);
        characterNameButton.SetContents("30");
        characterNameButton.OnLeftMouseDown += SetDayCount;
        characterNameButton.OnMouseOver += self.ShowOptionDescription;
        characterNameButton.OnMouseOut += self.ClearOptionDescription;
        characterNameButton.SetSnapPoint(tagGroup, 0);
        container.Append(characterNameButton);
        for (int i = 0; i < DoomsdayButtons.Length; i++)
        {
            var groupOptionButton =
                new global::SolarDoomsday.GroupOptionButton<DoomsdayOptions>(
                    Enum.GetValues<DoomsdayOptions>()[i],
                    titles[i],
                    descriptions[i],
                    colors[i],
                    icons[i],
                    1f,
                    1f,
                    16f
                )
                {
                    Width = StyleDimension.FromPixelsAndPercent(-1 * (DoomsdayButtons.Length), 1f / (float)(DoomsdayButtons.Length + 1) * usableWidthPercent),
                    Left = StyleDimension.FromPercent(1f - usableWidthPercent),
                    HAlign = (i + 1) / (float)(DoomsdayButtons.Length),
                };
            groupOptionButton.Top.Set(accumulatedHeight, 0f);
            groupOptionButton.OnLeftMouseDown += clickEvent;
            groupOptionButton.OnMouseOver += self.ShowOptionDescription;
            groupOptionButton.OnMouseOut += self.ClearOptionDescription;
            groupOptionButton.SetSnapPoint(tagGroup, i + 1);
            container.Append(groupOptionButton);
            DoomsdayButtons[i] = groupOptionButton;
        }
    }

    private static void AddEnableButton(
        UIWorldCreation self,
        UIElement container,
        float accumulatedHeight,
        UIElement.MouseEvent clickEvent,
        string tagGroup,
        float usableWidthPercent
    )
    {
        vars.self = self;
        vars.container = container;
        vars.accumulatedHeight = accumulatedHeight;
        vars.useableWidthPercent = usableWidthPercent;
        vars.tagGroup = "doomsday";

        var groupOptionButton =
            new global::SolarDoomsday.GroupOptionButton<Enabling>(
                Enabling.e,
                titles[3],
                Language.GetText(SolarDoomsday.mod.GetLocalizationKey("DoomsdaySelection.Enabling.Description")),
                Color.LightCoral,
                icons[4],
                1f,
                1f,
                16f
            )
            {
                Width = StyleDimension.FromPixelsAndPercent(-1, 1),
                Left = StyleDimension.FromPercent(1f - usableWidthPercent),
                HAlign = 0,
            };
        groupOptionButton.Top.Set(accumulatedHeight, 0f);
        groupOptionButton.OnLeftMouseDown += clickEvent;
        groupOptionButton.OnMouseOver += self.ShowOptionDescription;
        groupOptionButton.OnMouseOut += self.ClearOptionDescription;
        groupOptionButton.SetSnapPoint(tagGroup, 0);
        container.Append(groupOptionButton);
        enableButton = groupOptionButton;
    }

    private static void ClickEnableButton(UIMouseEvent evt, UIElement listeningElement)
    {
        ModContent.GetInstance<DoomsdayManager>().ApocalypseEnabledMenu = true;
        AddDoomsdayOptions(vars.self, vars.container, vars.accumulatedHeight, ClickInfectFirstOption, vars.tagGroup, vars.useableWidthPercent);
        listeningElement.Remove();
        foreach (GroupOptionButton<DoomsdayOptions> doomsdayButton in DoomsdayButtons)
        {
            doomsdayButton.SetCurrentOption(DoomsdayOptions.Stagnation);
        }
    }

    private static void ClickInfectFirstOption(UIMouseEvent evt, UIElement listeningElement)
    {
        var groupOptionButton = (GroupOptionButton<DoomsdayOptions>)listeningElement;
        ModContent.GetInstance<DoomsdayManager>().SelectedDoomsdayOption = groupOptionButton.OptionValue;
        foreach (GroupOptionButton<DoomsdayOptions> doomsdayButton in DoomsdayButtons)
        {
            doomsdayButton.SetCurrentOption(groupOptionButton.OptionValue);
        }
    }

    private static void SetDayCount(UIMouseEvent evt, UIElement listeningElement)
    {
        worldCreationState = Main.MenuUI.CurrentState;
        SoundEngine.PlaySound(SoundID.MenuOpen);
        Main.clrInput();
        UIVirtualKeyboard keyboard = new(Language.GetTextValue("Mods.SolarDoomsday.DoomsdaySelection.DaysLeft.Input"), "", OnFinishedSettingDays, ReturnToMenu);
        keyboard.SetMaxInputLength(4);
        Main.MenuUI.SetState(keyboard);
    }

    private static void OnFinishedSettingDays(string days)
    {
        if (Int32.TryParse(days, out int dayCount) && dayCount > 1)
        {
            DoomsdayManager.chosenDayNumber = dayCount;
            characterNameButton.SetContents(dayCount.ToString());
            characterNameButton.Recalculate();
            characterNameButton.TrimDisplayIfOverElementDimensions(4);
            characterNameButton.Recalculate();
            ReturnToMenu();
        }
    }

    private static void ReturnToMenu()
    {
        Main.MenuUI.SetState(worldCreationState);
    }
    /*
    private void DrawWorldSelectIcon(
        On_UIWorldListItem.orig_DrawSelf orig,
        UIWorldListItem uiItem,
        SpriteBatch spriteBatch
    )
    {
        orig.Invoke(uiItem, spriteBatch);
        bool data = uiItem.Data.TryGetHeaderData(
            ModContent.GetInstance<EvilSwapSystem>(),
            out var _data
        );
        UIElement WorldIcon = (UIElement)
            typeof(UIWorldListItem)
                .GetField("_worldIcon", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(uiItem);
        WorldFileData Data = (WorldFileData)
            typeof(AWorldListItem)
                .GetField("_data", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(uiItem);
        if (!data || Data.DontStarve || Data.RemixWorld || Data.DrunkWorld)
        {
            return;
        }
        if (_data.GetBool("GoodAndEvilSwap"))
        {
            UIImage element = new UIImage(
                ModContent.Request<Texture2D>(
                    "InfectionsRemix/Common/UI/IconGESwap" + (Data.IsHardMode ? "Hardmode" : "")
                )
            )
            {
                IgnoresMouseInteraction = true,
            };
            WorldIcon.Append(element);
        }
    }
	*/
}

public class GroupOptionButton<T> : Terraria.GameContent.UI.Elements.GroupOptionButton<T>
{
    public GroupOptionButton(
        T option,
        LocalizedText title,
        LocalizedText description,
        Color textColor,
        string iconTexturePath,
        float textSize = 1,
        float titleAlignmentX = 0.5f,
        float titleWidthReduction = 10
    )
        : base(
            option,
            title,
            description,
            textColor,
            iconTexturePath,
            textSize,
            titleAlignmentX,
            titleWidthReduction
        )
    { }

    public GroupOptionButton(
        T option,
        LocalizedText title,
        LocalizedText description,
        Color textColor,
        Asset<Texture2D> iconTexture,
        float textSize = 1,
        float titleAlignmentX = 0.5f,
        float titleWidthReduction = 10
    )
        : this(
            option,
            title,
            description,
            textColor,
            (string)null,
            textSize,
            titleAlignmentX,
            titleWidthReduction
        )
    {
        typeof(Terraria.GameContent.UI.Elements.GroupOptionButton<T>)
            .GetField("_iconTexture", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(this, iconTexture);
    }

    public void SetIcon(Asset<Texture2D> iconTexture)
    {
        typeof(Terraria.GameContent.UI.Elements.GroupOptionButton<T>)
            .GetField("_iconTexture", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(this, iconTexture);
    }
}

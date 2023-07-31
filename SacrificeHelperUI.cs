using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using static BTD_Mod_Helper.Api.Enums.UpgradeType;
using static BTD_Mod_Helper.Api.Enums.VanillaSprites;

namespace SacrificeHelper;

[RegisterTypeInIl2Cpp(false)]
public class SacrificeHelperUI : MonoBehaviour
{
    public const int InfoWidth = 500;
    public const int InfoHeight = 100;

    private static bool showingExtraParagonInfo;

    public TowerSelectionMenu menu;

    private ModHelperPanel paragonStuff;
    private ModHelperButton degreeButton;
    private ModHelperText degreeText;
    private ModHelperPanel extraParagonInfo;
    private Il2CppSystem.Collections.Generic.List<ModHelperText> paragonDetails;

    private ModHelperPanel templeStuff;
    private ModHelperButton sacrificeToggle;
    private ModHelperPanel extraSacrificeInfo;
    private Il2CppSystem.Collections.Generic.List<ModHelperText> sacrificeTowerSets;

    public SacrificeHelperUI(IntPtr ptr) : base(ptr)
    {
    }

    public void Initialise(TowerSelectionMenu towerSelectionMenu)
    {
        menu = towerSelectionMenu;

        CreateParagonStuff();
        CreateTempleStuff();
    }

    private void CreateParagonStuff()
    {
        paragonStuff = gameObject.AddModHelperPanel(new Info("ParagonStuff", InfoPreset.FillParent));
        degreeButton = paragonStuff.AddButton(new Info("ParagonButton", 375, -75, 135),
            UpgradeContainerParagon, new Action(() =>
            {
                showingExtraParagonInfo = !showingExtraParagonInfo;
                UpdateExtraInfo();
            }));
        degreeText = degreeButton.AddText(new Info("DegreeText", InfoPreset.FillParent), "0", 60);

        extraParagonInfo = paragonStuff.AddPanel(new Info("ExtraParagonInfo", 50, -50, InfoWidth)
        {
            PivotY = 1
        }, null, RectTransform.Axis.Vertical);
        paragonDetails = ParagonDetail.CreateTexts(extraParagonInfo).ToIl2CppList();
    }


    private void CreateTempleStuff()
    {
        templeStuff = gameObject.AddModHelperPanel(new Info("TempleStuff", InfoPreset.FillParent));
        sacrificeToggle = templeStuff.AddButton(new Info("TempleButton", 375, -75, 120),
            NotificationYellow, new Action(() =>
            {
                SacrificeHelperMod.templeSacrificesOff = !SacrificeHelperMod.templeSacrificesOff;
                UpdateUpgradeCosts();
                UpdateExtraInfo();
            }));
        sacrificeToggle.AddImage(new Info("SacrificeIcon", 80), BuffIconBloodSacrifice);

        extraSacrificeInfo = templeStuff.AddPanel(new Info("ExtraSacrificeInfo", 50, -50, InfoWidth)
        {
            PivotY = 1
        }, null, RectTransform.Axis.Vertical);
        var modHelperTexts = TowerSetType.All.ToList()
            .Select(s => CreateInfoLine(extraSacrificeInfo, s, $"MainMenuUiAtlas[{s}Btn]"));
        sacrificeTowerSets = modHelperTexts.ToIl2CppList();
    }

    public void TowerInfoChanged()
    {
        var tower = menu.selectedTower;
        if (tower == null)
        {
            ModHelper.Warning<SacrificeHelperMod>("Couldn't update Paragon Helper UI because tower was null");
            return;
        }

        UpdateParagonStuff(tower);
        UpdateTempleStuff(tower);
        UpdateExtraInfo();
    }

    private void UpdateParagonStuff(TowerToSimulation tower)
    {
        var canUpgradeToParagon = tower.CanUpgradeToParagon(true);
        paragonStuff.SetActive(canUpgradeToParagon);
        if (canUpgradeToParagon)
        {
            var degree = Utils.GetParagonDegree(tower, out var investmentInfo);
            degreeText.SetText($"{degree}");
            degreeText.Text.color = degree >= 100 ? Color.green : Color.white;

            var details = paragonDetails.ToList();
            for (var i = 0; i < ParagonDetail.AllDetails.Count; i++)
            {
                ParagonDetail.AllDetails[i].Update(details[i], investmentInfo);
            }
        }
    }

    private void UpdateTempleStuff(TowerToSimulation tower)
    {
        var canUpgradeToTemple = tower.Def.upgrades?.Any(model => model.upgrade == SunTemple) == true;
        var canUpgradeToGod = tower.Def.upgrades?.Any(model => model.upgrade == TrueSunGod) == true;
        templeStuff.SetActive(canUpgradeToTemple || canUpgradeToGod);
        if (canUpgradeToTemple || canUpgradeToGod)
        {
            var worths = Utils.GetTowerWorths(tower.tower);
            var colors = Utils.GetColors(worths, canUpgradeToGod);

            var towerSets = TowerSetType.All.ToList();
            var details = sacrificeTowerSets.ToList();
            for (var i = 0; i < details.Count; i++)
            {
                var towerSet = towerSets[i];
                var detail = details[i];

                detail.SetText($"${worths[towerSet]:N0}");
                detail.Text.color = colors[towerSet];
            }
        }
    }

    private void UpdateExtraInfo()
    {
        extraParagonInfo.SetActive(showingExtraParagonInfo);
        extraSacrificeInfo.SetActive(!SacrificeHelperMod.templeSacrificesOff);
        sacrificeToggle.Image.SetSprite(SacrificeHelperMod.templeSacrificesOff ? NotificationRed : NotificationYellow);
    }

    private void UpdateUpgradeCosts()
    {
        var gameModel = InGame.instance.GetGameModel();
        var templeUpgrade = gameModel.GetUpgrade(SunTemple);
        var godUpgrade = gameModel.GetUpgrade(TrueSunGod);
        if (SacrificeHelperMod.templeSacrificesOff)
        {
            Utils.ModifyTemple(templeUpgrade);
            Utils.ModifyGod(godUpgrade);
        }
        else
        {
            Utils.DefaultTemple(templeUpgrade);
            Utils.DefaultGod(godUpgrade);
        }

        for (var i = 0; i < menu.upgradeButtons.Count; i++)
        {
            var upgradeButton = menu.upgradeButtons[i];
            upgradeButton.UpdateCost();
            upgradeButton.UpdateVisuals(i, false);
        }
    }

    private static ModHelperText CreateInfoLine(ModHelperPanel parent, string id, string icon)
    {
        var panel = parent.AddPanel(new Info(id, InfoWidth, InfoHeight), null, RectTransform.Axis.Horizontal, 25);
        panel.AddImage(new Info("Icon", InfoHeight), icon);
        var text = panel.AddText(new Info("Amount", InfoWidth - InfoHeight - 50, InfoHeight), "", 69);
        text.Text.alignment = TextAlignmentOptions.MidlineLeft;
        return text;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Simulation.Towers.Behaviors;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.Popups;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using TMPro;
using UnityEngine;
using static SacrificeHelper.SacrificeHelperUI;

namespace SacrificeHelper;

public class ParagonDetail
{
    public static List<ParagonDetail> AllDetails { get; }

    static ParagonDetail()
    {
        AllDetails = new List<ParagonDetail>
        {
            new("Money Spent", VanillaSprites.CoinIcon, info => info.powerFromMoneySpent,
                model => model.maxPowerFromMoneySpent),
            new("Pops / Generated Cash", VanillaSprites.PopIcon, info => info.powerFromPops,
                model => model.maxPowerFromPops),
            new("Non Tier 5 Upgrades Purchased", VanillaSprites.UpgradeContainerGrey,
                info => info.powerFromNonTier5Tiers, model => model.maxPowerFromNonTier5Count,
                image => image.AddText(new Info("Text", InfoPreset.FillParent), "<5", 69)),
            new("Having Tier 5 Towers", VanillaSprites.UpgradeContainerTier5, info => info.powerFromTier5Count,
                model => model.maxPowerFromTier5Count,
                image => image.AddText(new Info("Text", InfoPreset.FillParent), "5", 69)),
            new("Geraldo Totems", VanillaSprites.ParagonPowerTotem, info => info.powerFromBonus)
        };
    }

    public string Name { get; }
    public string Icon { get; }
    public Func<ParagonTower.InvestmentInfo, float> Current { get; }
    public Func<ParagonDegreeDataModel, float> Max { get; }
    public Action<ModHelperComponent> ModifyIcon { get; }
    private float current;
    private float max;

    public ParagonDetail(string name, string icon, Func<ParagonTower.InvestmentInfo, float> current,
        Func<ParagonDegreeDataModel, float> max = null, Action<ModHelperComponent> modifyIcon = null)
    {
        Name = name;
        Icon = icon;
        Current = current;
        Max = max;
        ModifyIcon = modifyIcon;
    }

    public void Update(ModHelperText text, ParagonTower.InvestmentInfo investmentInfo)
    {
        current = Current(investmentInfo);
        max = Max?.Invoke(InGame.instance.GetGameModel().paragonDegreeDataModel) ?? -1;
        text.SetText($"{current:N0}");
        text.Text.color = max > 0 && current >= max ? Color.green : Color.white;
    }
    
    public static IEnumerable<ModHelperText> CreateTexts(ModHelperPanel extraParagonInfo) =>
        AllDetails.Select(detail => detail.CreateInfoLine(extraParagonInfo));

    private ModHelperText CreateInfoLine(ModHelperPanel parent)
    {
        var panel = parent.AddPanel(new Info(Name, InfoWidth, InfoHeight), null, RectTransform.Axis.Horizontal, 25);
        var icon = panel.AddButton(new Info("Icon", InfoHeight), Icon, new Action(() =>
        {
            var message = $"You are getting {current:N0} Paragon Power from {Name}.";
            if (max > 0)
            {
                message += $" You are {current / max:P0} of the way to the maximum of {max:N0}.";
            }

            PopupScreen.instance.SafelyQueue(screen => screen.ShowOkPopup(message));
        }));
        ModifyIcon?.Invoke(icon);
        var text = panel.AddText(new Info("Amount", InfoWidth - InfoHeight - 50, InfoHeight), "", 69);
        text.Text.alignment = TextAlignmentOptions.MidlineLeft;
        return text;
    }
}
﻿using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using HarmonyLib;
using MelonLoader;
using SacrificeHelper;

[assembly: MelonInfo(typeof(SacrificeHelperMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace SacrificeHelper;

public class SacrificeHelperMod : BloonsTD6Mod
{
    public static readonly ModSettingDouble SliderContributionPenalty = new(0.05f)
    {
        description = "The popup added in BTD6 v39 comes with a default 5% penalty to manually invested cash.\n" +
                      "Setting this to 0 would stop it, or negative would counteract it.",
        stepSize = .01f,
        min = -.99,
        max = 1,
        icon = VanillaSprites.UpgradeContainerParagonUnlocked
    };

    public static readonly ModSettingCategory ParagonPowerMaximums = new("Paragon Power Maximums");

    public static readonly ModSettingInt MaxPowerFromPops = new(90000)
    {
        displayName = "Max Paragon Power From Pops\n(-1 for unlimited)",
        min = -1,
        max = 200000,
        icon = VanillaSprites.PopIcon,
        category = ParagonPowerMaximums,
    };

    public static readonly ModSettingInt MaxPowerFromCash = new(60000)
    {
        displayName = "Max Paragon Power From Cash\n(-1 for unlimited)",
        min = -1,
        max = 200000,
        icon = VanillaSprites.CoinIcon,
        category = ParagonPowerMaximums
    };

    public static readonly ModSettingInt MaxPowerFromNonTier5s = new(10000)
    {
        displayName = "Max Paragon Power From Non Tier 5s\n(-1 for unlimited)",
        min = -1,
        max = 200000,
        icon = VanillaSprites.UpgradeContainerGrey,
        modifyOption = option => option.Icon.AddText(new Info("Text", InfoPreset.FillParent), "<5", 100),
        category = ParagonPowerMaximums
    };

    public static readonly ModSettingInt MaxPowerFromTier5s = new(50000)
    {
        displayName = "Max Paragon Power From Tier 5s\n(-1 for unlimited)",
        min = -1,
        max = 200000,
        icon = VanillaSprites.UpgradeContainerTier5,
        modifyOption = option => option.Icon.AddText(new Info("Text", InfoPreset.FillParent), "5", 100),
        category = ParagonPowerMaximums
    };

    public static readonly ModSettingCategory ParagonPowerWeights = new("Paragon Power Weights");

    private static readonly ModSettingInt PopsScaleFactor = new(180)
    {
        displayName = "Pops per Point of Paragon Power",
        min = 1,
        icon = VanillaSprites.PopIcon,
        category = ParagonPowerWeights
    };

    private static readonly ModSettingInt CashScaleFactor = new(20000)
    {
        displayName = "Paragon Power Scale Factor for Cash",
        min = 1,
        icon = VanillaSprites.CoinIcon,
        category = ParagonPowerWeights,
        description = "As of v39, the Paragon Upgrade Price is divided by this to get the final value"
    };

    private static readonly ModSettingInt NonTier5ScaleFactor = new(100)
    {
        displayName = "Paragon Power Scale Factor for Non Tier 5s",
        min = 0,
        icon = VanillaSprites.UpgradeContainerGrey,
        modifyOption = option => option.Icon.AddText(new Info("Text", InfoPreset.FillParent), "<5", 100),
        category = ParagonPowerWeights
    };

    private static readonly ModSettingInt Tier5ScaleFactor = new(6000)
    {
        displayName = "Paragon Power Scale Factor for Tier 5s",
        min = 0,
        icon = VanillaSprites.UpgradeContainerTier5,
        modifyOption = option => option.Icon.AddText(new Info("Text", InfoPreset.FillParent), "5", 100),
        category = ParagonPowerWeights
    };

    public static readonly ModSettingCategory TempleAlternateCosts = new("Template Alternate Costs");

    public static readonly ModSettingDouble TempleAlternateCostMod = new(.5)
    {
        displayName = "Alternate Sun Temple Cost",
        min = 0,
        max = 1,
        stepSize = .01f,
        icon = VanillaSprites.SunTempleUpgradeIcon,
        description = "What portion the cost should be if you decide to get a Sun Temple without doing sacrifices",
        category = TempleAlternateCosts
    };

    public static readonly ModSettingDouble GodAlternateCostMod = new(.2)
    {
        displayName = "Alternate True Sun God Cost",
        min = 0,
        max = 1,
        stepSize = .01f,
        icon = VanillaSprites.TrueSonGodUpgradeIcon,
        description = "What portion the cost should be if you decide to get a True Sun God without doing sacrifices",
        category = TempleAlternateCosts
    };

    public static bool templeSacrificesOff;

    public override void OnNewGameModel(GameModel result)
    {
        var degreeData = result.paragonDegreeDataModel;

        degreeData.maxPowerFromPops = MaxPowerFromPops;
        if (degreeData.maxPowerFromPops < 0)
        {
            degreeData.maxPowerFromPops = degreeData.MaxInvestment;
        }

        degreeData.maxPowerFromMoneySpent = MaxPowerFromCash;
        if (degreeData.maxPowerFromMoneySpent < 0)
        {
            degreeData.maxPowerFromMoneySpent = degreeData.MaxInvestment;
        }

        degreeData.maxPowerFromNonTier5Count = MaxPowerFromNonTier5s;
        if (degreeData.maxPowerFromNonTier5Count < 0)
        {
            degreeData.maxPowerFromNonTier5Count = degreeData.MaxInvestment;
        }

        degreeData.maxPowerFromTier5Count = MaxPowerFromTier5s;
        if (degreeData.maxPowerFromTier5Count < 0)
        {
            degreeData.maxPowerFromTier5Count = degreeData.MaxInvestment;
        }

        degreeData.popsOverX = PopsScaleFactor;
        degreeData.moneySpentOverX = CashScaleFactor;
        degreeData.nonTier5TowersMultByX = NonTier5ScaleFactor;
        degreeData.tier5TowersMultByX = Tier5ScaleFactor;

        degreeData.paidContributionPenalty = SliderContributionPenalty;

        templeSacrificesOff = false;
    }

    [HarmonyPatch(typeof(MonkeyTemple), nameof(MonkeyTemple.StartSacrifice))]
    public class MonkeyTemple_StartSacrifice
    {
        [HarmonyPrefix]
        public static bool Prefix() => !templeSacrificesOff;
    }
}
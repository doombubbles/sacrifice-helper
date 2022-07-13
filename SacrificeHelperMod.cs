using Assets.Scripts.Models;
using Assets.Scripts.Simulation.Towers.Behaviors;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using MelonLoader;
using SacrificeHelper;
using UnityEngine;

[assembly: MelonInfo(typeof(SacrificeHelperMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace SacrificeHelper;

public class SacrificeHelperMod : BloonsTD6Mod
{
    public const string SunTemple = "Sun Temple";
    public const string TrueSunGod = "True Sun God";

    public static readonly ModSettingInt TempleAlternateCost = new(50000)
    {
        displayName = "Alternate Sun Temple Cost",
        min = 0,
        icon = VanillaSprites.SunTempleUpgradeIcon,
        description = "The reduced cost for if you decide to get a Sun Temple without doing sacrifices"
    };

    public static readonly ModSettingInt GodAlternateCost = new(100000)
    {
        displayName = "Alternate True Sun God Cost",
        min = 0,
        icon = VanillaSprites.TrueSonGodUpgradeIcon,
        description = "The reduced cost for if you decide to get a True Sun God without doing sacrifices"
    };

    public static readonly ModSettingInt MaxFromPops = new(90000)
    {
        displayName = "Max Paragon Power From Pops\n(-1 for unlimited)",
        min = -1,
        icon = VanillaSprites.PopIcon
    };

    public static readonly ModSettingInt MaxFromCash = new(10000)
    {
        displayName = "Max Paragon Power From Cash\n(-1 for unlimited)",
        min = -1,
        icon = VanillaSprites.Gold2
    };

    public static readonly ModSettingInt MaxFromNonTier5s = new(10000)
    {
        displayName = "Max Paragon Power From Non Tier 5s\n(-1 for unlimited)",
        min = -1,
        icon = VanillaSprites.UpgradeContainerBlue
    };

    public static readonly ModSettingInt MaxFromTier5s = new(90000)
    {
        displayName = "Max Paragon Power From Tier 5s\n(-1 for unlimited)",
        min = -1,
        icon = VanillaSprites.UpgradeContainerTier5
    };

    private static readonly ModSettingInt PopsPerPoint = new(180)
    {
        displayName = "Pops per Point of Paragon Power",
        min = 1,
        icon = VanillaSprites.PopIcon
    };

    private static readonly ModSettingInt CashPerPoint = new(25)
    {
        displayName = "Cash per Point of Paragon Power",
        min = 1,
        icon = VanillaSprites.Gold2
    };

    private static readonly ModSettingInt NonTier5sScaleFactor = new(100)
    {
        displayName = "Paragon Power Scale Factor for Non Tier 5s",
        min = 0,
        icon = VanillaSprites.UpgradeContainerBlue
    };

    private static readonly ModSettingInt Tier5sScaleFactor = new(10000)
    {
        displayName = "Paragon Power Scale Factor for Tier 5s",
        min = 0,
        icon = VanillaSprites.UpgradeContainerTier5
    };

    public static bool templeSacrificesOff = false;

    public override void OnNewGameModel(GameModel result)
    {
        result.paragonDegreeDataModel.maxPowerFromPops = MaxFromPops;
        if (result.paragonDegreeDataModel.maxPowerFromPops < 0)
        {
            result.paragonDegreeDataModel.maxPowerFromPops = int.MaxValue;
        }

        result.paragonDegreeDataModel.maxPowerFromMoneySpent = MaxFromCash;
        if (result.paragonDegreeDataModel.maxPowerFromMoneySpent < 0)
        {
            result.paragonDegreeDataModel.maxPowerFromMoneySpent = int.MaxValue;
        }

        result.paragonDegreeDataModel.maxPowerFromNonTier5Count = MaxFromNonTier5s;
        if (result.paragonDegreeDataModel.maxPowerFromNonTier5Count < 0)
        {
            result.paragonDegreeDataModel.maxPowerFromNonTier5Count = int.MaxValue;
        }

        result.paragonDegreeDataModel.maxPowerFromTier5Count = MaxFromTier5s;
        if (result.paragonDegreeDataModel.maxPowerFromTier5Count < 0)
        {
            result.paragonDegreeDataModel.maxPowerFromTier5Count = int.MaxValue;
        }

        result.paragonDegreeDataModel.popsOverX = PopsPerPoint;
        result.paragonDegreeDataModel.moneySpentOverX = CashPerPoint;
        result.paragonDegreeDataModel.nonTier5TowersMultByX = NonTier5sScaleFactor;
        result.paragonDegreeDataModel.tier5TowersMultByX = Tier5sScaleFactor;
    }

    public override void OnGameObjectsReset()
    {
        if (TSMThemeChanges.templeText != null)
        {
            foreach (var (_, text) in TSMThemeChanges.templeText)
            {
                Object.Destroy(text);
            }

            TSMThemeChanges.templeText = null;
        }


        if (TSMThemeChanges.templeIcons != null)
        {
            foreach (var (_, icon) in TSMThemeChanges.templeIcons)
            {
                Object.Destroy(icon);
            }

            TSMThemeChanges.templeIcons = null;
        }

        if (TSMThemeChanges.templeInfoButton != null)
        {
            Object.Destroy(TSMThemeChanges.templeInfoButton);
            TSMThemeChanges.templeInfoButton = null;
        }

        if (TSMThemeChanges.paragonButton != null)
        {
            Object.Destroy(TSMThemeChanges.paragonButton);
            TSMThemeChanges.paragonButton = null;
        }

        if (TSMThemeChanges.paragonButtonText != null)
        {
            Object.Destroy(TSMThemeChanges.paragonButtonText);
            TSMThemeChanges.paragonButtonText = null;
        }

        if (TSMThemeChanges.paragonText != null)
        {
            foreach (var (_, text) in TSMThemeChanges.paragonText)
            {
                Object.Destroy(text);
            }

            TSMThemeChanges.paragonText = null;
        }


        if (TSMThemeChanges.paragonIcons != null)
        {
            foreach (var (_, icon) in TSMThemeChanges.paragonIcons)
            {
                Object.Destroy(icon);
            }

            TSMThemeChanges.paragonIcons = null;
        }
    }

    [HarmonyPatch(typeof(MonkeyTemple), nameof(MonkeyTemple.StartSacrifice))]
    public class MonkeyTemple_StartSacrifice
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return !templeSacrificesOff;
        }
    }
}
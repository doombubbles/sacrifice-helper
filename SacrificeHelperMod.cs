using Assets.Scripts.Models;
using Assets.Scripts.Simulation.Towers.Behaviors;
using Assets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
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
    public static readonly ModSettingCategory ParagonPowerMaximums = new("Paragon Power Maximums");

    public static readonly ModSettingInt MaxFromPops = new(90000)
    {
        displayName = "Max Paragon Power From Pops\n(-1 for unlimited)",
        min = -1,
        icon = VanillaSprites.PopIcon,
        category = ParagonPowerMaximums
    };

    public static readonly ModSettingInt MaxFromCash = new(10000)
    {
        displayName = "Max Paragon Power From Cash\n(-1 for unlimited)",
        min = -1,
        icon = VanillaSprites.CoinIcon,
        category = ParagonPowerMaximums
    };

    public static readonly ModSettingInt MaxFromNonTier5s = new(10000)
    {
        displayName = "Max Paragon Power From Non Tier 5s\n(-1 for unlimited)",
        min = -1,
        icon = VanillaSprites.UpgradeContainerGrey,
        modifyOption = option => option.Icon.AddText(new Info("Text", InfoPreset.FillParent), "<5", 100),
        category = ParagonPowerMaximums
    };

    public static readonly ModSettingInt MaxFromTier5s = new(90000)
    {
        displayName = "Max Paragon Power From Tier 5s\n(-1 for unlimited)",
        min = -1,
        icon = VanillaSprites.UpgradeContainerTier5,
        modifyOption = option => option.Icon.AddText(new Info("Text", InfoPreset.FillParent), "5", 100),
        category = ParagonPowerMaximums
    };

    public static readonly ModSettingCategory ParagonPowerWeights = new("Paragon Power Weights");

    private static readonly ModSettingInt PopsPerPoint = new(180)
    {
        displayName = "Pops per Point of Paragon Power",
        min = 1,
        icon = VanillaSprites.PopIcon,
        category = ParagonPowerWeights
    };

    private static readonly ModSettingInt CashPerPoint = new(25)
    {
        displayName = "Cash per Point of Paragon Power",
        min = 1,
        icon = VanillaSprites.CoinIcon,
        category = ParagonPowerWeights
    };

    private static readonly ModSettingInt NonTier5sScaleFactor = new(100)
    {
        displayName = "Paragon Power Scale Factor for Non Tier 5s",
        min = 0,
        icon = VanillaSprites.UpgradeContainerGrey,
        modifyOption = option => option.Icon.AddText(new Info("Text", InfoPreset.FillParent), "<5", 100),
        category = ParagonPowerWeights
    };

    private static readonly ModSettingInt Tier5sScaleFactor = new(10000)
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

        templeSacrificesOff = false;
    }

    [HarmonyPatch(typeof(MonkeyTemple), nameof(MonkeyTemple.StartSacrifice))]
    public class MonkeyTemple_StartSacrifice
    {
        [HarmonyPrefix]
        public static bool Prefix() => !templeSacrificesOff;
    }

    [HarmonyPatch(typeof(TowerSelectionMenu), nameof(TowerSelectionMenu.UpdateTower))]
    internal static class TowerSelectionMenu_UpdateTower
    {
        [HarmonyPostfix]
        private static void Postfix(TowerSelectionMenu __instance)
        {
            var themeManager = __instance.themeManager;
            var currentTheme = themeManager.CurrentTheme;
            if (currentTheme == null)
            {
                TaskScheduler.ScheduleTask(() => Postfix(__instance), () => themeManager.CurrentTheme != null);
                return;
            }

            var ui = currentTheme.GetComponent<SacrificeHelperUI>();
            if (ui == null)
            {
                ui = currentTheme.gameObject.AddComponent<SacrificeHelperUI>();
                ui.Initialise(themeManager);
            }

            ui.TowerInfoChanged();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using UnityEngine;

namespace SacrificeHelper;

public static class Utils
{
    public static void DefaultTemple(UpgradeModel upgradeModel)
    {
        upgradeModel.confirmation = UpgradeType.SunTemple;
        upgradeModel.cost = 5 * (int) Math.Round(upgradeModel.cost / SacrificeHelperMod.TempleAlternateCostMod / 5);
    }

    public static void ModifyTemple(UpgradeModel upgradeModel)
    {
        upgradeModel.confirmation = "";
        upgradeModel.cost = 5 * (int) Math.Round(upgradeModel.cost * SacrificeHelperMod.TempleAlternateCostMod / 5);
    }

    public static void DefaultGod(UpgradeModel upgradeModel)
    {
        upgradeModel.confirmation = UpgradeType.TrueSunGod;
        upgradeModel.cost = 5 * (int) Math.Round(upgradeModel.cost / SacrificeHelperMod.GodAlternateCostMod / 5);
    }

    public static void ModifyGod(UpgradeModel upgradeModel)
    {
        upgradeModel.confirmation = "";
        upgradeModel.cost = 5 * (int) Math.Round(upgradeModel.cost * SacrificeHelperMod.GodAlternateCostMod / 5);
    }

    public static Dictionary<string, Color> GetColors(Dictionary<string, float> worths, bool god)
    {
        var ret = new Dictionary<string, Color>();
        if (!god)
        {
            var worst = "";
            var min = float.MaxValue;
            foreach (var key in worths.Keys)
            {
                if (worths[key] < min)
                {
                    worst = key;
                    min = worths[key];
                }
            }

            ret[worst] = Color.red;
        }


        foreach (var key in worths.Keys)
        {
            if (ret.ContainsKey(key))
            {
                continue;
            }

            var worth = worths[key];
            var color = Color.red;
            if (worth > 50000)
            {
                color = Color.green;
            }
            else if (key == "Magic")
            {
                if (worth > 1000)
                {
                    color = Color.white;
                }
            }
            else if (worth > 300)
            {
                color = Color.white;
            }

            ret[key] = color;
        }

        return ret;
    }

    public static bool IsValidTower(Tower tower) => !tower.towerModel.isPowerTower &&
                                                    !tower.towerModel.isGeraldoItem &&
                                                    !tower.towerModel.IsBeastHandlerPet &&
                                                    !tower.towerModel.isParagon &&
                                                    tower.towerModel.baseId != "TempleBase-TempleBase";

    public static Dictionary<string, float> GetTowerWorths(Tower tower) =>
        TowerSetType.All.ToDictionary(s => s, s => GetTowerSetWorth(s, tower));

    private static float GetTowerSetWorth(string towerSet, Tower tower) => InGame.instance.GetTowerManager()
        .GetTowersInRange(tower.Position, tower.towerModel.range)
        .ToList()
        .Where(t => t.towerModel.towerSet.ToString() == towerSet && t.Id != tower.Id && IsValidTower(t))
        .Sum(t => t.worth);

    private static ParagonTower FakeParagonTower(Tower tower) => new()
    {
        Sim = tower.Sim,
        entity = tower.entity,
        model = tower.model,
        tower = tower,
        isActive = true,
        activeAt = -1
    };

    public static int GetParagonDegree(TowerToSimulation tower, out ParagonTower.InvestmentInfo investmentInfo, float bonus = 0)
    {
        var degreeDataModel = InGame.instance.GetGameModel().paragonDegreeDataModel;
        var degree = 0;
        var index = 0;
        investmentInfo = default;

        var paragonCost = tower.GetUpgradeCost(0, 6, -1, true);

        var paragonTower = FakeParagonTower(tower.tower);

        paragonTower.upgradeCost = paragonCost;

        var bonusInvestment = new ParagonTower.InvestmentInfo
        {
            powerFromMoneySpent = (bonus * degreeDataModel.moneySpentOverX) /
                                  ((1 + degreeDataModel.paidContributionPenalty) * paragonCost)
        };

        investmentInfo = InGame.instance.GetAllTowerToSim()
            .Where(tts =>
                tts.Def.baseId == tower.Def.baseId || tts.Def.GetChild<ParagonSacrificeBonusModel>() != null)
            .OrderBy(tts => paragonTower.GetTowerInvestment(tts.tower).totalInvestment)
            .Select(tts => paragonTower.GetTowerInvestment(tts.tower, tts.Def.tier >= 5 ? index++ : 3))
            .Aggregate(bonusInvestment, paragonTower.CombineInvestments);


        var requirements = degreeDataModel.powerDegreeRequirements;

        while (investmentInfo.totalInvestment >= requirements[degree])
        {
            degree++;
            if (degree == 100)
            {
                break;
            }
        }

        return degree;
    }
}
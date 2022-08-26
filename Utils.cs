﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Models.Towers.Upgrades;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Simulation.Towers.Behaviors;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New.InGame;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using UnityEngine;
using static Assets.Scripts.Simulation.Towers.Behaviors.ParagonTower;

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

    public static Dictionary<string, float> GetTowerWorths(Tower tower) =>
        TowerSetType.All.ToDictionary(s => s, s => GetTowerSetWorth(s, tower));

    private static float GetTowerSetWorth(string towerSet, Tower tower) => InGame.instance.GetTowerManager()
        .GetTowersInRange(tower.Position, tower.towerModel.range)
        .ToList()
        .Where(t => t.towerModel.towerSet == towerSet && t.Id != tower.Id)
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

    public static int GetParagonDegree(TowerToSimulation tower, out InvestmentInfo investmentInfo)
    {
        var degreeDataModel = InGame.instance.GetGameModel().paragonDegreeDataModel;
        var degree = 0;
        var index = 0;
        investmentInfo = default;

        var paragonCost = tower.GetUpgradeCost(0, 6, -1, true);

        // TODO seems like a bug with BTD6, should the paragon upgrade cost really be included ???
        tower.tower.worth += paragonCost;

        var paragonTower = FakeParagonTower(tower.tower);
        investmentInfo = InGame.instance.GetAllTowerToSim()
            .Where(tts => tts.Def.baseId == tower.Def.baseId || tts.Def.GetChild<ParagonSacrificeBonusModel>() != null)
            .OrderBy(tts => paragonTower.GetTowerInvestment(tts.tower, 3).totalInvestment)
            .Select(tts => paragonTower.GetTowerInvestment(tts.tower, tts.Def.tier >= 5 ? index++ : 3))
            .Aggregate(paragonTower.CombineInvestments);

        tower.tower.worth -= paragonCost;

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
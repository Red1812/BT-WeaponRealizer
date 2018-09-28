﻿using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using UnityEngine;

namespace WeaponRealizer
{
    internal static partial class Calculator
    {
        private static class DistanceBasedVariance
        {
            public static bool IsApplicable(Weapon weapon)
            {
                return Core.ModSettings.DistanceBasedVariance &&
                       HasPositiveVarianceMultiplier(weapon);
            }

            private const double Pi2 = Math.PI / 2.0;
            private static readonly Dictionary<string, float> VarianceMultipliers = new Dictionary<string, float>();
            public static float Calculate(ICombatant attacker, ICombatant target, Weapon weapon, float damage, float rawDamage)
            {
                var damagePerShot = weapon.DamagePerShot;
                var adjustment = rawDamage / damagePerShot;
                float varianceMultiplier;
                var distance = Vector3.Distance(attacker.TargetPosition, target.TargetPosition);
                var distanceDifference = weapon.MaxRange - distance;
                var distanceRatio = distanceDifference / weapon.MaxRange;
                var baseMultiplier = VarianceMultipliers[weapon.defId];
                var distanceBasedFunctionMultiplier = (float) Math.Atan(Pi2 * distanceRatio + baseMultiplier);
                if (distance < weapon.MaxRange)
                {
                    varianceMultiplier = Mathf.Max(
                        baseMultiplier,
                        Mathf.Min(
                            1.0f,
                            distanceBasedFunctionMultiplier
                        ));
                }
                else // out of range
                {
                    return damage;
                }
                var computedDamage = damage * varianceMultiplier * adjustment;
                Logger.Debug($"distanceBasedFunctionMultiplier: {distanceBasedFunctionMultiplier}\n" +
                             $"defId: {weapon.defId}\n" +
                             $"varianceMultiplier: {varianceMultiplier}\n" +
                             $"adjustment: {adjustment}\n" +
                             $"damage: {damage}\n" +
                             $"distance: {distance}\n" +
                             $"max: {weapon.MaxRange}\n" +
                             $"distanceDifference {distanceDifference}\n" +
                             $"baseMultplier: {baseMultiplier}\n" +
                             $"distanceRatio: {distanceRatio}\n" +
                             $"computedDamage: {computedDamage}\n");
                return computedDamage;
            }

            private static bool HasPositiveVarianceMultiplier(Weapon weapon)
            {
                if (!VarianceMultipliers.ContainsKey(weapon.defId))
                    VarianceMultipliers[weapon.defId] = ParseBaseMultiplier(weapon);
                return VarianceMultipliers[weapon.defId] > Epsilon;
            }

            private const string DistanceVarianceTagPrefix = "WR-variance_by_distance";
            private static readonly char[] TagDelimiter = new char[] {'-'};
            private static float ParseBaseMultiplier(Weapon weapon)
            {
                if (!weapon.weaponDef.ComponentTags.Any(tag => tag.StartsWith(DistanceVarianceTagPrefix)))
                    return 0.0f;
                var rawTag = weapon.weaponDef.ComponentTags.First(tag => tag.StartsWith(DistanceVarianceTagPrefix));
                var baseMultiplier =
                    rawTag == DistanceVarianceTagPrefix
                        ? Core.ModSettings.DistanceBasedVarianceMaxRangeDamageMultiplier
                        : float.Parse(rawTag.Split(TagDelimiter, 3).Last()) / 100.0f;
                return baseMultiplier;
            }
        }
    }
}
﻿using BattleTech;

namespace WeaponRealizer
{
    static partial class Calculator
    {
        private static class HeatDamageModifier
        {
            public static bool IsNotApplicable(Weapon weapon)
            {
                // TODO: need a mechanism for this to support multiplier on both sides of an attack
                return true;
            }
        }
    }
}
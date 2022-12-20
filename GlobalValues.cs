using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Katarina
{
    class GlobalValues
    {
        internal static float primaryHealCoefficient = MainPlugin.meleevamp.Value;
        internal static float secondaryHealCoefficient = MainPlugin.daggerhealpercent.Value;
        internal static float daggerPickupRadius = MainPlugin.daggerpickupzone.Value;
        internal static float daggerPickupExplosionRadius = MainPlugin.daggerpickupslash.Value;
        internal static float maxBlinkDistance = 175f;
        internal static float specialProjectileSpeed = 50;
        internal static float specialCDReductionOnKill = 4;
    }
}

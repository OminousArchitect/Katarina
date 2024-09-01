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
        internal static float secondaryHealCoefficient = 0.1f;
        internal static float daggerPickupRadius = MainPlugin.daggerpickupzone.Value;
        internal static float daggerPickupExplosionRadius = MainPlugin.daggerpickupslash.Value;
        internal static float maxBlinkDistance = 175f;
        internal static float specialProjectileSpeed = 51;
        internal static float specialCDReductionOnKill = 4;
    }
}

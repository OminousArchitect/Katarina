using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using ExtraSkillSlots;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;

namespace Katarina
{
    class Hook
    {
        internal static void Hooks() //they're really C# events
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            var damageInfo = damageReport.damageInfo;
            if (damageReport.victim && damageReport.victimBody && damageReport.attacker && damageReport.attackerBody)
            {
                var name = damageReport.attackerBody.name;
                bool flag1 = damageInfo.HasModdedDamageType(Prefabs.blade1) || damageInfo.HasModdedDamageType(Prefabs.blade2) || damageInfo.HasModdedDamageType(Prefabs.blade3)
                    || damageInfo.HasModdedDamageType(Prefabs.blade4) || damageInfo.HasModdedDamageType(Prefabs.blade5) || damageInfo.HasModdedDamageType(Prefabs.blade6);
                
                //killed by Voracious Blade
                if (flag1)
                {
                    if (damageReport.attackerBody.skillLocator && damageReport.attackerBody.skillLocator.secondary && MainPlugin.killexplosionfx.Value)
                    {
                        damageReport.attackerBody.skillLocator.secondary.AddOneStock();
                        
                        if (MainPlugin.killexplosionfx.Value)
                        {
                            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect"), new EffectData {origin = damageInfo.position}, true);
                        }
                    }
                    
                    if (NetworkServer.active)
                    {
                        damageReport.attackerBody.healthComponent.Heal(damageReport.victimBody.healthComponent.fullHealth * GlobalValues.secondaryHealCoefficient, default(ProcChainMask));
                    }
                }
                
                //killed by Shunpo
                if (damageInfo.HasModdedDamageType(Prefabs.blink))
                {
                    ExtraSkillLocator extraskills = damageReport.attackerBody.GetComponent<ExtraSkillLocator>();

                    if (extraskills.extraThird.stock == 0)
                    {
                        extraskills.extraThird.AddOneStock();
                    }

                    if (damageReport.attackerBody.skillLocator && damageReport.attackerBody.skillLocator.utility.stock == 0)
                    {
                        damageReport.attackerBody.skillLocator.utility.AddOneStock();
                    }

                    if (MainPlugin.killexplosionfx.Value)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect"), new EffectData {origin = damageInfo.position}, true);
                    }
                }
                
                //killed by dagger pickup
                if (damageInfo.HasModdedDamageType((Prefabs.daggerPickup)))
                {
                    if (damageReport.attackerBody.skillLocator && damageReport.attackerBody.skillLocator.secondary.stock == 0)
                    {
                        damageReport.attackerBody.skillLocator.secondary.AddOneStock();
                    }

                    if (MainPlugin.killexplosionfx.Value)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect"), new EffectData {origin = damageInfo.position}, true);
                    }
                }
                
                //Recharge Special
                if (name.Contains("NinesKatarinaBody") && damageReport.attackerBody.skillLocator && damageReport.attackerBody.skillLocator.special && NetworkServer.active)
                {
                    damageReport.attackerBody.skillLocator.special.RunRecharge(GlobalValues.specialCDReductionOnKill);
                }
                
                //Reset Shunpo on Melee Kill
                if (MainPlugin.resetUtilityOnPrimaryKill.Value && name.Contains("NinesKatarinaBody") && damageInfo.damageType == DamageType.BonusToLowHealth && damageReport.attackerBody.skillLocator && damageReport.attackerBody.skillLocator.utility)
                {
                    damageReport.attackerBody.skillLocator.utility.Reset();
                }
            }
        }
        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            // Charged dagger bleed proc, we do the ModdedDamageType bc vanilla doesn't have any other SuperBleed damagetypes other than OnCrit only.
            if (damageInfo.HasModdedDamageType(Prefabs.bladeBleed) && NetworkServer.active)
            {
                DotController.InflictDot(self.gameObject, damageInfo.attacker, DotController.DotIndex.SuperBleed, 15f * damageInfo.procCoefficient, 1f);
            }

            // This is for the M1 heal. We don't add ModdedDamageType to overlap attacks bc it is buggy as hell for clients. So instead we check for a vanilla damagetype and the attacker it comes from.
            if (damageInfo.attacker && damageInfo.attacker.name.Contains("NinesKatarinaBody"))
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody && NetworkServer.active)
                {
                    if (damageInfo.damageType == DamageType.BonusToLowHealth)
                    {
                        attackerBody.healthComponent.Heal(damageInfo.damage * GlobalValues.primaryHealCoefficient, default(ProcChainMask));
                    }
                }
            }
            orig(self, damageInfo);

            //Spawn Pickups
            if (damageInfo.attacker && self.alive)
            {
                Vector3 direction = (Vector3.up * 40) + (Vector3.forward * 15);
                if (damageInfo.HasModdedDamageType(Prefabs.blade1))
                {
                    if (NetworkServer.active)
                    {
                        ProjectileManager.instance.FireProjectile(Prefabs.dagger1ProjectilePickup, damageInfo.position, Util.QuaternionSafeLookRotation(direction), damageInfo.attacker, 0, 0, false, DamageColorIndex.Default);
                    }
                }
                else if (damageInfo.HasModdedDamageType(Prefabs.blade2))
                {
                    if (NetworkServer.active)
                    {
                        ProjectileManager.instance.FireProjectile(Prefabs.dagger2ProjectilePickup, damageInfo.position, Util.QuaternionSafeLookRotation(direction), damageInfo.attacker, 0, 0, false, DamageColorIndex.Default);
                    }
                }
                else if (damageInfo.HasModdedDamageType(Prefabs.blade3))
                {
                    if (NetworkServer.active)
                    {
                        ProjectileManager.instance.FireProjectile(Prefabs.dagger3ProjectilePickup, damageInfo.position, Util.QuaternionSafeLookRotation(direction), damageInfo.attacker, 0, 0, false, DamageColorIndex.Default);
                    }
                }
                else if (damageInfo.HasModdedDamageType(Prefabs.blade4))
                {
                    if (NetworkServer.active)
                    {
                        ProjectileManager.instance.FireProjectile(Prefabs.dagger4ProjectilePickup, damageInfo.position, Util.QuaternionSafeLookRotation(direction), damageInfo.attacker, 0, 0, false, DamageColorIndex.Default);
                    }
                }
                else if (damageInfo.HasModdedDamageType(Prefabs.blade5))
                {
                    if (NetworkServer.active)
                    {
                        ProjectileManager.instance.FireProjectile(Prefabs.dagger5ProjectilePickup, damageInfo.position, Util.QuaternionSafeLookRotation(direction), damageInfo.attacker, 0, 0, false, DamageColorIndex.Default);
                    }
                }
                else if (damageInfo.HasModdedDamageType(Prefabs.blade6))
                {
                    if (NetworkServer.active)
                    {
                        ProjectileManager.instance.FireProjectile(Prefabs.dagger6ProjectilePickup, damageInfo.position, Util.QuaternionSafeLookRotation(direction), damageInfo.attacker, 0, 0, false, DamageColorIndex.Default);
                    }
                }                
            }
        }

        internal static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {

        }
    }
}

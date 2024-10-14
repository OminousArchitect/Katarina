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
            //RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            GlobalEventManager.onClientDamageNotified += GlobalEventManager_onClientDamageNotified;
        }

        private static void GlobalEventManager_onClientDamageNotified(DamageDealtMessage damageDealtMessage)
        {
            //if (damageDealtMessage.victim.GetComponent<HealthComponent>().health <= 0 && damageDealtMessage.
        }
        
        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            var damageInfo = damageReport.damageInfo;
            if (damageReport.victim && damageReport.victimBody && damageReport.attacker && damageReport.attackerBody)
            {
                bool flag1 = damageInfo.HasModdedDamageType(Prefabs.blade1) || damageInfo.HasModdedDamageType(Prefabs.blade2) || damageInfo.HasModdedDamageType(Prefabs.blade3)
                             || damageInfo.HasModdedDamageType(Prefabs.blade4) || damageInfo.HasModdedDamageType(Prefabs.blade5) || damageInfo.HasModdedDamageType(Prefabs.blade6);
                
                //killed by Voracious Blade
                if (flag1)
                {
                    if (damageReport.attackerBody.skillLocator.secondary.baseRechargeInterval == 9f)
                    {
                        damageReport.attackerBody.skillLocator.secondary.AddOneStock();
                    }
                    else if (damageReport.attackerBody.skillLocator.secondary.baseRechargeInterval == 5.5f)
                    {
                        bool oneDagger = damageReport.attackerBody.skillLocator.secondary.stock == 1;
                        bool zeroDaggers = damageReport.attackerBody.skillLocator.secondary.stock == 0;
                        
                        if (oneDagger || zeroDaggers)
                        {
                            damageReport.attackerBody.skillLocator.secondary.Reset();   
                        }
                        else
                        {
                            damageReport.attackerBody.skillLocator.secondary.AddOneStock();
                        }
                    }

                    if (NetworkServer.active)
                    {
                        damageReport.attackerBody.healthComponent.Heal(damageReport.victimBody.healthComponent.fullHealth * GlobalValues.secondaryHealCoefficient, default(ProcChainMask));
                    }
                    
                    if (MainPlugin.killexplosionfx.Value)
                    {
                        if (!MainPlugin.collapseEveryone.Value)
                        {
                            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect"), new EffectData {origin = damageInfo.position}, true);
                        }
                        else
                        {
                            EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BleedOnHitVoid/FractureImpactEffect.prefab").WaitForCompletion(), new EffectData {origin = damageInfo.position}, true);
                        }
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
                    
                    if (damageReport.attackerBody.skillLocator && damageReport.attackerBody.skillLocator.secondary.stock == 0)
                    {
                        damageReport.attackerBody.skillLocator.secondary.AddOneStock();
                    }
                    
                    bool isFlying = damageReport.victimBody.isFlying;
                    bool isVulture = damageReport.victimBody.bodyIndex == MainPlugin.vultureIndex;
                    bool isPest = damageReport.victimBody.bodyIndex == MainPlugin.pestIndex;

                    switch (MainPlugin.shunpoSetting.Value)
                    {
                        case 0:
                            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect"), new EffectData {origin = damageInfo.position}, true);
                            break;
                        case 1:
                            EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BleedOnHitVoid/FractureImpactEffect.prefab").WaitForCompletion(), new EffectData {origin = damageInfo.position}, true);
                            break;
                        case 2:
                            if (isFlying || isPest || isVulture)
                            {
                                EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BleedOnHitVoid/FractureImpactEffect.prefab").WaitForCompletion(), new EffectData {origin = damageInfo.position}, true);
                            }
                            else
                            {
                                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect"), new EffectData {origin = damageInfo.position}, true);
                            }
                            break;
                    }
                }
                
                //killed by dagger pickup
                if (damageInfo.HasModdedDamageType(Prefabs.daggerPickup))
                {
                    ExtraSkillLocator extraskills = damageReport.attackerBody.GetComponent<ExtraSkillLocator>();

                    if (damageReport.attackerBody.bodyIndex == MainPlugin.katIndex && damageReport.attackerBody)
                    {
                        damageReport.attackerBody.AddTimedBuff(DLC1Content.Buffs.KillMoveSpeed, 2f);
                    }
                    
                    if (extraskills.extraThird.stock == 0)
                    {
                        extraskills.extraThird.AddOneStock();
                    }

                    if (extraskills.extraSecond.stock == 0)
                    {
                        extraskills.extraSecond.AddOneStock();
                    }
                    
                    if (damageReport.attackerBody.skillLocator && damageReport.attackerBody.skillLocator.secondary.stock == 0)
                    {
                        damageReport.attackerBody.skillLocator.secondary.AddOneStock();
                    }
                    
                    if (damageReport.attackerBody.skillLocator && damageReport.attackerBody.skillLocator.utility.stock == 0)
                    {
                        damageReport.attackerBody.skillLocator.utility.AddOneStock();
                    }
                    
                    if (MainPlugin.killexplosionfx.Value)
                    {
                        if (!MainPlugin.collapseEveryone.Value)
                        {
                            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect"), new EffectData {origin = damageInfo.position}, true);
                        }
                        else
                        {
                            EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BleedOnHitVoid/FractureImpactEffect.prefab").WaitForCompletion(), new EffectData {origin = damageInfo.position}, true);
                        }
                    }
                }
                
                //killed by melee attack
                if (damageReport.attackerBody.bodyIndex == MainPlugin.katIndex && damageInfo.damageType == DamageType.BonusToLowHealth)
                {
                    ExtraSkillLocator extraskills = damageReport.attackerBody.GetComponent<ExtraSkillLocator>();
                    
                    if (damageReport.attackerBody.skillLocator && damageReport.attackerBody.skillLocator.utility.stock == 0)
                    {
                        damageReport.attackerBody.skillLocator.utility.AddOneStock();
                    }

                    if (extraskills.extraThird.stock == 0)
                    {
                        extraskills.extraThird.AddOneStock();
                    }
                }
                
                //Recharge Special
                if (damageReport.attackerBody.bodyIndex == MainPlugin.katIndex && damageReport.attackerBody.skillLocator && damageReport.attackerBody.skillLocator.special && NetworkServer.active)
                {
                    damageReport.attackerBody.skillLocator.special.RunRecharge(GlobalValues.specialCDReductionOnKill);
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
                    if (damageInfo.damageType == DamageType.BypassArmor)
                    {
                        //attackerBody.healthComponent.Heal(damageInfo.damage * 0.03f, default(ProcChainMask));
                        //Debug.LogWarning("She really needs more healing idk what to do"); //TODO this
                    }
                }
            }
            
            // This is for M2 omnivamp
            var flag1 = damageInfo.HasModdedDamageType(Prefabs.blade1) || damageInfo.HasModdedDamageType(Prefabs.blade2) || damageInfo.HasModdedDamageType(Prefabs.blade3) || 
                         damageInfo.HasModdedDamageType(Prefabs.blade4) || damageInfo.HasModdedDamageType(Prefabs.blade5) || damageInfo.HasModdedDamageType(Prefabs.blade6);
            if (flag1 && NetworkServer.active)
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                attackerBody.healthComponent.Heal(damageInfo.damage * 0.08f, default(ProcChainMask)); //TODO M2 omnivamp
            }
            
            //Radial Slash healing
            if (damageInfo.HasModdedDamageType(Prefabs.daggerPickup))
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody && NetworkServer.active)
                { 
                    attackerBody.healthComponent.Heal(damageInfo.damage * 0.03f, default(ProcChainMask));
                }
            }

            //Botrk Shunpo
            if (damageInfo.HasModdedDamageType(Prefabs.blink))
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                float totaldamage;
                var enemypercenthealth = self.health * 0.10f; //TODO Botrk Enemy Health %
                float borkTotal = damageInfo.damage + enemypercenthealth;
                        
                if (self.body.isBoss)
                {
                    totaldamage = borkTotal * 0.70f;
                }
                else
                {
                    totaldamage = borkTotal;
                }
                damageInfo.damage = totaldamage;
                        
                float borkheal = damageInfo.damage * 0.07f; //TODO Botrk heal
                attackerBody.healthComponent.Heal(borkheal, default(ProcChainMask));
                        
                /*Debug.Log($"{canimath} is total damage -- 30 + {enemypercenthealth}");
                Debug.Log($"{borkheal} is  9 % healing");*/
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
    }
}

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
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;

namespace Katarina
{
    class BaseDaggerPickupState : BaseSkillState
    {
        private float duration = 1;
        private GameObject impactEffect = Prefabs.pickupfx;
        private GameObject aoeEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion();
        private float damageCoefficient = 4.2f; //TODO DaggerPickup Damage
        private float utilityCDToSet = 1f;
        private EffectIndex effectIndex
        {
            get
            {
                var effectComponent = impactEffect ? impactEffect.GetComponent<EffectComponent>() : null;
                return effectComponent ? effectComponent.effectIndex : EffectIndex.Invalid;
            }
        }
        public override void OnEnter()
        {
            base.OnEnter();

            if (base.characterBody.skillLocator && base.characterBody.skillLocator.utility)
            {
                if (base.characterBody.skillLocator.utility.cooldownRemaining >= utilityCDToSet && base.isAuthority)
                {
                    base.characterBody.skillLocator.utility.RunRecharge(base.characterBody.skillLocator.utility.finalRechargeInterval - utilityCDToSet);
                }
            }
            base.PlayAnimation("FullBody, Override", "DaggerPickup");
            AkSoundEngine.PostEvent(3507441748, base.gameObject);
            EffectManager.SimpleEffect(aoeEffect, base.characterBody.footPosition, Quaternion.identity, false);
            if (base.isAuthority)
            {
                var pickup = new BlastAttack();
                pickup.position = base.characterBody.corePosition;
                pickup.attacker = base.characterBody.gameObject;
                pickup.inflictor = base.characterBody.gameObject;
                pickup.attackerFiltering = AttackerFiltering.NeverHitSelf;
                pickup.baseDamage = base.characterBody.damage * damageCoefficient;
                pickup.baseForce = 150;
                pickup.falloffModel = BlastAttack.FalloffModel.None;
                pickup.radius = GlobalValues.daggerPickupExplosionRadius;
                pickup.procCoefficient = 1;
                pickup.impactEffect = effectIndex;
                pickup.AddModdedDamageType(Prefabs.daggerPickup);
                pickup.teamIndex = base.characterBody.teamComponent
                    ? base.characterBody.teamComponent.teamIndex
                    : TeamIndex.Player;
                pickup.Fire();
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
    class DaggerPickupAir : BaseDaggerPickupState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (base.characterBody.skillLocator && base.characterBody.skillLocator.secondary && !base.characterMotor.isGrounded)
            {
                base.characterBody.skillLocator.secondary.AddOneStock();
            }
        }
    }
}

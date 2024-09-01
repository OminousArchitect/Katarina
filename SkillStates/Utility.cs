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
    class Utility : BaseSkillState
    {
        private BladeController component;
        private ExtraInputBankTest inputbank2;

        protected virtual void NextState()
        {
            this.outer.SetNextState(new Blink());
        }

        protected virtual void CreateAreaIndicator()
        {
            if (component)
            {
                component.InstantiateAreaIndicator();
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            component = base.GetComponent<BladeController>();
            inputbank2 = outer.GetComponent<ExtraInputBankTest>();
            CreateAreaIndicator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //We instantiate and destroy the indicator all through the component in the character so it's properly networked. 
            //Since we only leave the skill using the check !base.inputBank.skill3.down, only the client will leave the state. 
            //The state stuff is properly networked by HG, so it will leave the state for other clients too. But our stuff isn't, so we instantiate it through the component and in the next state we destroy it, also through the component.
            if (component)
            {
                component.UpdateAreIndicatorPosition(base.GetAimRay());
            }

            //if (base.inputBank && !base.inputBank.skill3.down && base.isAuthority)
            if (inputbank2 && !inputbank2.extraSkill3.down && base.isAuthority)
            {
                NextState();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            base.PlayAnimation("FullBody, Override", "Shunpo");
            AkSoundEngine.PostEvent(2674848417, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }

    class Blink : MeleeSkillState
    {
        private float immunityDuration = 0.9f;
        private SphereSearch sphereSearch = new SphereSearch();
        private BladeController bladeController;
        protected KatarinaTracker katTracker;
        protected Vector3 blinkPosition;
        protected CharacterBody enemybody;
        private float radius = 8;
        private float coefficient;

        private GameObject silentCollapse = Prefabs.silentcollapseEffect; //Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BleedOnHitVoid/FractureImpactEffect.prefab").WaitForCompletion();
        
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, immunityDuration);
            }
            
            bladeController = base.GetComponent<BladeController>();
            SetBlinkPosition();
            if (base.isAuthority)
            {
                TeleportHelper.TeleportBody(base.characterBody, blinkPosition);
            }
            this.sphereSearch = new SphereSearch();
            this.sphereSearch.origin = blinkPosition;
            this.sphereSearch.radius = radius;
            this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
            
            var hurtbox = sphereSearch.RefreshCandidates()
                .FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(TeamIndex.Player)).OrderCandidatesByDistance()
                .FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes()[0];
            if (hurtbox && hurtbox.healthComponent)
            {
                bool flag1 = hurtbox.healthComponent.body.isFlying;
                bool flag2 = hurtbox.healthComponent.body.bodyIndex == MainPlugin.vultureIndex;
                bool flag3 = hurtbox.healthComponent.body.bodyIndex == MainPlugin.pestIndex;

                if (flag1 || flag2 || flag3)
                {
                    coefficient = MainPlugin.ihateflyingenemies.Value; //TODO Shunpo flying damage
                    FreezeVelocity();
                }

                else
                {
                    coefficient = MainPlugin.blinkdmg.Value; //TODO Shunpo damage
                }

                if (NetworkServer.active)
                {
                    DamageInfo damageInfo = new DamageInfo()
                    {
                        attacker = base.characterBody.gameObject,
                        inflictor = base.characterBody.gameObject,
                        damage = base.characterBody.damage * coefficient,
                        procCoefficient = 1.3f,
                        damageType = DamageType.Generic,
                        damageColorIndex = DamageColorIndex.Default,
                        position = hurtbox.transform.position,
                        rejected = false,
                    };
                    damageInfo.AddModdedDamageType(Prefabs.blink);
                    hurtbox.healthComponent.TakeDamage(damageInfo);
                }

                if (MainPlugin.shunpodamagefx.Value)
                {
                    EffectManager.SimpleEffect(silentCollapse, hurtbox.healthComponent.body.corePosition, Quaternion.identity, false);
                }
            }
        }

        protected virtual void SetBlinkPosition()
        {
            if (bladeController)
            {
                blinkPosition = bladeController.areaIndicatorPosition; //arrival
                bladeController.DestroyAreaIndicator();
                
                if (MainPlugin.shunpofx.Value)
                {
                    bool loud = MainPlugin.loudshunpo.Value;
                    if (loud)
                    {
                        EffectManager.SimpleEffect(Prefabs.shunpofx, blinkPosition,
                            Quaternion.identity, false);
                    }
                    else
                    {
                        EffectManager.SimpleEffect(Prefabs.silentshunpofx, blinkPosition,
                            Quaternion.identity, false);
                    }
                }
            }
        }

        protected virtual void FreezeVelocity()
        {

        }

        protected virtual void DoAimEffects()
        {
            if (MainPlugin.shunpofx.Value)
            {
                if (MainPlugin.loudshunpo.Value)
                {
                    EffectManager.SimpleEffect(Prefabs.shunpofx, base.characterBody.corePosition,
                        Quaternion.identity, false);
                }
                else
                {
                    EffectManager.SimpleEffect(Prefabs.silentshunpofx, base.characterBody.corePosition,
                        Quaternion.identity, false); //aim
                }
            }
        }
        
        public override void OnExit()
        {
            DoAimEffects();
        }
    }

    class AltUtility : Utility
    {
        protected override void CreateAreaIndicator()
        {

        }

        protected override void NextState()
        {
            this.outer.SetNextState(new BlinkTarget());
        }
    }

    class BlinkTarget : Blink
    {
        protected override void SetBlinkPosition()
        {
            katTracker = base.GetComponent<KatarinaTracker>();
            if (katTracker)
            {
                var target = katTracker.GetTrackingTarget();
                
                if (target)
                { 
                    enemybody = target.healthComponent.GetComponent<CharacterBody>();
                    var inputbank = enemybody.inputBank;

                    if (inputbank)
                    {
                        Ray enemylook = inputbank.GetAimRay();
                        blinkPosition = enemylook.origin + enemylook.direction * 2f;
                        DontBeZero();
                    }
                    else
                    {
                        blinkPosition = target ? target.transform.position : base.characterBody.corePosition;
                    }
                }
            }
        }

        private void DontBeZero()
        {
            if (blinkPosition == Vector3.zero)
            {
                blinkPosition = base.characterBody.corePosition;
            }
        }
        
        private float mercsmallhop = EntityStates.Merc.Assaulter.smallHopVelocity;
        protected override void FreezeVelocity()
        {
            if (!base.characterMotor.isGrounded)
            {
                base.SmallHop(base.characterMotor, mercsmallhop + 2f);
            }
            PlayAnimation("FullBody, Override", "DaggerPickup");
        }
    }
}
            
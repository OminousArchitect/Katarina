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
    class Primary : MeleeSkillState
    {
        private float duration;
        private float baseDuration = 0.65f;
        private float meleeDamage = 2.1f; //TODO Melee Damage
        private float minimumSwingDelay = 0.25f;
        private Transform modelTransform;
        private GameObject hitEffect;
        private GameObject slashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ThrowShiv.prefab").WaitForCompletion();
        private static bool hasAttacked;
        private string RightHand = "hand.r";
        private string LeftHand = "hand.l";
        private static bool slashed;

        public override void OnEnter()
        {
            base.OnEnter();
            
            base.StartAimMode();
            duration = baseDuration / base.attackSpeedStat;
            if (MainPlugin.silentslash.Value)
            {
                hitEffect = Prefabs.silentslashfx;
            }
            else
            {
                hitEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion();
            }
            this.damage = meleeDamage;
            this.animParameter = "Slash.playbackRate";
            modelTransform = base.GetModelTransform();
            hitEffectPrefab = hitEffect;
            if (modelTransform)
            {
                this.hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Primary");
            }
            SlashAnim();
            attack = NewOverlapAttack();
            attack.damageType = DamageType.BonusToLowHealth;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate(); 
            if (this.stopwatch >= minimumSwingDelay / base.attackSpeedStat && attack != null && !hasAttacked)
            {
                if (base.isAuthority)
                {
                    this.hitCallback = this.attack.Fire();
                }
            }
            if (attack != null && this.stopwatch >= 0.5f)
            {
                hasAttacked = false;
            }
            if (this.stopwatch >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public void SlashAnim()
        {
            if (!slashed)
            {
                slashed = true;
                base.PlayAnimation("Gesture, Override", "Slash1", "Slash.playbackRate", this.duration);
                EffectManager.SimpleMuzzleFlash(slashEffect, base.gameObject, LeftHand, false);
                AkSoundEngine.PostEvent(3386040098, base.gameObject);
                if (MainPlugin.enablemeleevoice.Value)
                {
                    AkSoundEngine.PostEvent(1226218464, base.gameObject);
                }
                //Debug.Log("----------------Left----------------");
            }
            else
            {
                slashed = false;
                base.PlayAnimation("Gesture, Override", "Slash2", "Slash.playbackRate", this.duration);
                EffectManager.SimpleMuzzleFlash(slashEffect, base.gameObject, RightHand, false);
                AkSoundEngine.PostEvent(3386040098, base.gameObject);
                if (MainPlugin.enablemeleevoice.Value)
                {
                    AkSoundEngine.PostEvent(1226218464, base.gameObject);
                }                
                //Debug.Log("----------------Right----------------");
            }
            
        }
        
        public override void OnExit()
        {
            base.OnExit();
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}

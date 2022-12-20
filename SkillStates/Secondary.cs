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

namespace Katarina
{
    class Secondary : BaseSkillState
    {
        private float maxCharge = 0.42f;
        private float stopwatch;
        private bool done;
        private bool configenabled;
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (MainPlugin.chargemechanic.Value)
            {
                configenabled = stopwatch >= maxCharge; //&& !base.inputBank.skill2.down;
            }
            else
            {
                configenabled = stopwatch >= maxCharge && !base.inputBank.skill2.down;
            }
            
            if (base.inputBank && base.inputBank.skill2.down)
            {
                stopwatch += Time.fixedDeltaTime;
            }
            
            if (stopwatch > maxCharge && !done) 
            {
                done = true;
                //Debug.Log("Charged");
                AkSoundEngine.PostEvent("Play_DaggersCharged", base.gameObject);
                base.PlayAnimation("Gesture, Override", "SecondaryCharged");
            }

            if (base.inputBank && !base.inputBank.skill2.down && stopwatch < maxCharge && base.isAuthority)
            {
                this.outer.SetNextState(new ThrowSingleDagger());
                return;
            }
            if (configenabled && base.isAuthority)
            {
                if (MainPlugin.resetslashalt.Value)
                {
                    this.outer.SetNextState(new BaseDaggerPickupState());
                }
                else
                {
                    this.outer.SetNextState(new ThrowGenjiDaggers());
                    return;
                }

            }
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
    class ThrowSingleDagger : BaseSkillState
    {
        private float duration = 1;
        public GameObject projectilePrefab;
        public float damageCoefficient = 3.5f; //TODO SingleDagger Damage
        private KatarinaTracker tracker;
        private CharacterBody target;
        
        public override void OnEnter()
        {
            base.OnEnter();
            RngPrefab();
            FireProjectile();
        }
        protected virtual void RngPrefab()
        {
            var rng = RoR2Application.rng.RangeInt(1, 7);
            base.GetComponent<BladeController>().SetBlade((BladeController.BladeIndex)rng);
            switch (rng)
            {
                case 1:
                    if (MainPlugin.randomdaggers.Value)
                    {
                        projectilePrefab = Prefabs.dagger3ProjectileThrow;
                    }
                    else
                    {
                        projectilePrefab = Prefabs.dagger2ProjectileThrow;
                    }
                    //Debug.Log("Setting Blade 1");
                    break;
                
                case 2:
                    if (MainPlugin.randomdaggers.Value)
                    {
                        projectilePrefab = Prefabs.dagger4ProjectileThrow;
                    }
                    else
                    {
                        projectilePrefab = Prefabs.dagger6ProjectileThrow;
                    }                    
                    //Debug.Log("Setting Blade 2");
                    break;
                
                case 3:
                    if (MainPlugin.randomdaggers.Value)
                    {
                        projectilePrefab = Prefabs.dagger1ProjectileThrow;
                    }
                    else
                    {
                        projectilePrefab = Prefabs.dagger4ProjectileThrow;
                    }                    
                    //Debug.Log("Setting Blade 3");
                    break;
                
                case 4:
                    if (MainPlugin.randomdaggers.Value)
                    {
                        projectilePrefab = Prefabs.dagger5ProjectileThrow;
                    }
                    else
                    {
                        projectilePrefab = Prefabs.dagger3ProjectileThrow;
                    }                    
                    //Debug.Log("Setting Blade 4");
                    break;
                
                case 5:
                    if (MainPlugin.randomdaggers.Value)
                    {
                        projectilePrefab = Prefabs.dagger6ProjectileThrow;
                    }
                    else
                    {
                        projectilePrefab = Prefabs.dagger5ProjectileThrow;
                    }                    
                    //Debug.Log("Setting Blade 5");
                    break;
                
                case 6:
                    if (MainPlugin.randomdaggers.Value)
                    {
                        projectilePrefab = Prefabs.dagger2ProjectileThrow;
                    }
                    else
                    {
                        projectilePrefab = Prefabs.dagger1ProjectileThrow;
                    }                    
                    //Debug.Log("Setting Blade 6");
                    break;
            }
        }
        protected virtual void FireProjectile()
        {

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                ProjectileManager.instance.FireProjectile
                (
                    projectilePrefab,
                    aimRay.origin,
                    Util.QuaternionSafeLookRotation(aimRay.direction),
                    base.gameObject,
                    base.characterBody.damage * damageCoefficient,
                    120,
                    Util.CheckRoll(base.characterBody.crit, base.characterBody.master),
                    
                    DamageColorIndex.Default
                );
                base.PlayAnimation("Gesture, Override", "ThrowDagger");
                AkSoundEngine.PostEvent(730767624, base.gameObject);
                if (MainPlugin.enabledaggervoice.Value)
                {
                    AkSoundEngine.PostEvent(2921051050, base.gameObject);
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }
    }
    class ThrowGenjiDaggers : ThrowSingleDagger
    {
        private float GenjiDamage = 1.35f; //TODO GenjiDagger Damage

        protected override void RngPrefab()
        {
            // Swapping for the bleed dagger projectiles
            var rng = RoR2Application.rng.RangeInt(1, 7);
            base.GetComponent<BladeController>().SetBlade((BladeController.BladeIndex)rng);
            switch (rng)
            {
                case 1:
                    projectilePrefab = Prefabs.dagger1ProjectileThrowBleed;
                    break;
                case 2:
                    projectilePrefab = Prefabs.dagger2ProjectileThrowBleed;
                    break;
                case 3:
                    projectilePrefab = Prefabs.dagger3ProjectileThrowBleed;
                    break;
                case 4:
                    projectilePrefab = Prefabs.dagger4ProjectileThrowBleed;
                    break;
                case 5:
                    projectilePrefab = Prefabs.dagger5ProjectileThrowBleed;
                    break;
                case 6:
                    projectilePrefab = Prefabs.dagger6ProjectileThrowBleed;
                    break;
            }
        }
        protected override void FireProjectile()
        {
            var aimRay = base.GetAimRay();
            RngPrefab();
            NewProjectile(base.GetAimRay().direction);
            RngPrefab();
            NewProjectile(new Vector3(aimRay.direction.x + -0.15f, aimRay.direction.y, aimRay.direction.z /*+ -0.15f*/));
            //RngPrefab();
            NewProjectile(new Vector3(aimRay.direction.x + 0.15f, aimRay.direction.y, aimRay.direction.z /*+ 0.15f*/));
            
            base.PlayAnimation("Gesture, Override", "ThrowDagger");
            AkSoundEngine.PostEvent(730767624, base.gameObject);
        }
        void NewProjectile(Vector3 direction)
        {
            if (base.isAuthority)
            {
                ProjectileManager.instance.FireProjectile
                (projectilePrefab, 
                    base.FindModelChild("hand.r").position,
                    Util.QuaternionSafeLookRotation(direction), 
                    base.gameObject, base.characterBody.damage * GenjiDamage, 
                    120, 
                    Util.CheckRoll(base.characterBody.crit, base.characterBody.master), 
                    DamageColorIndex.Default);
            }
        }
    }
}

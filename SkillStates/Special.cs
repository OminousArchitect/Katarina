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
using RoR2.Orbs;
using UnityEngine.AddressableAssets;

namespace Katarina
{
    class Special : BaseSkillState
    {
        private float duration = 2.5f;
        private float stopwatch;
        private float damageCoefficient = 1.95f; //TODO Death Lotus Damage
        private float attackInterval;
        private float baseAttackInterval = 0.16f;
        private float radius = 33;
        private float procCoefficient = 0.2f;
        private float ATKSpeedPercentToEnemyRatio = 0.2f;
        private List<HurtBox> priorityTargets = new List<HurtBox>();
        private List<HurtBox> newTargets;
        private SphereSearch sphereSearch = new SphereSearch();
        private BladeController component;
        private int maxTargets
        {
            get
            {                          
                return Mathf.Clamp(5 + Mathf.FloorToInt((base.attackSpeedStat - 1) / ATKSpeedPercentToEnemyRatio), 5, 99);
            }
        }
        private GameObject orbEffect;
        
        public override void OnEnter()
        {
            base.OnEnter();

            component = base.GetComponent<BladeController>();
            if (component)
            {
                if (!MainPlugin.altlotusfx.Value)
                {
                    orbEffect = Prefabs.lotusfx;
                }
                else
                {
                    orbEffect = Prefabs.altlotusfx;
                }
                
                /*switch (component.bladeIndex)
                {
                    case BladeController.BladeIndex.blade1:
                        //RoR2/Base/LunarSkillReplacements/LunarDetonatorOrbEffect.prefab -Alternate orb ghost replacement (purple lunar bursts)
                        orbEffect = Prefabs.lotusfx;
                        break;
                    case BladeController.BladeIndex.blade2:
                        orbEffect = Prefabs.lotusfx;
                        break;
                    case BladeController.BladeIndex.blade3:
                        orbEffect = Prefabs.lotusfx;
                        break;
                    case BladeController.BladeIndex.blade4:
                        orbEffect = Prefabs.lotusfx;
                        break;
                    case BladeController.BladeIndex.blade5:
                        orbEffect = Prefabs.lotusfx;
                        break;
                    case BladeController.BladeIndex.blade6:
                        orbEffect = Prefabs.lotusfx;
                        break;
                }*/
            }

            if (base.characterMotor)
            {
                base.characterMotor.useGravity = false;
                base.characterMotor.isFlying = true;
            }
            
            attackInterval = baseAttackInterval / base.attackSpeedStat;
            this.sphereSearch = new SphereSearch();
            this.sphereSearch.origin = base.characterBody.corePosition;
            this.sphereSearch.radius = radius;
            this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
            base.PlayAnimation("FullBody, Override", "DeathLotus", "Lotus.playbackRate", duration);
            AkSoundEngine.PostEvent(911155720, base.gameObject);
            if (MainPlugin.enablelotusvoice.Value)
            {
                AkSoundEngine.PostEvent(2431438415, base.gameObject);
            }
            
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.ElephantArmorBoost);
            }
        }
        void UpdateTargets()
        {
            List<HurtBox> bosses = new List<HurtBox>();
            List<HurtBox> elites = new List<HurtBox>();
            List<HurtBox> airbornes = new List<HurtBox>();
            List<HurtBox> regulars = new List<HurtBox>();
            var hurtboxes = sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(TeamIndex.Player)).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();
            for (int i = 0; i < hurtboxes.Length; i++)
            {
                if (hurtboxes[i] && hurtboxes[i].healthComponent && hurtboxes[i].healthComponent.alive && hurtboxes[i].healthComponent.body)
                {
                    if (hurtboxes[i].healthComponent.body.isBoss)
                    {
                        bosses.Add(hurtboxes[i]);
                    }
                    else if (hurtboxes[i].healthComponent.body.isElite)
                    {
                        elites.Add(hurtboxes[i]);
                    }
                    else if (!hurtboxes[i].healthComponent.body.GetComponent<CharacterMotor>())
                    {
                        airbornes.Add(hurtboxes[i]);
                    }
                    else
                    {
                        regulars.Add(hurtboxes[i]);
                    }
                }
            }
            
            newTargets = new List<HurtBox>();
            newTargets.AddRange(bosses);
            newTargets.AddRange(elites);
            newTargets.AddRange(airbornes);
            newTargets.AddRange(regulars);
            priorityTargets = new List<HurtBox>();
            priorityTargets.AddRange(newTargets.Take(maxTargets));
        }
        void AttackTargets()
        {
            for (int i = 0; i < priorityTargets.Count; i++)
            {
                if (priorityTargets[i])
                {
                    if (NetworkServer.active)   
                    {
                        LotusBladeOrb orb = new LotusBladeOrb();
                        orb.origin = base.characterBody.corePosition;
                        orb.target = priorityTargets[i];
                        orb.speed = GlobalValues.specialProjectileSpeed;
                        orb.owner = base.gameObject;
                        orb.damage = base.damageStat * damageCoefficient;
                        orb.orbEffect = orbEffect;
                        orb.procCoefficient = procCoefficient;
                        OrbManager.instance.AddOrb(orb);
                    }
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= attackInterval)
            {
                stopwatch = 0;
                base.characterMotor.velocity = Vector3.zero;
                UpdateTargets();
                AttackTargets();
            }
            //Checking if any button is down to cancel the ability. Also checking if it's .5 secs after it has been activated, to prevent it cancelling itself.
            bool minTime = base.fixedAge >= 0.5f;
            bool buttonDown = base.inputBank.skill1.down || base.inputBank.skill2.down || base.inputBank.skill3.down || base.inputBank.skill4.down;
            bool flag = minTime && buttonDown;
            if (base.fixedAge >= this.duration || flag && base.isAuthority)
            {
                base.PlayAnimation("FullBody, Override", "EndLotus");
                //Debug.Log("Exit");
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.ElephantArmorBoost);
            }
            
            if (base.characterMotor)
            {
                base.characterMotor.useGravity = true;
                base.characterMotor.isFlying = false;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}

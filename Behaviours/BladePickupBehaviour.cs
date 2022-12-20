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
    class BladePickupBehaviour : MonoBehaviour, IProjectileImpactBehavior
    {
        private bool inAir = true;
        private SphereSearch sphereSearch = new SphereSearch();
        private float stopwatch;
        private float searchInterval = 0.5f;
        private ProjectileController controller;
        private GameObject owner;
        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (impactInfo.collider && !impactInfo.collider.GetComponent<HurtBox>())
            {
                inAir = false;
            }
        }
        private void OnEnable()
        {
            this.sphereSearch = new SphereSearch();
            this.sphereSearch.origin = base.transform.position;
            this.sphereSearch.radius = GlobalValues.daggerPickupRadius;
            this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
        }
        private void FixedUpdate()
        {
            this.stopwatch += Time.fixedDeltaTime;
            //Gotta add it on fixedupdate bc it takes a few frames for it to set or some shit
            if (!controller || !owner)
            {
                controller = base.GetComponent<ProjectileController>();
                if (controller && controller.owner)
                {
                    owner = controller.owner;
                }
            }
            if (!owner)
            {
                return;
            }
            var stateMachine = Array.Find<EntityStateMachine>(owner.GetComponents<EntityStateMachine>(), (EntityStateMachine element) => element.customName == "Speen");
            float distance = Vector3.Distance(base.transform.position, owner.transform.position);
            if (distance <= GlobalValues.daggerPickupRadius)
            {
                if (stateMachine)
                {
                    var motor = owner.GetComponent<CharacterMotor>();
                    if (inAir)
                    {
                        stateMachine.SetInterruptState(new DaggerPickupAir(), InterruptPriority.Frozen);
                    }
                    else
                    {
                        stateMachine.SetInterruptState(new BaseDaggerPickupState(), InterruptPriority.Frozen);
                    }
                }
                UnityEngine.Object.Destroy(base.gameObject);
            }
            // Use this if: you want any Katarina doing the attack no matter who threw the dagger. Otherwise use the one above.
            /*if (stopwatch >= searchInterval)
            {
                stopwatch = 0;
                var hurtboxes = sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(TeamIndex.Player)).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();
                for (int i = 0; i < hurtboxes.Length; i++)
                {
                    if (hurtboxes[i] && hurtboxes[i].healthComponent && hurtboxes[i].healthComponent.alive && hurtboxes[i].healthComponent.body && hurtboxes[i].healthComponent.body.name.Contains(MainPlugin.SURVIVORNAME + "Body"))
                    {
                        if (stateMachine)
                        {
                            stateMachine.SetInterruptState(new DaggerPickupState(), InterruptPriority.Frozen);
                        }
                        UnityEngine.Object.Destroy(base.gameObject);
                    }
                }
            }*/
        }
    }
}

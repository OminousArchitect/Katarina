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
using RoR2.Orbs;
using RoR2.Networking;
using RoR2.CharacterAI;

namespace Katarina
{
    class LotusBladeOrb : Orb
    {
        public float speed;
        public float damage;
        internal GameObject owner;
        public GameObject orbEffect;
        public float procCoefficient;
        public override void Begin()
        {
            base.duration = base.distanceToTarget / this.speed;
            EffectData effectData = new EffectData
            {
                origin = this.origin,
                genericFloat = base.duration
            };
            effectData.SetHurtBoxReference(this.target);
            EffectManager.SpawnEffect(orbEffect, effectData, true);
        }
        public override void OnArrival()
        {
            if (this.target && this.target.healthComponent)
            {
                DamageInfo damageInfo = new DamageInfo()
                {
                    attacker = owner,
                    inflictor = owner,
                    damage = damage,
                    procCoefficient = procCoefficient,
                    damageType = DamageTypeExtended.Generic,
                    damageColorIndex = DamageColorIndex.Default,
                    position = this.target.transform.position,
                    rejected = false,
                };
                //damageInfo.AddModdedDamageType(Prefabs.blink);
                this.target.healthComponent.TakeDamage(damageInfo);
            }
        }
    }
}

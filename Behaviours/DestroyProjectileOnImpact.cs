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
    //Inheriting IProjectileImpactBehavior so it calls the OnProjectileImpact method 
    class DestroyProjectileOnImpact : MonoBehaviour, IProjectileImpactBehavior
    {
        public bool destroyOnEntity = true;
        public bool destroyOnTerrain = false;
        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            //true if collision object is an entity, false if terrain
            bool flag = impactInfo.collider.GetComponent<HurtBox>();
            if (flag && destroyOnEntity)
            {
                Destroy(base.gameObject);
            }
            else
            {

            }
        }
    }
}

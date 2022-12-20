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
    class KatarinaTracker : HuntressTracker
    {
        public bool enable = true;
        
        public bool canExecute
        {
            get
            {
                return base.trackingTarget ? true : false;
            }
        }

        private void Awake()
        {
            base.Awake();
            //indicator.visualizerInstance.GetComponent<SpriteRenderer>().color = Color.magenta;
        }

        private void FixedUpdate()
        {
            base.FixedUpdate();
            this.indicator.active = enable;
        }
    }
}

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

namespace Katarina
{
    class CharacterMain : GenericCharacterMain
    {
        private KatarinaTracker component;
        public override void OnEnter()
        {
            base.OnEnter();
            component = base.GetComponent<KatarinaTracker>();
        }
        public override void Update()
        {
            base.Update();
            if (component && base.skillLocator && base.skillLocator.utility && base.skillLocator.utility.skillDef)
            {
                component.enable = base.skillLocator.utility.skillDef.skillNameToken == MainPlugin.SURVIVORNAMEKEY + "ALT_UTIL" && base.skillLocator.utility.IsReady();
            }
        }
    }
}

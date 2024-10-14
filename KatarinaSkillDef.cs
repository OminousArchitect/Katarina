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
using JetBrains.Annotations;

namespace Katarina
{
    // Adding a new skilldef to enable/disable alt utility
    class KatarinaSkillDef : SkillDef
    {
        public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new KatarinaSkillDef.InstanceData
            {
                katarinaTracker = skillSlot.GetComponent<KatarinaTracker>()
            };
        }
        internal static bool IsExecutable([NotNull] GenericSkill skillSlot)
        {
            KatarinaTracker tracker = ((KatarinaSkillDef.InstanceData)skillSlot.skillInstanceData).katarinaTracker;
            return tracker.canExecute;
        }
        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return KatarinaSkillDef.IsExecutable(skillSlot) && base.CanExecute(skillSlot);
        }
        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && KatarinaSkillDef.IsExecutable(skillSlot);
        }
        class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public KatarinaTracker katarinaTracker;
        }
    }
}

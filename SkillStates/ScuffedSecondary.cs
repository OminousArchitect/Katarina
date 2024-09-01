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
    public class ScuffedSecondary : BaseSkillState
    {
        private float maxCharge = 0.25f;
        private float stopwatch;
        private bool done;
        public override void FixedUpdate()
        {
            base.FixedUpdate();

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
                this.outer.SetNextState(new BaseDaggerPickupState());
                return;
            }
            
            bool charged = stopwatch >= maxCharge; //&& !base.inputBank.skill2.down;
            if (charged && base.isAuthority)
            {
                this.outer.SetNextState(new BaseDaggerPickupState());
            }
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
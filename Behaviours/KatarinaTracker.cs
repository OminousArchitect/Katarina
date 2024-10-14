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
            private float stopwatch;
            private void Awake()
            {
                indicator = new Indicator(base.gameObject, Prefabs.aimIndicator);
            }
            private void Start()
            {
                maxTrackingDistance = 370;
                search.viewer = null;
                search.filterByLoS = false;
                base.Start();
            }
            private void FixedUpdate()
            {
                trackerUpdateStopwatch = 10;
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= 1f / trackerUpdateFrequency)
                {
                    stopwatch -= 1f / trackerUpdateFrequency;
                    Ray aimRay = new Ray(inputBank.aimOrigin, inputBank.aimDirection);
                    SearchForTargetNoLos(aimRay);
                    indicator.targetTransform = (trackingTarget ? trackingTarget.transform : null);
                }
            }
            private void SearchForTargetNoLos(Ray aimRay)
            {
                search.teamMaskFilter = TeamMask.GetUnprotectedTeams(teamComponent.teamIndex);
                search.filterByLoS = false;
                search.searchOrigin = aimRay.origin;
                search.searchDirection = aimRay.direction;
                search.sortMode = BullseyeSearch.SortMode.Distance;
                search.maxDistanceFilter = maxTrackingDistance;
                search.maxAngleFilter = 15f;
                search.RefreshCandidates();
                search.FilterOutGameObject(base.gameObject);
                trackingTarget = search.GetResults().FirstOrDefault<HurtBox>();
            }
        }
}

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
    class BladeController : MonoBehaviour
    {
        private GameObject areaIndicator;
        public Vector3 areaIndicatorPosition
        {
            get
            {
                return areaIndicator ? areaIndicator.transform.position : Vector3.zero;
            }
        }
        public void InstantiateAreaIndicator()
        {
            areaIndicator = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressArrowRainIndicator.prefab").WaitForCompletion());
            areaIndicator.transform.localScale = Vector3.one * 3;
        }
        public void UpdateAreIndicatorPosition(Ray aimRay)
        {
            if (areaIndicator)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(aimRay, out raycastHit, GlobalValues.maxBlinkDistance, LayerIndex.world.mask | LayerIndex.entityPrecise.mask))
                {
                    areaIndicator.transform.position = raycastHit.point;
                    areaIndicator.transform.up = raycastHit.normal;
                }
                else
                {
                    areaIndicator.transform.position = aimRay.GetPoint(GlobalValues.maxBlinkDistance);
                }
            }
        }
        public void DestroyAreaIndicator()
        {
            if (areaIndicator)
            {
                Destroy(areaIndicator);
            }
        }
        
        private ChildLocator childLocator;
        private List<string> childNameList = new List<string>()
        {
            "blade1",
            "blade2",
            "blade3",
            "blade4",
            "blade5",
            "blade6",
        };
        public enum BladeIndex
        {
            blade1 = 1,
            blade2 = 2,
            blade3 = 3,
            blade4 = 4,
            blade5 = 5,
            blade6 = 6
        }
        private BladeController.BladeIndex blade = new BladeIndex();
        public BladeIndex bladeIndex
        {
            get
            {
                return blade;
            }
        }
        public void SetBlade(BladeController.BladeIndex index)
        {
            blade = index;
            UpdateModel();
        }
        private void Awake()
        {
            blade = BladeIndex.blade1;
            var modelLocator = base.GetComponent<ModelLocator>();
            if (modelLocator && modelLocator.modelTransform)
            {
                childLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
            }
        }
        private void UpdateModel()
        {
            if (!childLocator)
            {
                return;
            }
            string childName = blade.ToString();
            foreach (string s in childNameList)
            {
                if (s == childName)
                {
                    childLocator.FindChild(childName).gameObject.SetActive(true);
                }
                else
                {
                    childLocator.FindChild(s).gameObject.SetActive(false);
                }
            }
        }
    }
}

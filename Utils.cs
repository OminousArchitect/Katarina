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
using HG;
using System.Runtime.InteropServices;
using UnityEngine.Events;

namespace SurvivorTemplate
{
    class Utils
    {
        internal static void RegisterEffect(GameObject effect, float duration, string soundName = "")
        {
            var effectcomponent = effect.GetComponent<EffectComponent>();
            if (!effectcomponent)
            {
                effectcomponent = effect.AddComponent<EffectComponent>();
            }
            if (!effect.GetComponent<DestroyOnTimer>())
            {
                effect.AddComponent<DestroyOnTimer>().duration = duration;
            }
            if (!effect.GetComponent<NetworkIdentity>())
            {
                effect.AddComponent<NetworkIdentity>();
            }
            if (!effect.GetComponent<VFXAttributes>())
            {
                effect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            }
            effectcomponent.applyScale = false;
            effectcomponent.effectIndex = EffectIndex.Invalid;
            effectcomponent.parentToReferencedTransform = true;
            effectcomponent.positionAtReferencedTransform = true;
            effectcomponent.soundName = soundName;
            ContentAddition.AddEffect(effect);
        }
        public static Material InstantiateMaterial(Color color, Texture tex, Color emColor, float emPower, Texture emTex, float normStr, Texture normTex)
        {
            Material mat = UnityEngine.Object.Instantiate<Material>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            if (mat)
            {
                mat.SetColor("_Color", color);
                mat.SetTexture("_MainTex", tex);
                mat.SetColor("_EmColor", emColor);
                mat.SetFloat("_EmPower", emPower);
                mat.SetTexture("_EmTex", emTex);
                mat.SetFloat("_NormalStrength", 1f);
                mat.SetTexture("_NormalTex", normTex);
                return mat;
            }
            return mat;
        }
        public static Material FindMaterial(string name)
        {
            Material[] objs = Resources.FindObjectsOfTypeAll<Material>() as Material[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i];
                    }
                }
            }
            return null;
        }
    }
}

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
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;

namespace Katarina
{
    class Utils
    {
        private static Material defaultIndicatorMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matAreaIndicatorRim.mat").WaitForCompletion();
        internal static ModdedDamageTypeHolderComponent SwapModdedDamageType(GameObject obj, ModdedDamageType moddedDamageType)
        {
            var component = obj.GetComponent<ModdedDamageTypeHolderComponent>();
            component.Remove(Prefabs.blade1);
            component.Add(moddedDamageType);
            return component;
        }
        internal static GameObject CreateNewColoredIndicator(GameObject obj, Transform target, Color color)
        {
            Material newIndicatorMat = UnityEngine.Object.Instantiate(defaultIndicatorMat);
            newIndicatorMat.SetColor("_TintColor", color);

            var newObj = UnityEngine.Object.Instantiate(obj, target);
            newObj.transform.localPosition = Vector3.zero;
            newObj.transform.localScale = Vector3.one * GlobalValues.daggerPickupRadius;
            newObj.GetComponentInChildren<MeshRenderer>().material = newIndicatorMat;
            return newObj;
        }
        internal static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
        public static Sprite CreateSpriteFromTexture(Texture2D texture)
        {
            if (texture)
            {
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                return sprite;
            }
            return null;
        }
        public static GameObject FindInActiveObjectByName(string name)
        {
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i].gameObject;
                    }
                }
            }
            return null;
        }
        public static GameObject CreateHitbox(string name, Transform parent, Vector3 scale)
        {
            var hitboxTransform1 = new GameObject(name);
            hitboxTransform1.transform.SetParent(parent);
            hitboxTransform1.transform.localPosition = Vector3.zero;
            hitboxTransform1.transform.localRotation = Quaternion.identity;
            hitboxTransform1.transform.localScale = scale;
            var hitBoxGroup1 = parent.gameObject.AddComponent<HitBoxGroup>();
            HitBox hitBox = hitboxTransform1.AddComponent<HitBox>();
            hitboxTransform1.layer = LayerIndex.projectile.intVal;
            hitBoxGroup1.hitBoxes = new HitBox[] { hitBox };
            hitBoxGroup1.groupName = name;
            return hitboxTransform1;
        }
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

using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using EntityStates;
using RoR2;
using BepInEx.Configuration;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using KinematicCharacterController;
using RoR2.UI;
using RoR2.Projectile;

namespace SurvivorTemplate
{
    class ItemDisplays
    {
        internal static ItemDisplayRuleSet itemDisplayRuleSet;
        internal static List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules;
        private static Dictionary<string, GameObject> itemDisplayPrefabs = new Dictionary<string, GameObject>();

        internal static void PopulateDisplays()
        {
            PopulateFromBody("Commando");
            PopulateFromBody("Croco");
            PopulateFromBody("Mage");
        }
        private static void PopulateFromBody(string bodyName)
        {
            ItemDisplayRuleSet itemDisplayRuleSet = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/" + bodyName + "Body").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;

            ItemDisplayRuleSet.KeyAssetRuleGroup[] item = itemDisplayRuleSet.keyAssetRuleGroups;

            for (int i = 0; i < item.Length; i++)
            {
                ItemDisplayRule[] rules = item[i].displayRuleGroup.rules;

                for (int j = 0; j < rules.Length; j++)
                {
                    GameObject followerPrefab = rules[j].followerPrefab;
                    if (followerPrefab)
                    {
                        string name = followerPrefab.name;
                        string key = name?.ToLower();
                        if (!itemDisplayPrefabs.ContainsKey(key))
                        {
                            itemDisplayPrefabs[key] = followerPrefab;
                        }
                    }
                }
            }
        }
        public static void RegisterDisplays()
        {
            itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
            itemDisplayRuleSet.name = MainPlugin.SURVIVORNAME + "IDRS";

            GameObject characterPrefab = MainPlugin.characterPrefab;
            GameObject gameObject = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel component = gameObject.GetComponent<CharacterModel>();
            component.itemDisplayRuleSet = itemDisplayRuleSet;
        }
        public static void SetIDRS()
        {
            itemDisplayRules = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();

            //NewEquipIDRS(RoR2Content.Equipment.GoldGat, "DisplayGoldGat", "chest", new Vector3(0.01104F, 0.02289F, -0.00421F), new Vector3(6.21242F, 82.78071F, 304.3307F), new Vector3(0.003F, 0.003F, 0.003F));
            //NewEquipIDRS(RoR2Content.Equipment.Jetpack, "DisplayBugWings", "chest", new Vector3(0F, 0.00687F, -0.01029F), new Vector3(0, 0F, 0F), new Vector3(0.006F, 0.006F, 0.006F));

            itemDisplayRuleSet.keyAssetRuleGroups = itemDisplayRules.ToArray();
            itemDisplayRuleSet.GenerateRuntimeValues();
        }
        private static void NewEquipIDRS(EquipmentDef equipDef, string equipName, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)
        {
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = equipDef,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay(equipName),
                            childName = childName,
                            localPos = localPos,
                            localAngles = localAngles,
                            localScale = localScale,
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
        }
        internal static GameObject LoadDisplay(string name)
        {
            if (itemDisplayPrefabs.ContainsKey(name.ToLower()))
            {
                if (itemDisplayPrefabs[name.ToLower()]) return itemDisplayPrefabs[name.ToLower()];
            }

            return null;
        }
    }
}

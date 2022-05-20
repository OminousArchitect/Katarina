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
        private static Dictionary<UnityEngine.Object, GameObject> itemDisplayPrefabs = new Dictionary<UnityEngine.Object, GameObject>();
        private static Vector3 vec = Vector3.one;
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
                        ItemDef itemDef = item[i].keyAsset as ItemDef;
                        EquipmentDef equipDef = item[i].keyAsset as EquipmentDef;
                        if (itemDef != null)
                        {
                            if (!itemDisplayPrefabs.ContainsKey(itemDef))
                            {
                                itemDisplayPrefabs.Add(itemDef, followerPrefab);
                            }
                        }
                        if (equipDef != null)
                        {
                            if (!itemDisplayPrefabs.ContainsKey(equipDef))
                            {
                                itemDisplayPrefabs.Add(equipDef, followerPrefab);
                            }
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

            NewIDRS(RoR2Content.Items.AlienHead, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ArmorPlate, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ArmorReductionOnHit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ArtifactKey, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.AttackSpeedOnCrit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.AutoCastEquipment, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Bandolier, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.BarrierOnKill, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.BarrierOnOverHeal, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Bear, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.BeetleGland, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Behemoth, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.BleedOnHit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.BleedOnHitAndExplode, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.BonusGoldPackOnKill, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.BossDamageBonus, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.BounceNearby, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.CaptainDefenseMatrix, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ChainLightning, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Clover, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.CrippleWardOnLevel, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.CritGlasses, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Crowbar, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Dagger, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.DeathMark, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.DrizzlePlayerHelper, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.EnergizedOnEquipmentUse, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.EquipmentMagazine, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ExecuteLowHealthElite, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ExplodeOnDeath, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ExtraLife, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.FallBoots, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Feather, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.FireballsOnHit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.FireRing, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Firework, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.FlatHealth, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.FocusConvergence, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Ghost, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.GhostOnKill, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.GoldOnHit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.HeadHunter, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.HealOnCrit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.HealthDecay, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.HealWhileSafe, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Hoof, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.IceRing, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Icicle, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.IgniteOnKill, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.IncreaseHealing, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Infusion, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.InvadingDoppelganger, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.JumpBoost, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.KillEliteFrenzy, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Knurl, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.LaserTurbine, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.LightningStrikeOnHit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.LunarBadLuck, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.LunarDagger, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.LunarPrimaryReplacement, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.LunarSecondaryReplacement, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.LunarSpecialReplacement, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.LunarTrinket, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.LunarUtilityReplacement, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Medkit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.MinionLeash, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Missile, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.MonstersOnShrineUse, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Mushroom, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.NearbyDamageBonus, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.NovaOnHeal, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.NovaOnLowHealth, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ParentEgg, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Pearl, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.PersonalShield, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Phasing, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Plant, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.RandomDamageZone, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.RepeatHeal, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.RoboBallBuddy, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.SecondarySkillMagazine, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Seed, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ShieldOnly, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ShinyPearl, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.ShockNearby, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.SiphonOnLowHealth, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.SlowOnHit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.SprintArmor, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.SprintBonus, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.SprintOutOfCombat, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.SprintWisp, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Squid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.StickyBomb, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.StunChanceOnHit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.SummonedEcho, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Syringe, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Talisman, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Thorns, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.TitanGoldDuringTP, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.TonicAffliction, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.Tooth, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.TPHealingNova, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.TreasureCache, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.UseAmbientLevel, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.UtilitySkillMagazine, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.WarCryOnMultiKill, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Items.WardOnLevel, "chest", Vector3.one, Vector3.one, Vector3.one);

            NewIDRS(DLC1Content.Items.AttackSpeedAndMoveSpeed, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.BearVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.BleedOnHitVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.ChainLightningVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.CloverVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.ConvertCritChanceToCritDamage, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.CritDamage, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.CritGlassesVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.DroneWeapons, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.DroneWeaponsBoost, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.DroneWeaponsDisplay1, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.DroneWeaponsDisplay2, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.ElementalRingVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.EquipmentMagazineVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.ExplodeOnDeathVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.ExtraLifeVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.FragileDamageBonus, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.FreeChest, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.GoldOnHurt, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.GummyCloneIdentifier, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.HalfAttackSpeedHalfCooldowns, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.HalfSpeedDoubleHealth, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.HealingPotion, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.ImmuneToDebuff, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.LunarSun, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.MinorConstructOnKill, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.MissileVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.MoreMissile, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.MoveSpeedOnKill, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.MushroomVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.OutOfCombatArmor, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.PermanentDebuffOnHit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.PrimarySkillShuriken, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.RandomEquipmentTrigger, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.RandomlyLunar, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.RegeneratingScrap, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.SlowOnHitVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.StrengthenBurn, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.TreasureCacheVoid, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.VoidmanPassiveItem, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Items.VoidMegaCrabItem, "chest", Vector3.one, Vector3.one, Vector3.one);

            NewIDRS(RoR2Content.Equipment.AffixBlue, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.AffixEcho, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.AffixHaunted, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.AffixLunar, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.AffixPoison, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.AffixRed, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.AffixWhite, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.BFG, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Blackhole, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.BurnNearby, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Cleanse, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.CommandMissile, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.CrippleWard, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.CritOnUse, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.DeathProjectile, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.DroneBackup, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.FireBallDash, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Fruit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.GainArmor, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Gateway, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.GoldGat, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Jetpack, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.LifestealOnHit, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Lightning, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.LunarPotion, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Meteor, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.PassiveHealing, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.QuestVolatileBattery, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Recycle, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Saw, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Scanner, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.TeamWarCry, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(RoR2Content.Equipment.Tonic, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Equipment.BossHunter, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Equipment.EliteVoidEquipment, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Equipment.GummyClone, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Equipment.LunarPortalOnUse, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Equipment.Molotov, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Equipment.MultiShopCard, "chest", Vector3.one, Vector3.one, Vector3.one);
            NewIDRS(DLC1Content.Equipment.VendingMachine, "chest", Vector3.one, Vector3.one, Vector3.one);

            itemDisplayRuleSet.keyAssetRuleGroups = itemDisplayRules.ToArray();
            itemDisplayRuleSet.GenerateRuntimeValues();
        }
        private static void NewIDRS(UnityEngine.Object obj, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale, LimbFlags flags = LimbFlags.None)
        {
            var itemDef = obj as ItemDef;
            var equipDef = obj as EquipmentDef;
            if (itemDef != null)
            {
                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = itemDef,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                   {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay(itemDef),
                            childName = childName,
                            localPos = localPos,
                            localAngles = localAngles,
                            localScale = localScale,
                            limbMask = flags
                        }
                   }
                    }
                });
            }
            else
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
                            followerPrefab = ItemDisplays.LoadDisplay(equipDef),
                            childName = childName,
                            localPos = localPos,
                            localAngles = localAngles,
                            localScale = localScale,
                            limbMask = flags
                        }
                   }
                    }
                });
            }

        }
        internal static GameObject LoadDisplay(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                ItemDef itemDef = obj as ItemDef;
                EquipmentDef equipDef = obj as EquipmentDef;
                if (itemDef != null)
                {
                    if (itemDisplayPrefabs.ContainsKey(itemDef))
                    {
                        if (itemDisplayPrefabs[itemDef]) return itemDisplayPrefabs[itemDef];
                    }
                }
                if (equipDef != null)
                {
                    if (itemDisplayPrefabs.ContainsKey(equipDef))
                    {
                        if (itemDisplayPrefabs[equipDef]) return itemDisplayPrefabs[equipDef];
                    }
                }
            }

            return null;
        }
    }
}

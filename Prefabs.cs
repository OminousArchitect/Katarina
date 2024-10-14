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
using static R2API.DamageAPI;
using RoR2.EntityLogic;

namespace Katarina
{
    class Prefabs
    {
        internal static GameObject dagger1ProjectileThrow;
        internal static GameObject dagger2ProjectileThrow;
        internal static GameObject dagger3ProjectileThrow;
        internal static GameObject dagger4ProjectileThrow;
        internal static GameObject dagger5ProjectileThrow;
        internal static GameObject dagger6ProjectileThrow;

        internal static GameObject dagger1ProjectilePickup;
        internal static GameObject dagger2ProjectilePickup;
        internal static GameObject dagger3ProjectilePickup;
        internal static GameObject dagger4ProjectilePickup;
        internal static GameObject dagger5ProjectilePickup;
        internal static GameObject dagger6ProjectilePickup;

        internal static GameObject dagger1ProjectileThrowBleed;
        internal static GameObject dagger2ProjectileThrowBleed;
        internal static GameObject dagger3ProjectileThrowBleed;
        internal static GameObject dagger4ProjectileThrowBleed;
        internal static GameObject dagger5ProjectileThrowBleed;
        internal static GameObject dagger6ProjectileThrowBleed;
        
        internal static GameObject pickupfx;
        internal static GameObject lotusfx;
        internal static GameObject altlotusfx;
        internal static GameObject shunpofx;
        internal static GameObject silentshunpofx;
        internal static GameObject silentslashfx;
        internal static GameObject silentcollapseEffect;
        internal static GameObject aimIndicator;

        //gotta create a damagetype for each blade color bc we gotta check for it on healthcomponent.TakeDamage, in order to spawn the correct pickup dagger color
        internal static ModdedDamageType blade1;
        internal static ModdedDamageType blade2;
        internal static ModdedDamageType blade3;
        internal static ModdedDamageType blade4;
        internal static ModdedDamageType blade5;
        internal static ModdedDamageType blade6;
        internal static ModdedDamageType bladeBleed;
        
        internal static ModdedDamageType blink;
        internal static ModdedDamageType daggerPickup;
        private static float daggerMaxPickupTime = 5.5f;

        internal static void CreatePrefabs()
        {
            blade1 = ReserveDamageType();
            blade2 = ReserveDamageType();
            blade3 = ReserveDamageType();
            blade4 = ReserveDamageType();
            blade5 = ReserveDamageType();
            blade6 = ReserveDamageType();
            bladeBleed = ReserveDamageType();
            blink = ReserveDamageType();
            daggerPickup = ReserveDamageType();
            
            OrbAPI.AddOrb(typeof(LotusBladeOrb));
            
            //var targettracker = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressTrackingIndicator.prefab").WaitForCompletion();
            //targetindicator = PrefabAPI.InstantiateClone(targettracker, "katarinatracker", false);
            //.GetComponent<SpriteRenderer>().color = Color.magenta;
            
            var defaultIndicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressArrowRainIndicator.prefab").WaitForCompletion();

            var baseProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivProjectile.prefab").WaitForCompletion(), "BaseProjectile", false);
            baseProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
            baseProjectile.GetComponent<ProjectileSingleTargetImpact>().hitSoundString = null;
            baseProjectile.GetComponent<ProjectileSingleTargetImpact>().enemyHitSoundString = null;
            
            aimIndicator = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressTrackingIndicator.prefab").WaitForCompletion(), "KatAimIndicator", false);
            foreach (SpriteRenderer s in aimIndicator.GetComponentsInChildren<SpriteRenderer>())
            {
                float tH, tS, tV;
                Color.RGBToHSV(Color.magenta, out tH, out tS, out tV);

                float H, S, V;
                Color.RGBToHSV(s.color, out H, out S, out V);

                Color newColor = Color.HSVToRGB(tH, S, V);
                newColor.a = s.color.a;

                s.color = newColor;
            }
            
            //setup ghosts for pickup daggers
            var dagger1ProjectileGhost = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger1fab"), "KatarinaBlade1ProjectileGhost", false);
            dagger1ProjectileGhost.AddComponent<ProjectileGhostController>();
            Utils.CreateNewColoredIndicator(defaultIndicator, dagger1ProjectileGhost.transform, new Color(1f, 0f, 0.792156863f));

            var dagger2ProjectileGhost = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger2fab"), "KatarinaBlade2ProjectileGhost", false);
            dagger2ProjectileGhost.AddComponent<ProjectileGhostController>();
            Utils.CreateNewColoredIndicator(defaultIndicator, dagger2ProjectileGhost.transform, new Color(0.592156863f, 0f, 0.964705882f));

            var dagger3ProjectileGhost = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger3fab"), "KatarinaBlade3ProjectileGhost", false);
            dagger3ProjectileGhost.AddComponent<ProjectileGhostController>();
            Utils.CreateNewColoredIndicator(defaultIndicator, dagger3ProjectileGhost.transform, new Color(0f, 0.964705882f, 0.0823529412f));

            var dagger4ProjectileGhost = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger4fab"), "KatarinaBlade4ProjectileGhost", false);
            dagger4ProjectileGhost.AddComponent<ProjectileGhostController>();
            Utils.CreateNewColoredIndicator(defaultIndicator, dagger4ProjectileGhost.transform, new Color(1f, 0f, 0f));

            var dagger5ProjectileGhost = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger5fab"), "KatarinaBlade5ProjectileGhost", false);
            dagger5ProjectileGhost.AddComponent<ProjectileGhostController>();
            Utils.CreateNewColoredIndicator(defaultIndicator, dagger5ProjectileGhost.transform, new Color(0.0235294118f, 0.611764706f, 1f));

            var dagger6ProjectileGhost = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger6fab"), "KatarinaBlade6ProjectileGhost", false);
            dagger6ProjectileGhost.AddComponent<ProjectileGhostController>();
            Utils.CreateNewColoredIndicator(defaultIndicator, dagger6ProjectileGhost.transform, new Color(0.207843137f, 0.031372549f, 0.537254902f));

            //Here I create the first dagger projectile then clone it and change the ghostprefab.
            //I added a ProjectileImpactExplosion so it automatically destroys the projectile for me after it his the ground. The destroy time is the static float daggerMaxPickupTime created above.
            //Most stuff is pretty self explanatory, but I'm commenting anyway in case you miss it or something.

            //register pickup daggers
            dagger1ProjectilePickup = PrefabAPI.InstantiateClone(baseProjectile, "KatarinaBlade1ProjectilePickup", true);
            //UnityEngine.Object.Destroy(dagger1ProjectilePickup.GetComponent<ProjectileSingleTargetImpact>());
            //dagger1ProjectilePickup.GetComponent<ProjectileSingleTargetImpact>().hitSoundString = "Play_DaggerLand";
            dagger1ProjectilePickup.GetComponent<ProjectileController>().ghostPrefab = dagger1ProjectileGhost;
            dagger1ProjectilePickup.GetComponent<ProjectileStickOnImpact>().ignoreCharacters = true;
            dagger1ProjectilePickup.GetComponent<ProjectileStickOnImpact>().stickSoundString = "Play_DaggerLand";
            dagger1ProjectilePickup.GetComponent<ProjectileSimple>().desiredForwardSpeed = 20;
            dagger1ProjectilePickup.GetComponent<Rigidbody>().mass = 1;
            var impact = dagger1ProjectilePickup.AddComponent<ProjectileImpactExplosion>();
            impact.lifetime = 10;
            impact.destroyOnEnemy = false;
            impact.destroyOnWorld = false;
            impact.timerAfterImpact = true;
            impact.lifetimeAfterImpact = daggerMaxPickupTime;
            dagger1ProjectilePickup.AddComponent<BladePickupBehaviour>();
            ContentAddition.AddProjectile(dagger1ProjectilePickup);

            dagger2ProjectilePickup = PrefabAPI.InstantiateClone(dagger1ProjectilePickup, "KatarinaBlade2ProjectilePickup", true);
            dagger2ProjectilePickup.GetComponent<ProjectileController>().ghostPrefab = dagger2ProjectileGhost;
            dagger2ProjectilePickup.GetComponent<ProjectileStickOnImpact>().stickSoundString = "Play_DaggerLand";
            var sphere2 = dagger2ProjectilePickup.AddComponent<SphereCollider>();
            sphere2.isTrigger = true;
            sphere2.radius = 1.34f;
            var hurtboxGroup2 = dagger2ProjectilePickup.AddComponent<HurtBoxGroup>();
            var hurtBox2 = dagger2ProjectilePickup.AddComponent<HurtBox>();
            hurtBox2.healthComponent = null;
            hurtBox2.isBullseye = true;
            hurtBox2.damageModifier = HurtBox.DamageModifier.Normal;
            hurtBox2.hurtBoxGroup = hurtboxGroup2;
            hurtBox2.indexInGroup = 0;
            hurtboxGroup2.hurtBoxes = new HurtBox[] { hurtBox2 };
            hurtboxGroup2.mainHurtBox = hurtBox2;
            hurtboxGroup2.bullseyeCount = 1;
            ContentAddition.AddProjectile(dagger2ProjectilePickup);

            dagger3ProjectilePickup = PrefabAPI.InstantiateClone(dagger1ProjectilePickup, "KatarinaBlade3ProjectilePickup", true);
            dagger3ProjectilePickup.GetComponent<ProjectileController>().ghostPrefab = dagger3ProjectileGhost;
            dagger3ProjectilePickup.GetComponent<ProjectileStickOnImpact>().stickSoundString = "Play_DaggerLand";
            var sphere3 = dagger3ProjectilePickup.AddComponent<SphereCollider>();
            sphere3.isTrigger = true;
            sphere3.radius = 1.34f;
            var hurtboxGroup3 = dagger3ProjectilePickup.AddComponent<HurtBoxGroup>();
            var hurtBox3 = dagger3ProjectilePickup.AddComponent<HurtBox>();
            hurtBox3.healthComponent = null;
            hurtBox3.isBullseye = true;
            hurtBox3.damageModifier = HurtBox.DamageModifier.Normal;
            hurtBox3.hurtBoxGroup = hurtboxGroup3;
            hurtBox3.indexInGroup = 0;
            hurtboxGroup3.hurtBoxes = new HurtBox[] { hurtBox3 };
            hurtboxGroup3.mainHurtBox = hurtBox3;
            hurtboxGroup3.bullseyeCount = 1;
            ContentAddition.AddProjectile(dagger3ProjectilePickup);

            dagger4ProjectilePickup = PrefabAPI.InstantiateClone(dagger1ProjectilePickup, "KatarinaBlade4ProjectilePickup", true);
            dagger4ProjectilePickup.GetComponent<ProjectileController>().ghostPrefab = dagger4ProjectileGhost;
            dagger4ProjectilePickup.GetComponent<ProjectileStickOnImpact>().stickSoundString = "Play_DaggerLand";
            var sphere4 = dagger4ProjectilePickup.AddComponent<SphereCollider>();
            sphere4.isTrigger = true;
            sphere4.radius = 1.34f;
            var hurtboxGroup4 = dagger4ProjectilePickup.AddComponent<HurtBoxGroup>();
            var hurtBox4 = dagger4ProjectilePickup.AddComponent<HurtBox>();
            hurtBox4.healthComponent = null;
            hurtBox4.isBullseye = true;
            hurtBox4.damageModifier = HurtBox.DamageModifier.Normal;
            hurtBox4.hurtBoxGroup = hurtboxGroup4;
            hurtBox4.indexInGroup = 0;
            hurtboxGroup4.hurtBoxes = new HurtBox[] { hurtBox4 };
            hurtboxGroup4.mainHurtBox = hurtBox4;
            hurtboxGroup4.bullseyeCount = 1;
            ContentAddition.AddProjectile(dagger4ProjectilePickup);

            dagger5ProjectilePickup = PrefabAPI.InstantiateClone(dagger1ProjectilePickup, "KatarinaBlade5ProjectilePickup", true);
            dagger5ProjectilePickup.GetComponent<ProjectileController>().ghostPrefab = dagger5ProjectileGhost;
            dagger5ProjectilePickup.GetComponent<ProjectileStickOnImpact>().stickSoundString = "Play_DaggerLand";
            var sphere5 = dagger5ProjectilePickup.AddComponent<SphereCollider>();
            sphere5.isTrigger = true;
            sphere5.radius = 1.34f;
            var hurtboxGroup5 = dagger5ProjectilePickup.AddComponent<HurtBoxGroup>();
            var hurtBox5 = dagger5ProjectilePickup.AddComponent<HurtBox>();
            hurtBox5.healthComponent = null;
            hurtBox5.isBullseye = true;
            hurtBox5.damageModifier = HurtBox.DamageModifier.Normal;
            hurtBox5.hurtBoxGroup = hurtboxGroup5;
            hurtBox5.indexInGroup = 0;
            hurtboxGroup5.hurtBoxes = new HurtBox[] { hurtBox5 };
            hurtboxGroup5.mainHurtBox = hurtBox5;
            hurtboxGroup5.bullseyeCount = 1;
            ContentAddition.AddProjectile(dagger5ProjectilePickup);

            dagger6ProjectilePickup = PrefabAPI.InstantiateClone(dagger1ProjectilePickup, "KatarinaBlade6ProjectilePickup", true);
            dagger6ProjectilePickup.GetComponent<ProjectileController>().ghostPrefab = dagger6ProjectileGhost;
            dagger6ProjectilePickup.GetComponent<ProjectileStickOnImpact>().stickSoundString = "Play_DaggerLand";
            var sphere6 = dagger6ProjectilePickup.AddComponent<SphereCollider>();
            sphere6.isTrigger = true;
            sphere6.radius = 1.34f;
            var hurtboxGroup6 = dagger6ProjectilePickup.AddComponent<HurtBoxGroup>();
            var hurtBox6 = dagger6ProjectilePickup.AddComponent<HurtBox>();
            hurtBox6.healthComponent = null;
            hurtBox6.isBullseye = true;
            hurtBox6.damageModifier = HurtBox.DamageModifier.Normal;
            hurtBox6.hurtBoxGroup = hurtboxGroup6;
            hurtBox6.indexInGroup = 0;
            hurtboxGroup6.hurtBoxes = new HurtBox[] { hurtBox6 };
            hurtboxGroup6.mainHurtBox = hurtBox6;
            hurtboxGroup6.bullseyeCount = 1;
            ContentAddition.AddProjectile(dagger6ProjectilePickup);
            
            var mercfx = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion();
            pickupfx = PrefabAPI.InstantiateClone(mercfx, "katpickupfx", false);
            pickupfx.GetComponent<EffectComponent>().soundName = null;
            ContentAddition.AddEffect(pickupfx);
            
            var critflurry = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/FlurryArrowCritOrbEffect.prefab").WaitForCompletion();
            lotusfx = PrefabAPI.InstantiateClone(critflurry, "katlotusfx", false);
            UnityEngine.Object.Destroy(lotusfx.GetComponent<AkEvent>());
            ContentAddition.AddEffect(lotusfx);

            var lunarstuff = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarSkillReplacements/LunarDetonatorOrbEffect.prefab").WaitForCompletion();
            altlotusfx = PrefabAPI.InstantiateClone(lunarstuff, "kataltlotusfx", false);
            UnityEngine.Object.Destroy(altlotusfx.GetComponent<RTPCController>());
            ContentAddition.AddEffect(altlotusfx);

            shunpofx = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ExplodeOnDeathVoid/ExplodeOnDeathVoidExplosionEffect.prefab").WaitForCompletion();
            ContentAddition.AddEffect(shunpofx);
            
            var voidsent = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ExplodeOnDeathVoid/ExplodeOnDeathVoidExplosionEffect.prefab").WaitForCompletion();
            silentshunpofx = PrefabAPI.InstantiateClone(voidsent, "shunpofx", false);
            silentshunpofx.GetComponent<EffectComponent>().soundName = null;
            ContentAddition.AddEffect(silentshunpofx);
            
            var mercslash = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion();
            silentslashfx = PrefabAPI.InstantiateClone(mercslash, "silentslashfx", false);
            silentslashfx.GetComponent<EffectComponent>().soundName = null;
            ContentAddition.AddEffect(silentslashfx);
            
            var collapseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BleedOnHitVoid/FractureImpactEffect.prefab").WaitForCompletion();
            silentcollapseEffect = PrefabAPI.InstantiateClone(collapseEffect, "katcollapseEffect", false);
            silentcollapseEffect.GetComponent<EffectComponent>().soundName = null;
            ContentAddition.AddEffect(silentcollapseEffect);

            //setup for single dagger ghosts
            var dagger1throw = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger1fab"), "KatarinaThrown1", false);
            dagger1throw.AddComponent<ProjectileGhostController>();
            
            var dagger2throw = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger2fab"), "KatarinaThrown2", false);
            dagger2throw.AddComponent<ProjectileGhostController>();
            
            var dagger3throw = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger3fab"), "KatarinaThrown3", false);
            dagger3throw.AddComponent<ProjectileGhostController>();
            
            var dagger4throw = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger4fab"), "KatarinaThrown4", false);
            dagger4throw.AddComponent<ProjectileGhostController>();
            
            var dagger5throw = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger5fab"), "KatarinaThrown5", false);
            dagger5throw.AddComponent<ProjectileGhostController>();
            
            var dagger6throw = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger6fab"), "KatarinaThrown6", false);
            dagger6throw.AddComponent<ProjectileGhostController>();

            //register single daggers
            dagger1ProjectileThrow = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivProjectile.prefab").WaitForCompletion(), "KatarinaBlade1ProjectileThrow", true);
            //Component I created to quickly destroy the projectile after it collides with an entity. Adding for this version bc it should only spawn a model if it collides with terrain.
            dagger1ProjectileThrow.AddComponent<DestroyProjectileOnImpact>();
            dagger1ProjectileThrow.AddComponent<ModdedDamageTypeHolderComponent>().Add(blade1);
            dagger1ProjectileThrow.GetComponent<SphereCollider>().radius = MainPlugin.daggerspherebox.Value;
            dagger1ProjectileThrow.GetComponent<ProjectileSimple>().desiredForwardSpeed = MainPlugin.throwvelocity.Value;
            dagger1ProjectileThrow.GetComponent<ProjectileSingleTargetImpact>().hitSoundString = "Play_DaggerLand";
            dagger1ProjectileThrow.GetComponent<ProjectileSingleTargetImpact>().enemyHitSoundString = "Play_DaggerHitEnemy";
            var singlecontroller = dagger1ProjectileThrow.GetComponent<ProjectileController>();
            singlecontroller.ghostPrefab = dagger1throw;
            singlecontroller.procCoefficient = 0.8f;
            ContentAddition.AddProjectile(dagger1ProjectileThrow);

            dagger2ProjectileThrow = PrefabAPI.InstantiateClone(dagger1ProjectileThrow, "KatarinaBlade2ProjectileThrow", true);
            Utils.SwapModdedDamageType(dagger2ProjectileThrow, blade2);
            dagger2ProjectileThrow.GetComponent<ProjectileController>().ghostPrefab = dagger2throw;
            ContentAddition.AddProjectile(dagger2ProjectileThrow);

            dagger3ProjectileThrow = PrefabAPI.InstantiateClone(dagger1ProjectileThrow, "KatarinaBlade3ProjectileThrow", true);
            Utils.SwapModdedDamageType(dagger3ProjectileThrow, blade3);
            dagger3ProjectileThrow.GetComponent<ProjectileController>().ghostPrefab = dagger3throw;
            ContentAddition.AddProjectile(dagger3ProjectileThrow);

            dagger4ProjectileThrow = PrefabAPI.InstantiateClone(dagger1ProjectileThrow, "KatarinaBlade4ProjectileThrow", true);
            Utils.SwapModdedDamageType(dagger4ProjectileThrow, blade4);
            dagger4ProjectileThrow.GetComponent<ProjectileController>().ghostPrefab = dagger4throw;
            ContentAddition.AddProjectile(dagger4ProjectileThrow);

            dagger5ProjectileThrow = PrefabAPI.InstantiateClone(dagger1ProjectileThrow, "KatarinaBlade5ProjectileThrow", true);
            Utils.SwapModdedDamageType(dagger5ProjectileThrow, blade5);
            dagger5ProjectileThrow.GetComponent<ProjectileController>().ghostPrefab = dagger5throw;
            ContentAddition.AddProjectile(dagger5ProjectileThrow);

            dagger6ProjectileThrow = PrefabAPI.InstantiateClone(dagger1ProjectileThrow, "KatarinaBlade6ProjectileThrow", true);
            Utils.SwapModdedDamageType(dagger6ProjectileThrow, blade6);
            dagger6ProjectileThrow.GetComponent<ProjectileController>().ghostPrefab = dagger6throw;
            ContentAddition.AddProjectile(dagger6ProjectileThrow);

            //setup for Genji dagger ghosts
            var dagger1bleed = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger1fab"), "KatarinaThrown1Bleed", false);
            dagger1bleed.AddComponent<ProjectileGhostController>();
            
            var dagger2bleed = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger2fab"), "KatarinaThrown2Bleed", false);
            dagger2bleed.AddComponent<ProjectileGhostController>();
            
            var dagger3bleed = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger3fab"), "KatarinaThrown3Bleed", false);
            dagger3bleed.AddComponent<ProjectileGhostController>();
            
            var dagger4bleed = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger4fab"), "KatarinaThrown4Bleed", false);
            dagger4bleed.AddComponent<ProjectileGhostController>();
            
            var dagger5bleed = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger5fab"), "KatarinaThrown5Bleed", false);
            dagger5bleed.AddComponent<ProjectileGhostController>();
            
            var dagger6bleed = PrefabAPI.InstantiateClone(KatAssets.MainAssetBundle.LoadAsset<GameObject>("dagger6fab"), "KatarinaThrown6Bleed", false);
            dagger6bleed.AddComponent<ProjectileGhostController>();
            
            //register Genji daggers
            dagger1ProjectileThrowBleed = PrefabAPI.InstantiateClone(baseProjectile, "KatarinaBlade1ProjectileThrowBleed", true);
            dagger1ProjectileThrowBleed.AddComponent<ModdedDamageTypeHolderComponent>().Add(bladeBleed);
            dagger1ProjectileThrowBleed.GetComponent<ProjectileSingleTargetImpact>().hitSoundString = "Play_DaggerLand";
            dagger1ProjectileThrowBleed.GetComponent<ProjectileSingleTargetImpact>().enemyHitSoundString = "Play_DaggerHitEnemy";
            dagger1ProjectileThrowBleed.GetComponent<SphereCollider>().radius = 0.6f;
            var genjicontroller = dagger1ProjectileThrowBleed.GetComponent<ProjectileController>();
            genjicontroller.ghostPrefab = dagger1bleed;
            genjicontroller.procCoefficient = 0.3f;
            ContentAddition.AddProjectile(dagger1ProjectileThrowBleed);

            dagger2ProjectileThrowBleed = PrefabAPI.InstantiateClone(dagger1ProjectileThrowBleed, "KatarinaBlade2ProjectileThrowBleed", true);
            dagger2ProjectileThrowBleed.GetComponent<ProjectileController>().ghostPrefab = dagger2bleed;
            ContentAddition.AddProjectile(dagger2ProjectileThrowBleed);

            dagger3ProjectileThrowBleed = PrefabAPI.InstantiateClone(dagger1ProjectileThrowBleed, "KatarinaBlade3ProjectileThrowBleed", true);
            dagger3ProjectileThrowBleed.GetComponent<ProjectileController>().ghostPrefab = dagger3bleed;
            ContentAddition.AddProjectile(dagger3ProjectileThrowBleed);

            dagger4ProjectileThrowBleed = PrefabAPI.InstantiateClone(dagger1ProjectileThrowBleed, "KatarinaBlade4ProjectileThrowBleed", true);
            dagger4ProjectileThrowBleed.GetComponent<ProjectileController>().ghostPrefab = dagger4bleed;
            ContentAddition.AddProjectile(dagger4ProjectileThrowBleed);

            dagger5ProjectileThrowBleed = PrefabAPI.InstantiateClone(dagger1ProjectileThrowBleed, "KatarinaBlade5ProjectileThrowBleed", true);
            dagger5ProjectileThrowBleed.GetComponent<ProjectileController>().ghostPrefab = dagger5bleed;
            ContentAddition.AddProjectile(dagger5ProjectileThrowBleed);

            dagger6ProjectileThrowBleed = PrefabAPI.InstantiateClone(dagger1ProjectileThrowBleed, "KatarinaBlade6ProjectileThrowBleed", true);
            dagger6ProjectileThrowBleed.GetComponent<ProjectileController>().ghostPrefab = dagger6bleed;
            ContentAddition.AddProjectile(dagger6ProjectileThrowBleed);
        }
    }
}

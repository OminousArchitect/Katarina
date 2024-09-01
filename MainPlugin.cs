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
using ExtraSkillSlots;
using UnityEngine.AddressableAssets;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace Katarina
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(MODUID, MODNAME, VERSION)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    //[R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SoundAPI), nameof(LanguageAPI), nameof(LoadoutAPI), nameof(DamageAPI), nameof(RecalculateStatsAPI), nameof(OrbAPI))]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Nines.Katarina";
        public const string MODNAME = "Katarina";
        public const string VERSION = "2.2.0";
        public const string SURVIVORNAME = "NinesKatarina";
        public const string SURVIVORNAMEKEY = "NINESKATARINA";
        public static GameObject characterPrefab;
        // Color used in characterbody
        private static readonly Color characterColor = new Color(0.925490196f, 0.435294118f, 0.91372549f);
        
        internal static ConfigEntry<bool> shunpofx;
        internal static ConfigEntry<bool> killexplosionfx;
        internal static ConfigEntry<bool> enablelotusvoice;
        internal static ConfigEntry<bool> enablemeleevoice;
        internal static ConfigEntry<bool> enabledaggervoice;
        internal static ConfigEntry<bool> loudshunpo;
        internal static ConfigEntry<bool> silentslash;
        internal static ConfigEntry<bool> randomdaggers;
        internal static ConfigEntry<bool> altlotusfx;
        internal static ConfigEntry<bool> shunpodamagefx;
        internal static ConfigEntry<bool> collapseEveryone;

        internal static ConfigEntry<float> throwvelocity;
        internal static ConfigEntry<float> daggerpickupzone;
        internal static ConfigEntry<float> daggerpickupslash;
        internal static ConfigEntry<float> radialhealpercent;
        internal static ConfigEntry<float> blinkdmg;
        internal static ConfigEntry<float> ihateflyingenemies;
        internal static ConfigEntry<float> daggerspherebox;
        internal static ConfigEntry<int> shunpoSetting;

        internal static ConfigEntry<float> mainRootSpeed;
        internal static ConfigEntry<float> baseMaxHealth;
        internal static ConfigEntry<float> levelMaxHealth;
        internal static ConfigEntry<float> baseRegen;
        internal static ConfigEntry<float> levelRegen;
        internal static ConfigEntry<float> baseMoveSpeed;
        internal static ConfigEntry<float> levelMoveSpeed;
        internal static ConfigEntry<float> baseAcceleration;
        internal static ConfigEntry<float> baseJumpPower;
        internal static ConfigEntry<float> levelJumpPower;
        internal static ConfigEntry<float> levelDamage;
        internal static ConfigEntry<float> baseAttackSpeed;
        internal static ConfigEntry<float> levelAttackSpeed;
        internal static ConfigEntry<float> baseArmor;
        internal static ConfigEntry<float> levelArmor;
        internal static ConfigEntry<float> sprintingSpeedMultiplier;

        internal static Shader hopooshaders = Resources.Load<Shader>("Shaders/Deferred/HGStandard");

        public static BodyIndex vultureIndex;
        public static BodyIndex pestIndex;
        public static BodyIndex katIndex;
        
        
        private void Awake()
        {
            shunpofx = base.Config.Bind<bool>(new ConfigDefinition("VFX", "Shunpo VFX"), true, new ConfigDescription("", null, Array.Empty<object>()));
            killexplosionfx = base.Config.Bind<bool>(new ConfigDefinition("VFX", "Special Effects On Reset"), true, new ConfigDescription("", null, Array.Empty<object>()));
            collapseEveryone = base.Config.Bind<bool>(new ConfigDefinition("VFX", "Special Effect Choice"), false, new ConfigDescription("false for Lights Out, true for Collapse", null, Array.Empty<object>()));
            shunpoSetting = base.Config.Bind<int>(new ConfigDefinition("VFX", "Shunpo Kill Effect"), 2, new ConfigDescription("0: Lights Out 1: Collapse 2: Lights Out for ground enemies and Collapse for flying enemies", null, Array.Empty<object>()));
            altlotusfx = base.Config.Bind<bool>(new ConfigDefinition("VFX", "Alternate Death Lotus Projectiles"), false, new ConfigDescription("False for red daggers, True for Scheman magic bursts", null, Array.Empty<object>()));
            enablelotusvoice = Config.Bind<bool>(new ConfigDefinition("Voice", "Death Lotus Voice"), true, new ConfigDescription("", null, Array.Empty<object>()));
            enablemeleevoice = Config.Bind<bool>(new ConfigDefinition("Voice", "Sinister Steel Voice"), true, new ConfigDescription("", null, Array.Empty<object>()));
            enabledaggervoice = Config.Bind<bool>(new ConfigDefinition("Voice", "Voracious Blade Voice"), true, new ConfigDescription("", null, Array.Empty<object>()));
            randomdaggers = base.Config.Bind<bool>(new ConfigDefinition("Daggers", "True Random Daggers"), false, new ConfigDescription("true to randomize her weapons and dagger pickups, false to sync them together", null, Array.Empty<object>()));
            loudshunpo = base.Config.Bind<bool>(new ConfigDefinition("Shunpo", "Shunpo Explosion Sound"), true, new ConfigDescription("", null, Array.Empty<object>()));
            silentslash = base.Config.Bind<bool>(new ConfigDefinition("Sinister Steel", "Quieter Impact"), true, new ConfigDescription("Mercenary slash impact sfx, turned off by default", null, Array.Empty<object>()));
            shunpodamagefx = base.Config.Bind<bool>(new ConfigDefinition("Shunpo", "Enemy Damage FX"), true, new ConfigDescription("Collapse effect on enemy", null, Array.Empty<object>()));
            
            blinkdmg = base.Config.Bind<float>(new ConfigDefinition("Shunpo", "Damage Coefficient"), 2.6f, new ConfigDescription("", null, Array.Empty<object>()));
            ihateflyingenemies = base.Config.Bind<float>(new ConfigDefinition("Shunpo", "Flying Enemy Damage Coefficient"), 4.6f, new ConfigDescription("", null, Array.Empty<object>()));
            daggerpickupzone = base.Config.Bind<float>(new ConfigDefinition("Daggers", "Pickup Zone Radius"), 5f, new ConfigDescription("", null,Array.Empty<object>()));
            daggerpickupslash = base.Config.Bind<float>(new ConfigDefinition("Daggers", "Pickup Slash Radius"), 15f, new ConfigDescription("", null,Array.Empty<object>()));
            radialhealpercent = base.Config.Bind<float>(new ConfigDefinition("Daggers", "Omnivamp"), 0.3f, new ConfigDescription("Percent of damage dealt on a per enemy basis. 0.3 is 3% of damage done to one enemy, times the number of enemies hit.", null, Array.Empty<object>()));
            daggerspherebox = base.Config.Bind<float>(new ConfigDefinition("Daggers", "Hitbox"), 1.1f, new ConfigDescription("Feel free to mess with this, it's measured as a radius. Serrated Shiv is 0.3", null, Array.Empty<object>()));
            throwvelocity = base.Config.Bind<float>(new ConfigDefinition("Daggers", "Velocity"), 200, new ConfigDescription("Default is 185, changed to 200 for v2.2, Serrated Shiv is 160", null, Array.Empty<object>()));
            
            /*
            mainRootSpeed = base.Config.Bind<float>(new ConfigDefinition("Stats", "MainRootSpeed"), 0, new ConfigDescription("", null, Array.Empty<object>()));
            baseMaxHealth = base.Config.Bind<float>(new ConfigDefinition("Stats", "BaseMaxHealth"), 120, new ConfigDescription("", null, Array.Empty<object>()));
            levelMaxHealth = base.Config.Bind<float>(new ConfigDefinition("Stats", "LevelMaxHealth"), 35, new ConfigDescription("", null, Array.Empty<object>()));
            baseRegen = base.Config.Bind<float>(new ConfigDefinition("Stats", "BaseRegen"), 1, new ConfigDescription("", null, Array.Empty<object>()));
            levelRegen = base.Config.Bind<float>(new ConfigDefinition("Stats", "LevelRegen"), 0.33f, new ConfigDescription("", null, Array.Empty<object>()));
            baseMoveSpeed = base.Config.Bind<float>(new ConfigDefinition("Stats", "BaseMoveSpeed"), 7, new ConfigDescription("", null, Array.Empty<object>()));
            levelMoveSpeed = base.Config.Bind<float>(new ConfigDefinition("Stats", "LevelMoveSpeed"), 0, new ConfigDescription("", null, Array.Empty<object>()));
            baseAcceleration = base.Config.Bind<float>(new ConfigDefinition("Stats", "BaseAcceleration"), 110, new ConfigDescription("", null, Array.Empty<object>()));
            baseJumpPower = base.Config.Bind<float>(new ConfigDefinition("Stats", "BaseJumpPower"), 20, new ConfigDescription("", null, Array.Empty<object>()));
            levelJumpPower = base.Config.Bind<float>(new ConfigDefinition("Stats", "LevelJumpPower"), 0, new ConfigDescription("", null, Array.Empty<object>()));
            levelDamage = base.Config.Bind<float>(new ConfigDefinition("Stats", "LevelDamage"), 2.8f, new ConfigDescription("", null, Array.Empty<object>()));
            baseAttackSpeed = base.Config.Bind<float>(new ConfigDefinition("Stats", "BaseAttackSpeed"), 1.3f, new ConfigDescription("", null, Array.Empty<object>()));
            levelAttackSpeed = base.Config.Bind<float>(new ConfigDefinition("Stats", "LevelAttackSpeed"), 0.01f, new ConfigDescription("", null, Array.Empty<object>()));
            baseArmor = base.Config.Bind<float>(new ConfigDefinition("Stats", "BaseArmor"), 20, new ConfigDescription("", null, Array.Empty<object>()));
            levelArmor = base.Config.Bind<float>(new ConfigDefinition("Stats", "LevelArmor"), 0.25f, new ConfigDescription("", null, Array.Empty<object>()));
            sprintingSpeedMultiplier = base.Config.Bind<float>(new ConfigDefinition("Stats", "SprintingSpeedMultiplier"), 1.45f, new ConfigDescription("", null, Array.Empty<object>()));
            */
            
            
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (self, user, t) => { };
            
            Assets.PopulateAssets();
            Prefabs.CreatePrefabs();
            CreateSurvPrefab();
            RegisterStates();
            RegisterCharacter();
            Hook.Hooks();
            RoR2Application.onLoad += () => 
            {   //cache BodyIndex on load
                vultureIndex = BodyCatalog.FindBodyIndex("VultureBody");
                pestIndex = BodyCatalog.FindBodyIndex("FlyingVerminBody");
                katIndex = BodyCatalog.FindBodyIndex("NinesKatarinaBody");
            };
        }
        
        
        internal static void CreateSurvPrefab()
        {
            characterPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), SURVIVORNAME + "Body", true);
            characterPrefab.AddComponent<BladeController>();
            characterPrefab.AddComponent<KatarinaTracker>().maxTrackingDistance = 93f;
            characterPrefab.GetComponent<KatarinaTracker>().maxTrackingAngle = 21f;
            //characterPrefab.AddComponent<KatarinaSkillSwitchBehaviour>();
            characterPrefab.AddComponent<ExtraSkillLocator>();
            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            Destroy(characterPrefab.transform.Find("ModelBase").gameObject);
            Destroy(characterPrefab.transform.Find("CameraPivot").gameObject);
            Destroy(characterPrefab.transform.Find("AimOrigin").gameObject);
            //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(replacementDef, SURVIVORNAME + "Body", slotIndex, variantIndex);

            GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>("katarina");

            GameObject ModelBase = new GameObject("ModelBase");
            ModelBase.transform.parent = characterPrefab.transform;
            ModelBase.transform.localPosition = new Vector3(0f, -0.94f, 0f);
            ModelBase.transform.localRotation = Quaternion.identity;
            ModelBase.transform.localScale = new Vector3(1.42f, 1.42f, 1.42f);

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = ModelBase.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.7f, 0f); //TODO AimOrigin
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = ModelBase.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = characterPrefab.GetComponent<CharacterDirection>();
            characterDirection.moveVector = Vector3.zero;
            characterDirection.targetTransform = ModelBase.transform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            CharacterBody bodyComponent = characterPrefab.GetComponent<CharacterBody>(); //todo CharacterBody stats
            bodyComponent.name = SURVIVORNAME + "Body";
            bodyComponent.baseNameToken = SURVIVORNAMEKEY + "_NAME";
            bodyComponent.subtitleNameToken = SURVIVORNAMEKEY + "_SUBTITLE";
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.SprintAnyDirection;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0f;
            bodyComponent.baseMaxHealth = 120f;
            bodyComponent.levelMaxHealth = 35f;
            bodyComponent.baseRegen = 1;
            bodyComponent.levelRegen = 0.33f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0;
            bodyComponent.baseMoveSpeed = 7f;
            bodyComponent.levelMoveSpeed = 0;
            bodyComponent.baseAcceleration = 110f;
            bodyComponent.baseJumpPower = 20f;
            bodyComponent.levelJumpPower = 0f;
            bodyComponent.baseDamage = 13;
            bodyComponent.levelDamage = 2.8f;
            bodyComponent.baseAttackSpeed = 1.3f;
            bodyComponent.levelAttackSpeed = 0.02f;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0;
            bodyComponent.baseArmor = 20f;
            bodyComponent.levelArmor = 0f;
            bodyComponent.baseJumpCount = 1;
            bodyComponent.sprintingSpeedMultiplier = 1.45f;
            bodyComponent.wasLucky = false;
            bodyComponent._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2CrosshairPrepRevolver.prefab").WaitForCompletion();
            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.MainAssetBundle.LoadAsset<Texture>("KatIcon");
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.skinIndex = 0U;
            bodyComponent.preferredPodPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/robocratepod");
            bodyComponent.bodyColor = characterColor;

            CharacterMotor characterMotor = characterPrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 110f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;

            InputBankTest inputBankTest = characterPrefab.GetComponent<InputBankTest>();
            inputBankTest.moveVector = Vector3.zero;

            var cameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();
            cameraParams.data.minPitch = -70;
            cameraParams.data.maxPitch = 70;
            cameraParams.data.wallCushion = 0.1f;
            cameraParams.data.pivotVerticalOffset = 0.6f;
            cameraParams.data.idealLocalCameraPos = new Vector3(0, 0, -10);
            #region IdkMan
            GameObject cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.parent = characterPrefab.transform;
            cameraPivot.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            cameraPivot.transform.localRotation = Quaternion.identity;
            #endregion
            CameraTargetParams cameraTargetParams = characterPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = cameraParams;
            cameraTargetParams.cameraPivotTransform = characterPrefab.transform.Find("CameraPivot");
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            ModelLocator modelLocator = characterPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = ModelBase.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false;
            modelLocator.preserveModel = false;

            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            CharacterModel characterModel = model.AddComponent<CharacterModel>();

            SkinnedMeshRenderer[] renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            List<CharacterModel.RendererInfo> rendererInfoList = new List<CharacterModel.RendererInfo>();
            for (int i = 0; i < renderers.Length; i++)
            {
                CharacterModel.RendererInfo newRenderer = new CharacterModel.RendererInfo()
                {
                    renderer = renderers[i],
                    defaultMaterial = Utils.InstantiateMaterial(renderers[i].material.color, renderers[i].material.GetTexture("_MainTex"), Color.white, 0, null, 1, null),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                };
                rendererInfoList.Add(newRenderer);
            }
            
            characterModel.baseRendererInfos = rendererInfoList.ToArray();
            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();
            Reflection.SetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer", renderers[0]);//renderer1);
            SkinnedMeshRenderer fieldValue = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");

            ModelSkinController modelSkinController = model.AddComponent<ModelSkinController>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "BODY_DEFAULT_SKIN_NAME", "Default");

            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo.GameObjectActivations = new SkinDef.GameObjectActivation[0];
            skinDefInfo.Icon = LoadoutAPI.CreateSkinIcon(
                new Color(0.909803922f, 0.450980392f, 0.890196078f),
                new Color(0.937254902f, 0.862745098f, 0.658823529f), 
                new Color(0.44705882352f, 0.380392157f, 0.525490196f),
                new Color(0.980392157f, 0.31372549f, 0.576470588f)
                );
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[0];
            skinDefInfo.Name = "Default";
            skinDefInfo.NameToken = "Battle Queen";
            skinDefInfo.RendererInfos = characterModel.baseRendererInfos;
            skinDefInfo.RootObject = model;
            skinDefInfo.UnlockableDef = null;
            SkinDef skinDef = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            modelSkinController.skins = new SkinDef[]
            {
                skinDef
            };

            HealthComponent healthComponent = characterPrefab.GetComponent<HealthComponent>();
            healthComponent.health = 110f;
            healthComponent.shield = 0f;
            healthComponent.barrier = 0f;
            healthComponent.magnetiCharge = 0f;
            healthComponent.body = null;
            healthComponent.dontShowHealthbar = false;
            healthComponent.globalDeathEventChanceCoefficient = 1f;

            characterPrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
            characterPrefab.GetComponent<InteractionDriver>().highlightInteractor = true;

            SfxLocator sfxLocator = characterPrefab.GetComponent<SfxLocator>();
            sfxLocator.deathSound = "Play_DeathQuote";
            sfxLocator.barkSound = "";
            sfxLocator.openSound = "";
            sfxLocator.landingSound = "Play_char_land";
            sfxLocator.fallDamageSound = "Play_char_land_fall_damage";
            sfxLocator.aliveLoopStart = "";
            sfxLocator.aliveLoopStop = "";

            Rigidbody rigidbody = characterPrefab.GetComponent<Rigidbody>();
            rigidbody.mass = 100f;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;

            CapsuleCollider capsuleCollider = childLocator.FindChild("collider").GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = false;
            capsuleCollider.material = null;

            KinematicCharacterMotor kinematicCharacterMotor = characterPrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            kinematicCharacterMotor.Capsule = capsuleCollider;
            kinematicCharacterMotor.Rigidbody = rigidbody;

            kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 0.2f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;
            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;
            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = Vector3.up;
            kinematicCharacterMotor.StepHandling = StepHandlingMethod.None;
            kinematicCharacterMotor.LedgeHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.SafeMovement = false;

            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();

            HurtBox componentInChildren = capsuleCollider.gameObject.AddComponent<HurtBox>();
            componentInChildren.gameObject.layer = LayerIndex.entityPrecise.intVal;
            componentInChildren.healthComponent = healthComponent;
            componentInChildren.isBullseye = true;
            componentInChildren.damageModifier = HurtBox.DamageModifier.Normal;
            componentInChildren.hurtBoxGroup = hurtBoxGroup;
            componentInChildren.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                componentInChildren
            };

            hurtBoxGroup.mainHurtBox = componentInChildren;
            hurtBoxGroup.bullseyeCount = 1;

            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.inputBank = inputBankTest;
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMax = 55f;
            aimAnimator.pitchRangeMin = -50f;
            aimAnimator.yawRangeMin = -44f;
            aimAnimator.yawRangeMax = 44f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 8f;

            /*FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");*/

            EntityStateMachine mainStateMachine = bodyComponent.GetComponent<EntityStateMachine>();
            mainStateMachine.mainStateType = new SerializableEntityStateType(typeof(CharacterMain));

            CharacterDeathBehavior characterDeathBehavior = characterPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            var newStateMachine = characterPrefab.AddComponent<EntityStateMachine>();
            newStateMachine.customName = "Blade";
            newStateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));
            newStateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));

            var newStateMachine2 = characterPrefab.AddComponent<EntityStateMachine>();
            newStateMachine2.customName = "Blink";
            newStateMachine2.initialStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));
            newStateMachine2.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));

            var newStateMachine3 = characterPrefab.AddComponent<EntityStateMachine>();
            newStateMachine3.customName = "Speen";
            newStateMachine3.initialStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));
            newStateMachine3.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));
            
            
            var newStateMachine4 = characterPrefab.AddComponent<EntityStateMachine>();
            newStateMachine4.customName = "Swipe";
            newStateMachine4.initialStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));
            newStateMachine4.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));

            NetworkStateMachine networkStateMachine = bodyComponent.GetComponent<NetworkStateMachine>();
            var list = networkStateMachine.stateMachines.ToList();
            list.Add(newStateMachine);
            list.Add(newStateMachine2);
            list.Add(newStateMachine3);
            list.Add(newStateMachine4);
            networkStateMachine.stateMachines = list.ToArray();

            var punchHitbox = Utils.CreateHitbox("Primary", model.transform, new Vector3(3.5f, 4.5f, 3.8f));
            punchHitbox.transform.localPosition = new Vector3(0, 1f, 1);
            Utils.CreateHitbox("Utility", model.transform, new Vector3(8, 8, 8));
            ContentAddition.AddBody(characterPrefab);
        }
        private void RegisterCharacter()
        {
            var characterDisplay = PrefabAPI.InstantiateClone(characterPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, SURVIVORNAME + "Display", true);
            characterDisplay.AddComponent<NetworkIdentity>();

            string desc = "Katarina is a blazing-speed melee assassin who can infinitely reset her abilities so long as there is something to kill." +
                          "<style=cSub>\r\n\r\n< ! > Using your abilities in the correct order will allow you to reset them more than once against even a single enemy. Experiment with different combos to maximize your damage output and zip around the battlefield."
                          +
                          "<style=cSub>\r\n\r\n< ! > The fan of daggers from Voracious Blade will not spawn pickups, but are good for dealing with tanky enemies at range."
                          +
                          "<style=cSub>\r\n\r\n< ! > Shunpo does additional bonus damage based on the target's current % HP and heals you for a portion of that amount. Naturally you'll yield the highest damage/healing output by casting it on full-health targets."
                          +
                          "<style=cSub>\r\n\r\n< ! > Death lotus targets more enemies the greater attack speed you have and its cooldown is decreased by 4 seconds with each kill.";

            string outro = "..and so she left, her instincts, and her blades honed evermore.";
            string fail = "..and so she stayed, her hubris proving lethal.";

            LanguageAPI.Add(SURVIVORNAMEKEY + "_NAME", "Katarina");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_DESCRIPTION", desc);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SUBTITLE", "The Sinister Blade");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_OUTRO", outro);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_FAIL", fail);

            var survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            {
                survivorDef.cachedName = SURVIVORNAMEKEY + "_NAME";
                survivorDef.unlockableDef = null;
                survivorDef.descriptionToken = SURVIVORNAMEKEY + "_DESCRIPTION";
                survivorDef.primaryColor = characterColor;
                survivorDef.bodyPrefab = characterPrefab;
                survivorDef.displayPrefab = characterDisplay;
                survivorDef.outroFlavorToken = SURVIVORNAMEKEY + "_OUTRO";
                survivorDef.desiredSortPosition = 30f;
                survivorDef.mainEndingEscapeFailureFlavorToken = SURVIVORNAMEKEY + "_FAIL";
            };

            ContentAddition.AddSurvivorDef(survivorDef);

            SkillSetup();

            var characterMaster = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/CommandoMonsterMaster"), SURVIVORNAME + "Master", true);

            ContentAddition.AddMaster(characterMaster);

            CharacterMaster component = characterMaster.GetComponent<CharacterMaster>();
            component.bodyPrefab = characterPrefab;
        }
        void RegisterStates()
        {
            bool hmm;
            ContentAddition.AddEntityState<Primary>(out hmm);
            ContentAddition.AddEntityState<Secondary>(out hmm);
            ContentAddition.AddEntityState<ThrowSingleDagger>(out hmm);
            ContentAddition.AddEntityState<ThrowGenjiDaggers>(out hmm);
            ContentAddition.AddEntityState<Utility>(out hmm);
            ContentAddition.AddEntityState<AltUtility>(out hmm);
            ContentAddition.AddEntityState<Blink>(out hmm);
            ContentAddition.AddEntityState<BlinkTarget>(out hmm);
            ContentAddition.AddEntityState<Special>(out hmm);
            ContentAddition.AddEntityState<CharacterMain>(out hmm);
            ContentAddition.AddEntityState<BaseDaggerPickupState>(out hmm);
            ContentAddition.AddEntityState<DaggerPickupAir>(out hmm);
        }
        void SkillSetup()
        {
            foreach (GenericSkill obj in characterPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }
            PassiveSetup();
            PrimarySetup();
            SecondarySetup();
            ExtraSecondary();
            //AltSecondarySetup();
            AltUtilitySetup();
            ExtraUtility();
            //UtilitySetup();
            SpecialSetup();
        }
        void PassiveSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_NAME", "Voracity");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION", "Always looking for her next target, Katarina <color=#E92630>resets</color> her ability cooldowns <style=cIsDamage>on-kill</style>. Hitting an enemy with a Voracious Blade will spawn a <color=#E92630>dagger</color> that can be picked up for <style=cIsDamage>500% damage</style>.");
            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = SURVIVORNAMEKEY + "_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("Passive1");
        }
        void PrimarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1", "Sinister Steel");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1_DESCRIPTION", "<style=cIsDamage>Slayer.</style> Swipe your blades in front of you for <style=cIsDamage>210% damage.</style>");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Primary));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("primary");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_M1_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_M1";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_M1";
            SkillDef.keywordTokens = new string[1]
            {"KEYWORD_SLAYER" };

            ContentAddition.AddSkillDef(SkillDef);

            component.primary = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.primary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.primary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);

        }
        void SecondarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            
            /*LanguageAPI.Add(SURVIVORNAMEKEY + "ALT_M2", "You Can Really Just Delete this");
            LanguageAPI.Add(SURVIVORNAMEKEY + "ALT_M2_DESCRIPTION", "Input has been moved anyways");
            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(ThrowSingleDagger));
            SkillDef.activationStateMachineName = "Blade";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 6f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.dontAllowPastMaxStocks = true;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("secondary");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "ALT_M2_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "ALT_M2";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "ALT_M2";
            ContentAddition.AddSkillDef(SkillDef);*/
            
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2", "Voracious Blade");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2_DESCRIPTION", "Throw a dagger for <style=cIsDamage>360% damage</style>, or hold to throw a fan of daggers for <style=cIsDamage>5x135% damage</style> that <style=cIsHealth>hemorrhage.</style>");
            var Alt1SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            Alt1SkillDef.activationState = new SerializableEntityStateType(typeof(Secondary));
            Alt1SkillDef.activationStateMachineName = "Blade";
            Alt1SkillDef.baseMaxStock = 1;
            Alt1SkillDef.baseRechargeInterval = 5.5f;
            Alt1SkillDef.beginSkillCooldownOnSkillEnd = true;
            Alt1SkillDef.canceledFromSprinting = false;
            Alt1SkillDef.fullRestockOnAssign = false;
            Alt1SkillDef.interruptPriority = InterruptPriority.Any;
            Alt1SkillDef.isCombatSkill = true;
            Alt1SkillDef.mustKeyPress = true;
            Alt1SkillDef.cancelSprintingOnActivation = false;
            Alt1SkillDef.rechargeStock = 1;
            Alt1SkillDef.dontAllowPastMaxStocks = true;
            Alt1SkillDef.requiredStock = 1;
            Alt1SkillDef.stockToConsume = 1;
            Alt1SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("secondary");
            Alt1SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_M2_DESCRIPTION";
            Alt1SkillDef.skillName = SURVIVORNAMEKEY + "_M2";
            Alt1SkillDef.skillNameToken = SURVIVORNAMEKEY + "_M2";
            ContentAddition.AddSkillDef(Alt1SkillDef);
            
            component.secondary = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.secondary.skillFamily;

            /*skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
            };*/
            
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = Alt1SkillDef,
                viewableNode = new ViewablesCatalog.Node(Alt1SkillDef.skillNameToken, false, null)
            };
            
            ContentAddition.AddSkillFamily(skillFamily);
        }
        
        void ExtraSecondary()
        {
            ExtraSkillLocator extracomponent = characterPrefab.GetComponent<ExtraSkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_ALT2_SECONDARY", "Preparation");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_ALT2_SECONDARY_DESCRIPTION",
                "Swipe your blades around you for <style=cIsDamage>500% damage</style>. " +
                "Decreases the cooldown of both <color=#E92630>Shunpo</color> techniques by <style=cIsUtility>7 seconds</style>.");
            
            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(BaseDaggerPickupState));
            SkillDef.activationStateMachineName = "Swipe";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 6f;
            SkillDef.beginSkillCooldownOnSkillEnd = false;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("Preparation");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_ALT2_SECONDARY_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_ALT2_SECONDARY";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_ALT2_SECONDARY";

            ContentAddition.AddSkillDef(SkillDef);

            extracomponent.extraSecond = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily katsixthfamily = ScriptableObject.CreateInstance<SkillFamily>();
            katsixthfamily.variants = new SkillFamily.Variant[1];
            extracomponent.extraSecond.SetFieldValue("_skillFamily", katsixthfamily);
            SkillFamily skillFamily = extracomponent.extraSecond.skillFamily;
            
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
        }
        
        void UtilitySetup()
        {
            KatarinaSkillSwitchBehaviour switchComponent = characterPrefab.GetComponent<KatarinaSkillSwitchBehaviour>();
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Utility));
            SkillDef.activationStateMachineName = "Blink";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 10f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = false;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("utility");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_UTIL_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_UTIL";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_UTIL";

            //switchComponent.skillDefList.Add(SkillDef);

            ContentAddition.AddSkillDef(SkillDef);

            component.utility = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);                     
        }
        void AltUtilitySetup()
        {   //KatarinaSkillSwitchBehaviour switchComponent = characterPrefab.GetComponent<KatarinaSkillSwitchBehaviour>();
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "ALT_UTIL", "Scheman Shunpo");
            LanguageAPI.Add(SURVIVORNAMEKEY + "ALT_UTIL_DESCRIPTION", "Dash in the <color=#E92630>blink of an eye</color> to target enemy for <style=cIsDamage>260% damage</style> on-arrival.");

            var SkillDef = ScriptableObject.CreateInstance<KatarinaSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(AltUtility));
            SkillDef.activationStateMachineName = "Blink";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 6.5f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = false;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.forceSprintDuringState = true;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("utility2");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "ALT_UTIL_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "ALT_UTIL";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "ALT_UTIL";

            //switchComponent.skillDefList.Add(SkillDef);

            ContentAddition.AddSkillDef(SkillDef);

            component.utility = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
        }

        void ExtraUtility()
        {
            ExtraSkillLocator extracomponent = characterPrefab.GetComponent<ExtraSkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL", "Noxian Shunpo");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL_DESCRIPTION", 
                "Dash in the <color=#E92630>blink of an eye</color> to target location for <style=cIsDamage>260% damage</style> on-arrival.");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Utility));
            SkillDef.activationStateMachineName = "Blink";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 9f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = false;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("utility");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_UTIL_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_UTIL";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_UTIL";

            ContentAddition.AddSkillDef(SkillDef);

            extracomponent.extraThird = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily katfifthfamily = ScriptableObject.CreateInstance<SkillFamily>();
            katfifthfamily.variants = new SkillFamily.Variant[1];
            extracomponent.extraThird.SetFieldValue("_skillFamily", katfifthfamily);
            SkillFamily skillFamily = extracomponent.extraThird.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
        }
        
        void SpecialSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC", "Death Lotus");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC_DESCRIPTION", 
                "<style=cIsUtility>Channel for 3 seconds,</style> rapidly throwing daggers at the nearest <style=cIsDamage>5 enemies</style> for <style=cIsDamage>195% damage</style> per dagger.");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Special));
            SkillDef.activationStateMachineName = "Body";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 45f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = true;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("special");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_SPEC_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_SPEC";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_SPEC";

            ContentAddition.AddSkillDef(SkillDef);

            component.special = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.special.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.special.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);

        }
    }
}

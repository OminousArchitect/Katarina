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
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SoundAPI), nameof(LanguageAPI), nameof(LoadoutAPI), nameof(DamageAPI), nameof(RecalculateStatsAPI), nameof(OrbAPI))]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Nines.Katarina";
        public const string MODNAME = "Katarina";
        public const string VERSION = "2.0.0";
        public const string SURVIVORNAME = "NinesKatarina";
        public const string SURVIVORNAMEKEY = "NINESKATARINA";
        public static GameObject characterPrefab;
        // Color used in characterbody
        private static readonly Color characterColor = new Color(0.925490196f, 0.435294118f, 0.91372549f);
        
        internal static ConfigEntry<KeyCode> skillSwitchToggle;
        internal static ConfigEntry<bool> resetUtilityOnPrimaryKill;
        internal static ConfigEntry<bool> vfxaim;
        internal static ConfigEntry<bool> vfxarrival;
        internal static ConfigEntry<bool> vfxtargetarrival;
        internal static ConfigEntry<bool> killexplosionfx;
        internal static ConfigEntry<bool> enablelotusvoice;
        internal static ConfigEntry<bool> enablemeleevoice;
        internal static ConfigEntry<bool> enabledaggervoice;
        internal static ConfigEntry<bool> silentshunpo;
        internal static ConfigEntry<bool> silentslash;
        internal static ConfigEntry<bool> randomdaggers;
        internal static ConfigEntry<bool> altlotusfx;
        internal static ConfigEntry<bool> chargemechanic;
        internal static ConfigEntry<bool> resetslashalt;

        internal static ConfigEntry<float> throwvelocity;
        internal static ConfigEntry<float> daggerpickupzone;
        internal static ConfigEntry<float> daggerpickupslash;
        internal static ConfigEntry<float> daggerhealpercent;
        internal static ConfigEntry<float> meleevamp;
        internal static ConfigEntry<float> blinkdmg;
        internal static ConfigEntry<float> ihateflyingenemies;
        internal static ConfigEntry<float> daggerspherebox;

        internal static Shader hopooshaders = Resources.Load<Shader>("Shaders/Deferred/HGStandard");
        public static BodyIndex vultureIndex;
        public static BodyIndex pestIndex;
        
        
        private void Awake()
        {
            vfxaim = base.Config.Bind<bool>(new ConfigDefinition("VFX", "Noxian Shunpo Aim FX"), true, new ConfigDescription("", null, Array.Empty<object>()));
            vfxarrival = base.Config.Bind<bool>(new ConfigDefinition("VFX", "Noxian Shunpo Arrival FX"), true, new ConfigDescription("", null, Array.Empty<object>())); 
            vfxtargetarrival = base.Config.Bind<bool>(new ConfigDefinition("VFX", "Scheman Shunpo FX"), true, new ConfigDescription("", null, Array.Empty<object>()));
            killexplosionfx = base.Config.Bind<bool>(new ConfigDefinition("VFX", "Bandit Explosion FX On Reset"), true, new ConfigDescription("", null, Array.Empty<object>()));
            altlotusfx = base.Config.Bind<bool>(new ConfigDefinition("VFX", "Alternate Death Lotus Projectiles"), false, new ConfigDescription("False for red daggers, True for Scheman magic bursts", null, Array.Empty<object>()));
            enablelotusvoice = Config.Bind<bool>(new ConfigDefinition("Voice", "Death Lotus Voice"), true, new ConfigDescription("", null, Array.Empty<object>()));
            enablemeleevoice = Config.Bind<bool>(new ConfigDefinition("Voice", "Sinister Steel Voice"), true, new ConfigDescription("", null, Array.Empty<object>()));
            enabledaggervoice = Config.Bind<bool>(new ConfigDefinition("Voice", "Voracious Blade Voice"), true, new ConfigDescription("", null, Array.Empty<object>()));
            resetUtilityOnPrimaryKill = base.Config.Bind<bool>(new ConfigDefinition("Sinister Steel", "Reset Shunpo Technique On Melee Kill"), false, new ConfigDescription("Brought back from 1.0 but disabled by default because it's kinda op with rework", null, Array.Empty<object>()));
            meleevamp = base.Config.Bind<float>(new ConfigDefinition("Sinister Steel", "Omnivamp"), 0.08f, new ConfigDescription("Percentage of damage dealt, 0.08 is 8%", null, Array.Empty<object>()));
            daggerpickupzone = base.Config.Bind(new ConfigDefinition("Daggers", "Pickup Zone Radius"), 5f, new ConfigDescription("", null,Array.Empty<object>()));
            daggerpickupslash = base.Config.Bind(new ConfigDefinition("Daggers", "Pickup Slash Radius"), 15f, new ConfigDescription("", null,Array.Empty<object>()));
            randomdaggers = base.Config.Bind<bool>(new ConfigDefinition("Daggers", "True Random Daggers"), false, new ConfigDescription("true to randomize her weapons and dagger pickups, false to sync them together", null, Array.Empty<object>()));
            throwvelocity = base.Config.Bind<float>(new ConfigDefinition("Daggers", "Velocity"), 185, new ConfigDescription("Default is 185, Serrated Shiv is 160", null, Array.Empty<object>()));
            daggerhealpercent = base.Config.Bind<float>(new ConfigDefinition("Daggers", "Omnivamp"), 0.1f, new ConfigDescription("Percentage of enemy max health, 0.1 is 10%", null, Array.Empty<object>()));
            daggerspherebox = base.Config.Bind<float>(new ConfigDefinition("Daggers", "Hitbox"), 1.2f, new ConfigDescription("Feel free to mess with this, it's measured as a radius. Serrated Shiv is 0.3", null, Array.Empty<object>()));
            skillSwitchToggle = base.Config.Bind<KeyCode>(new ConfigDefinition("Shunpo", "Keybind"), KeyCode.F, new ConfigDescription("", null, Array.Empty<object>()));
            blinkdmg = base.Config.Bind<float>(new ConfigDefinition("Shunpo", "Damage Coefficient"), 2f, new ConfigDescription("", null, Array.Empty<object>()));
            ihateflyingenemies = base.Config.Bind<float>(new ConfigDefinition("Shunpo", "Flying Enemy Damage Coefficient"), 4f, new ConfigDescription("", null, Array.Empty<object>()));
            silentshunpo = base.Config.Bind<bool>(new ConfigDefinition("Shunpo", "Quieter Shunpo"), false, new ConfigDescription("Enable or disable to mute the sound from the explosion vfx prefab", null, Array.Empty<object>()));
            silentslash = base.Config.Bind<bool>(new ConfigDefinition("Sinister Steel", "Quieter Impact"), true, new ConfigDescription("Mercenary slash impact sfx", null, Array.Empty<object>()));
            chargemechanic = base.Config.Bind<bool>(new ConfigDefinition("Daggers", "Charge Mechanic"), true, new ConfigDescription("Fire automatically when charged", null, Array.Empty<object>()));
            resetslashalt = base.Config.Bind<bool>(new ConfigDefinition("Daggers", "Experimental Alt Charge"), false, new ConfigDescription("Old W from before her rework (in League)", null, Array.Empty<object>()));
            
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (self, user, t) => { };
            Assets.PopulateAssets();
            Prefabs.CreatePrefabs();
            CreatePrefab();
            RegisterStates();
            RegisterCharacter();
            Hook.Hooks();
            RoR2Application.onLoad += () => 
            {   //cache BodyIndex on load
                vultureIndex = BodyCatalog.FindBodyIndex("VultureBody");
                pestIndex = BodyCatalog.FindBodyIndex("FlyingVerminBody");
            };
        }
        internal static void CreatePrefab()
        {
            characterPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MercBody"), SURVIVORNAME + "Body", true);
            characterPrefab.AddComponent<BladeController>();
            characterPrefab.AddComponent<KatarinaTracker>().maxTrackingDistance = 90f;
            characterPrefab.GetComponent<KatarinaTracker>().maxTrackingAngle = 18f;
            //characterPrefab.AddComponent<KatarinaSkillSwitchBehaviour>();
            characterPrefab.AddComponent<ExtraSkillLocator>();
            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            Destroy(characterPrefab.transform.Find("ModelBase").gameObject);
            Destroy(characterPrefab.transform.Find("CameraPivot").gameObject);
            Destroy(characterPrefab.transform.Find("AimOrigin").gameObject);

            GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>("katarina");

            GameObject ModelBase = new GameObject("ModelBase");
            ModelBase.transform.parent = characterPrefab.transform;
            ModelBase.transform.localPosition = new Vector3(0f, -0.94f, 0f);
            ModelBase.transform.localRotation = Quaternion.identity;
            ModelBase.transform.localScale = new Vector3(1.42f, 1.42f, 1.42f);

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = ModelBase.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 2f, 0f);
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
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 120;
            bodyComponent.levelMaxHealth = 35;
            bodyComponent.baseRegen = 1f;
            bodyComponent.levelRegen = 0.33f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0;
            bodyComponent.baseMoveSpeed = 7;
            bodyComponent.levelMoveSpeed = 0;
            bodyComponent.baseAcceleration = 110;
            bodyComponent.baseJumpPower = 20;
            bodyComponent.levelJumpPower = 0;
            bodyComponent.baseDamage = 15;
            bodyComponent.levelDamage = 2.8f;
            bodyComponent.baseAttackSpeed = 1.3f;
            bodyComponent.levelAttackSpeed = 0;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0;
            bodyComponent.baseArmor = 20;
            bodyComponent.levelArmor = 0.25f;
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
            characterMotor.mass = 160f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;

            InputBankTest inputBankTest = characterPrefab.GetComponent<InputBankTest>();
            inputBankTest.moveVector = Vector3.zero;

            CameraTargetParams cameraTargetParams = characterPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MercBody").GetComponent<CameraTargetParams>().cameraParams;
            cameraTargetParams.cameraParams.data.pivotVerticalOffset = 0.3f;
            //cameraTargetParams.cameraParams.data.fov = 120f;
            cameraTargetParams.cameraPivotTransform = null;
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
                    defaultMaterial = Utils.InstantiateMaterial(renderers[i].material.color, renderers[i].material.GetTexture("_MainTex"), Color.black, 0, null, 1, null),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
                    ignoreOverlays = false
                };
                rendererInfoList.Add(newRenderer);
            }
            characterModel.baseRendererInfos = rendererInfoList.ToArray(); //here right?
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
                skinDef,
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

            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherFootstepDust.prefab").WaitForCompletion();

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

            NetworkStateMachine networkStateMachine = bodyComponent.GetComponent<NetworkStateMachine>();
            var list = networkStateMachine.stateMachines.ToList();
            list.Add(newStateMachine);
            list.Add(newStateMachine2);
            list.Add(newStateMachine3);
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
                          "<style=cSub>\r\n\r\n< ! > Your melee attacks have innate execution damage, allowing you to clean up low-health enemies quickly after closing the distance with Shunpo."
                          + Environment.NewLine +
                          "<style=cSub>\r\n\r\n< ! > Your secondary resets only when killing with the single-dagger portion so choose accordingly between area or individual damage before firing."
                          + Environment.NewLine +
                          "<style=cSub>\r\n\r\n< ! > Shunpo is both a powerful offensive AND defensive tool with both techniques possessing independent cooldowns. The cooldown of your currently-equipped technique is set to 1 upon picking up a dagger."
                          + Environment.NewLine +
                          "<style=cSub>\r\n\r\n< ! > Learning to chain your abilities is key to maximizing both your damage output and your survivability.";

            string outro = "..and so she left, her skills (and blades) honed evermore.";
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
            
            //UtilitySetup();
            AltUtilitySetup();
            
            ExtraUtility();
            
            SpecialSetup();
        }
        void PassiveSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_NAME", "Voracity");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION", "Always looking for her next target, Katarina <color=#E92630>resets</color> her ability cooldowns <style=cIsDamage>on-kill</style> or <color=#E92630>with a dagger.</color>");

            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = SURVIVORNAMEKEY + "_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("Passive1");
        }
        void PrimarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1", "Sinister Steel");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1_DESCRIPTION", "<style=cIsDamage>Slayer.</style> Swipe your blades for <style=cIsDamage>180% damage.</style>");

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
            {
                "KEYWORD_SLAYER"
            };

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
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2", "Voracious Blade");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2_DESCRIPTION", "Throw a dagger for <style=cIsDamage>330% damage</style>, or hold to throw a fan of daggers for <style=cIsDamage>3x135% damage</style> that <style=cIsHealth>hemorrhage.</style>");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Secondary));
            SkillDef.activationStateMachineName = "Blade";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 5.5f;
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
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_M2_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_M2";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_M2";

            ContentAddition.AddSkillDef(SkillDef);

            component.secondary = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.secondary.skillFamily;

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
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL", "Noxian Shunpo");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL_DESCRIPTION", 
                "Dash in the <color=#E92630>blink of an eye</color> to target location for <style=cIsDamage>200% damage on-arrival.</style>");

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
        {
            //KatarinaSkillSwitchBehaviour switchComponent = characterPrefab.GetComponent<KatarinaSkillSwitchBehaviour>();
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "ALT_UTIL", "Scheman Shunpo");
            LanguageAPI.Add(SURVIVORNAMEKEY + "ALT_UTIL_DESCRIPTION",
                "_M2_DESCRIPTION", "Dash in the <color=#E92630>blink of an eye</color> to target enemy for <style=cIsDamage>200% damage on arrival.</style>");

            var SkillDef = ScriptableObject.CreateInstance<KatarinaSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(AltUtility));
            SkillDef.activationStateMachineName = "Blink";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 5f;
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

            /*
            var genericskill = component.gameObject.AddComponent<GenericSkill>();
            genericskill.hideInCharacterSelect = true;
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            genericskill.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = genericskill.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
            */
            
        }

        void ExtraUtility()
        {
            ExtraSkillLocator extracomponent = characterPrefab.GetComponent<ExtraSkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL", "Noxian Shunpo");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL_DESCRIPTION", 
                "Dash in the <color=#E92630>blink of an eye</color> to target location for <style=cIsDamage>200% damage on-arrival.</style>");

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
                "<style=cIsUtility>Channel for 3 seconds,</style> rapidly throwing daggers at the nearest <style=cIsDamage>5 enemies</style> for <style=cIsDamage>170% damage</style> per dagger.");

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

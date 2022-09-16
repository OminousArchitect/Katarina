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

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace SurvivorTemplate
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(MODUID, MODNAME, VERSION)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SoundAPI), nameof(LanguageAPI), nameof(LoadoutAPI), nameof(DamageAPI), nameof(RecalculateStatsAPI))]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Dragonyck.SurvivorTemplate";
        public const string MODNAME = "SurvivorTemplate";
        public const string VERSION = "1.0.0";
        public const string SURVIVORNAME = "Survivor";
        public const string SURVIVORNAMEKEY = "SURVIVOR";
        public static GameObject characterPrefab;
        public GameObject characterDisplay;
        public GameObject doppelganger;
        private static readonly Color characterColor = new Color(0.7f, 0.7f, 0.7f);

        private void Awake()
        {
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (self, user, t) => { };

            Assets.PopulateAssets();
            //Achievements.RegisterUnlockables();
            Prefabs.CreatePrefabs();
            CreatePrefab();
            RegisterStates();
            RegisterCharacter();
            CreateDoppelganger();
            Hook.Hooks();
            //ItemDisplays.RegisterDisplays();
            //ItemDisplays.PopulateDisplays();
        }
        private static GameObject CreateModel(GameObject main)
        {
            Destroy(main.transform.Find("ModelBase").gameObject);
            Destroy(main.transform.Find("CameraPivot").gameObject);
            Destroy(main.transform.Find("AimOrigin").gameObject);

            GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>("Survivor");

            return model;
        }
        internal static void CreatePrefab()
        {
            characterPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), SURVIVORNAME + "Body", true);
            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            GameObject createModel = CreateModel(characterPrefab);

            GameObject ModelBase = new GameObject("ModelBase");
            ModelBase.transform.parent = characterPrefab.transform;
            ModelBase.transform.localPosition = new Vector3(0f, -0.94f, 0f);
            ModelBase.transform.localRotation = Quaternion.identity;
            ModelBase.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = ModelBase.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 2f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = createModel.transform;
            transform.parent = ModelBase.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = characterPrefab.GetComponent<CharacterDirection>();
            characterDirection.moveVector = Vector3.zero;
            characterDirection.targetTransform = ModelBase.transform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = createModel.GetComponentInChildren<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            CharacterBody bodyComponent = characterPrefab.GetComponent<CharacterBody>();
            bodyComponent.name = SURVIVORNAME + "Body";
            bodyComponent.baseNameToken = SURVIVORNAMEKEY + "_NAME";
            bodyComponent.subtitleNameToken = SURVIVORNAMEKEY + "_SUBTITLE";
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 100;
            bodyComponent.levelMaxHealth = 35;
            bodyComponent.baseRegen = 1f;
            bodyComponent.levelRegen = 0.33f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0;
            bodyComponent.baseMoveSpeed = 7;
            bodyComponent.levelMoveSpeed = 0;
            bodyComponent.baseAcceleration = 110;
            bodyComponent.baseJumpPower = 15;
            bodyComponent.levelJumpPower = 0;
            bodyComponent.baseDamage = 12;
            bodyComponent.levelDamage = 2.4f;
            bodyComponent.baseAttackSpeed = 1;
            bodyComponent.levelAttackSpeed = 0;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0;
            bodyComponent.baseArmor = 0;
            bodyComponent.levelArmor = 0;
            bodyComponent.baseJumpCount = 1;
            bodyComponent.sprintingSpeedMultiplier = 1.45f;
            bodyComponent.wasLucky = false;
            //bodyComponent._defaultCrosshairPrefab = null;
            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            //bodyComponent.portraitIcon = Assets.charPortrait;
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
            cameraTargetParams.cameraParams = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CameraTargetParams>().cameraParams;
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

            ChildLocator childLocator = createModel.GetComponent<ChildLocator>();

            CharacterModel characterModel = createModel.AddComponent<CharacterModel>();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = createModel.GetComponentInChildren<SkinnedMeshRenderer>().material,
                    renderer = createModel.GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    renderer = childLocator.FindChild("Head").GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultMaterial = childLocator.FindChild("Head").GetComponentInChildren<SkinnedMeshRenderer>().material,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    renderer = childLocator.FindChild("Wheel").GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultMaterial = childLocator.FindChild("Wheel").GetComponentInChildren<SkinnedMeshRenderer>().material,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    renderer = childLocator.FindChild("StickyBombs").GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultMaterial = childLocator.FindChild("StickyBombs").GetComponentInChildren<SkinnedMeshRenderer>().material,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
            };
            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();
            Reflection.SetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer", characterModel.baseRendererInfos[0].renderer.gameObject.GetComponent<SkinnedMeshRenderer>());

            GameObject gameObject = MainPlugin.characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel component = gameObject.GetComponent<CharacterModel>();
            ModelSkinController modelSkinController = gameObject.AddComponent<ModelSkinController>();
            ChildLocator component2 = gameObject.GetComponent<ChildLocator>();
            SkinnedMeshRenderer fieldValue = Reflection.GetFieldValue<SkinnedMeshRenderer>(component, "mainSkinnedMeshRenderer");
            LanguageAPI.Add(SURVIVORNAMEKEY + "BODY_DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add(SURVIVORNAMEKEY + "BODY_MASTERY_SKIN_NAME", "Mastery");

            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo.GameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = component2.FindChild("Head").gameObject,
                    shouldActivate = true
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = component2.FindChild("Wheel").gameObject,
                    shouldActivate = true
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = component2.FindChild("StickyBombs").gameObject,
                    shouldActivate = true
                },
            };
            skinDefInfo.Icon = LoadoutAPI.CreateSkinIcon(Color.white, new Color(0.5686275f, 0.6745098f, 0.8117647f), new Color(0.6784314f, 0.6666667f, 0.7098039f), new Color(0.1490196f, 0.172549f, 0.2352941f));
            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = fieldValue,
                    mesh = fieldValue.sharedMesh
                },
                new SkinDef.MeshReplacement
                {
                    renderer = defaultRenderers[1].renderer,
                    mesh = component2.FindChild("Head").gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh
                },
                new SkinDef.MeshReplacement
                {
                    renderer = defaultRenderers[2].renderer,
                    mesh = component2.FindChild("Wheel").gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh
                },
                new SkinDef.MeshReplacement
                {
                    renderer = defaultRenderers[3].renderer,
                    mesh = component2.FindChild("StickyBombs").gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh
                },
            };
            skinDefInfo.Name = SURVIVORNAMEKEY + "BODY_DEFAULT_SKIN_NAME";
            skinDefInfo.NameToken = SURVIVORNAMEKEY + "BODY_DEFAULT_SKIN_NAME";
            skinDefInfo.RendererInfos = component.baseRendererInfos;
            skinDefInfo.RootObject = gameObject;
            skinDefInfo.UnlockableDef = null;
            CharacterModel.RendererInfo[] rendererInfos = skinDefInfo.RendererInfos;
            CharacterModel.RendererInfo[] array = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(array, 0);
            Material material = array[0].defaultMaterial;
            bool flag = material;
            if (flag)
            {
                material = Utils.InstantiateMaterial(Color.white, Assets.MainAssetBundle.LoadAsset<Material>("bomber").GetTexture("_MainTex"), Color.black, 0, null, 1, null);
                array[0].defaultMaterial = material;
                array[1].defaultMaterial = material;
                array[2].defaultMaterial = material;
            }
            material = array[1].defaultMaterial;
            material = array[2].defaultMaterial;
            bool flag1 = material;
            if (flag1)
            {
                material = Utils.InstantiateMaterial(Color.white, Assets.MainAssetBundle.LoadAsset<Material>("stickybomb").GetTexture("_MainTex"), new Color(0, 2, 0.9960784f),
                    1, Assets.MainAssetBundle.LoadAsset<Material>("stickybomb").GetTexture("_EmissionMap"), 1, null);
                array[3].defaultMaterial = material;
            }
            material = array[3].defaultMaterial;
            skinDefInfo.RendererInfos = array;
            SkinDef skinDef = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            /////////////// SKIN 02 //////////////////////
            ///

            LoadoutAPI.SkinDefInfo skinDefInfo2 = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo2.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo2.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo2.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo2.GameObjectActivations = new SkinDef.GameObjectActivation[0];
            skinDefInfo2.Icon = LoadoutAPI.CreateSkinIcon(Color.white, new Color(0.4941176f, 0.509804f, 0.5568628f), new Color(0.1803922f, 0.2196078f, 0.2117647f), new Color(0.03529412f, 0.09019608f, 0.1803922f));
            skinDefInfo2.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = fieldValue,
                    mesh = Assets.MainAssetBundle.LoadAsset<Mesh>("mBomber"),
                },
                new SkinDef.MeshReplacement
                {
                    renderer = array[1].renderer,
                    mesh = Assets.MainAssetBundle.LoadAsset<Mesh>("mHead"),
                },
            };
            skinDefInfo2.Name = SURVIVORNAMEKEY + "BODY_MASTERY_SKIN_NAME";
            skinDefInfo2.NameToken = SURVIVORNAMEKEY + "BODY_MASTERY_SKIN_NAME";
            skinDefInfo2.RendererInfos = component.baseRendererInfos;
            skinDefInfo2.RootObject = gameObject;
            skinDefInfo2.UnlockableDef = null;
            rendererInfos = skinDefInfo.RendererInfos;
            array = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(array, 0);
            material = array[0].defaultMaterial;
            if (flag)
            {
                material = Utils.InstantiateMaterial(Color.white, Assets.MainAssetBundle.LoadAsset<Material>("masterybomber").GetTexture("_MainTex"), Color.black, 0, null, 1, null);
                array[0].defaultMaterial = material;
                array[2].defaultMaterial = material;
            }
            material = array[0].defaultMaterial;
            if (flag)
            {
                material = Assets.MainAssetBundle.LoadAsset<Material>("masterybomberhead");
                array[1].defaultMaterial = material;
            }
            material = array[1].defaultMaterial;
            skinDefInfo2.RendererInfos = array;
            SkinDef skinDef2 = LoadoutAPI.CreateNewSkinDef(skinDefInfo2);

            modelSkinController.skins = new SkinDef[]
            {
                skinDef,
                skinDef2,
            };

            // RAGDOLL SETUP

            var ragdollMaterial = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RagdollController>().bones[1].GetComponent<Collider>().material;

            var ragdoll = createModel.AddComponent<RagdollController>();
            ragdoll.bones = new Transform[]
            {
                component2.FindChild("stomach"),
                component2.FindChild("chest"),
                component2.FindChild("head"),
                component2.FindChild("bomb.r"),
                component2.FindChild("bomb.l"),
                component2.FindChild("lower_arm.r"),
                component2.FindChild("upper_arm.r"),
                component2.FindChild("upper_arm.l"),
                component2.FindChild("lower_arm.l"),
                component2.FindChild("wheelbone"),
            };

            foreach (Transform i in ragdoll.bones)
            {
                if (i)
                {
                    i.gameObject.layer = LayerIndex.ragdoll.intVal;
                    Collider l = i.GetComponent<Collider>();
                    if (l)
                    {
                        l.material = ragdollMaterial;
                        l.sharedMaterial = ragdollMaterial;
                    }
                }
            }

            TeamComponent teamComponent = null;
            if (characterPrefab.GetComponent<TeamComponent>() != null) teamComponent = characterPrefab.GetComponent<TeamComponent>();
            else teamComponent = characterPrefab.GetComponent<TeamComponent>();
            teamComponent.hideAllyCardDisplay = false;
            teamComponent.teamIndex = TeamIndex.None;

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
            sfxLocator.deathSound = "Play_ui_player_death";
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

            HurtBoxGroup hurtBoxGroup = createModel.AddComponent<HurtBoxGroup>();

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

            AimAnimator aimAnimator = createModel.AddComponent<AimAnimator>();
            aimAnimator.inputBank = inputBankTest;
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMax = 55f;
            aimAnimator.pitchRangeMin = -50f;
            aimAnimator.yawRangeMin = -44f;
            aimAnimator.yawRangeMax = 44f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 8f;

            FootstepHandler footstepHandler = createModel.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");

            EntityStateMachine mainStateMachine = bodyComponent.GetComponent<EntityStateMachine>();
            mainStateMachine.mainStateType = new SerializableEntityStateType(typeof(CharacterMain));

            CharacterDeathBehavior characterDeathBehavior = characterPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(EntityStates.Commando.DeathState));

            var newStateMachine = characterPrefab.AddComponent<EntityStateMachine>();
            newStateMachine.customName = "PoppyBomb";
            newStateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));
            newStateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));

            var newStateMachine2 = characterPrefab.AddComponent<EntityStateMachine>();
            newStateMachine2.customName = "PoppyBombDetonate";
            newStateMachine2.initialStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));
            newStateMachine2.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));

            NetworkStateMachine networkStateMachine = bodyComponent.GetComponent<NetworkStateMachine>();
            var list = networkStateMachine.stateMachines.ToList();
            list.Add(newStateMachine);
            list.Add(newStateMachine2);
            networkStateMachine.stateMachines = list.ToArray();

            ContentAddition.AddBody(characterPrefab);
        }
        private void RegisterCharacter()
        {
            characterDisplay = PrefabAPI.InstantiateClone(characterPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, SURVIVORNAME + "Display", true);
            characterDisplay.AddComponent<NetworkIdentity>();
            var c = characterDisplay.GetComponentInChildren<ModelSkinController>().skins;
            for (int i = 0; i < c.Length; i++)
            {
                var r = c[i].rendererInfos;
                for (int re = 0; re < r.Length; re++)
                {
                    if (r[re].defaultMaterial.name.Contains("matCommandoDualies(Clone)"))
                        r[re].defaultMaterial.shaderKeywords = null;
                }
            }

            string desc = "" +
                "<style=cSub>\r\n\r\n< ! > "
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > "
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > "
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > ";

            string outro = "..and so he left, seeking the destruction of his next target.";
            string fail = "..and so he vanished, saddened by the missed opportunities.";

            LanguageAPI.Add(SURVIVORNAMEKEY + "_NAME", SURVIVORNAME);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_DESCRIPTION", desc);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SUBTITLE", "SubHuman");
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
                survivorDef.desiredSortPosition = 0.2f;
                survivorDef.mainEndingEscapeFailureFlavorToken = SURVIVORNAMEKEY + "_FAIL";
            };

            ContentAddition.AddSurvivorDef(survivorDef);

            SkillSetup();
        }
        void RegisterStates()
        {
            bool hmm;
            ContentAddition.AddEntityState<Primary>(out hmm);
            ContentAddition.AddEntityState<Secondary>(out hmm);
            ContentAddition.AddEntityState<Utility>(out hmm);
            ContentAddition.AddEntityState<Special>(out hmm);
            ContentAddition.AddEntityState<CharacterMain>(out hmm);
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
            UtilitySetup();
            SpecialSetup();

        }
        void PassiveSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_NAME", "");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION", "");

            component.passiveSkill.enabled = false;
            component.passiveSkill.skillNameToken = SURVIVORNAMEKEY + "_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("passive");
        }
        void PrimarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1", "");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1_DESCRIPTION", "");

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
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2", "");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2_DESCRIPTION", "");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Secondary));
            SkillDef.activationStateMachineName = "Slide";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
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
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL", "");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL_DESCRIPTION", "");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Utility));
            SkillDef.activationStateMachineName = "PoppyBomb";
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
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("utility");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_UTIL_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_UTIL";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_UTIL";

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
        void SpecialSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC", "");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC_DESCRIPTION", "");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Special));
            SkillDef.activationStateMachineName = "Slide";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 8f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
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

        private void CreateDoppelganger()
        {

            doppelganger = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/CommandoMonsterMaster"), SURVIVORNAME + "Master", true);

            ContentAddition.AddMaster(doppelganger);

            CharacterMaster component = doppelganger.GetComponent<CharacterMaster>();
            component.bodyPrefab = characterPrefab;
        }
    }
}

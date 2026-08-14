using GDEngine.Core;
using GDEngine.Core.Audio;
using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Components.Controllers.Physics;
using GDEngine.Core.Debug;
using GDEngine.Core.Entities;
using GDEngine.Core.Events;
using GDEngine.Core.Factories;
using GDEngine.Core.Managers;
using GDEngine.Core.Orchestration;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Rendering.UI;
using GDEngine.Core.Screen;
using GDEngine.Core.Serialization;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using GDEngine.Core.Timing;
using GDEngine.Core.Utilities;
using GDGame.Demos.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using GDGame.Zones;
using GDGame.Zones.Shared;

namespace GDGame
{
    public class Main : Game
    {
        #region Core Fields (Common to all games)   
        
        private GraphicsDeviceManager _graphics;
        private ContentDictionary<Texture2D> _textureDictionary;
        private ContentDictionary<Model> _modelDictionary;
        private ContentDictionary<SpriteFont> _fontDictionary;
        private ContentDictionary<SoundEffect> _soundDictionary;
        private ContentDictionary<Effect> _effectsDictionary;
        private bool _disposed = false;
        private Material _matBasicUnlit, _matBasicLit, _matAlphaCutout, _matBasicUnlitGround;
        private PBRMaterial _matPBR;

        #endregion

        #region Engine & Scene Management Fields
        private SceneManager _sceneManager;
        private MenuManager _menuManager;
        private UIDebugInfo _debugRenderer;
        private float _lastPortalTriggerTime = float.NegativeInfinity;
        private const float PORTAL_COOLDOWN_SECONDS = 2f;
        #endregion

        #region Core Methods (Common to all games)     
        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            #region Core
            Window.Title = "My Amazing Game";
            InitializeGraphics(ScreenResolution.R_WXGA_16_10_1280x800);
            InitializeMouse();
            InitializeContext();

            var relativeFilePathAndName = "assets/data/asset_manifest.json";
            LoadAssetsFromJSON(relativeFilePathAndName);
            InitializeEffects();

            // Game component that exists outside scene to manage and swap scenes
            InitializeSceneManager();

            // Create the scene and register it
            InitializeScene();


            EngineContext.Instance.Events.Subscribe<GDEngine.Core.Events.TriggerEvent>(evt =>
            {
                var portal = evt.TriggerBody?.GameObject?.GetComponent<ZonePortal>();
                if (portal != null)
                {
                    if (Time.TimeSinceStartupSecs - _lastPortalTriggerTime < PORTAL_COOLDOWN_SECONDS)
                    {
                        return;
                    }
                    _lastPortalTriggerTime = Time.TimeSinceStartupSecs;

                    _pendingSceneName = portal.TargetSceneName;
                }
            });

            // Safe to use _sceneManager.ActiveScene from here on
            InitializeSystems();
            InitializeCameras();
            InitializeCameraManagers();

            int scale = 500;
            InitializeSkyParent();
            InitializeSkyBox(scale);
            InitializeCollidableGround(scale);
            #endregion

            // Mouse reticle
            InitializeUI();

            // Main menu
            InitializeMenuManager();

            // Set pause and show menu
            SetPauseShowMenu();

            // Set the active scene
            _sceneManager.SetActiveScene(HubSceneBuilder.SCENE_NAME);

            base.Initialize();
        }

        private void SetPauseShowMenu()
        {
            // Give scenemanager the events reference so that it can publish the pause event
            _sceneManager.EventBus = EngineContext.Instance.Events;
            // Set paused and publish pause event
            _sceneManager.Paused = true;

            // Put all components that should be paused to sleep
            EngineContext.Instance.Events.Subscribe<GamePauseChangedEvent>(e =>
            {
                bool paused = e.IsPaused;

                _sceneManager.ActiveScene.GetSystem<PhysicsSystem>()?.SetPaused(paused);
                _sceneManager.ActiveScene.GetSystem<PhysicsDebugSystem>()?.SetPaused(paused);
                _sceneManager.ActiveScene.GetSystem<GameStateSystem>()?.SetPaused(paused);
            });
        }

        private void InitializeSceneManager()
        {
            _sceneManager = new SceneManager(this);
            Components.Add(_sceneManager);
        }

        private string _pendingSceneName = null;
        private PhysicsZoneBuilder _physicsZoneBuilder;
        private AudioZoneBuilder _audioZoneBuilder;
        private CameraZoneBuilder _cameraZoneBuilder;
        private OrchestrationZoneBuilder _orchestrationZoneBuilder;
        private EventsStateZoneBuilder _eventsStateZoneBuilder;
        private HubSceneBuilder _hubBuilder;
        private void InitializeCameraManagers()
        {
            //inside scene
            var go = new GameObject("Camera Manager");
            go.AddComponent<CameraEventListener>();
            _sceneManager.ActiveScene.Add(go);
        }

        private void InitializeMenuManager()
        {
            _menuManager = new MenuManager(this, _sceneManager);
            Components.Add(_menuManager);

            RebindMenuToActiveScene();

            // Subscribe to high-level events
            _menuManager.PlayRequested += () =>
            {
                _sceneManager.Paused = false;
                _menuManager.HideMenus();

                //fade out menu sound
            };

            _menuManager.ExitRequested += () =>
            {
                Exit();
            };

            _menuManager.MusicVolumeChanged += v =>
            {
                // Forward to audio manager
                System.Diagnostics.Debug.WriteLine("MusicVolumeChanged");

                //raise event to set sound
               // EngineContext.Instance.Events.Publish(new PlaySfxEvent)
            };

            _menuManager.SfxVolumeChanged += v =>
            {
                // Forward to audio manager
                System.Diagnostics.Debug.WriteLine("SfxVolumeChanged");

                //raise event to set sound
            };

   
        }

        private void RebindMenuToActiveScene()
        {
            Texture2D btnTex = _textureDictionary.Get("button_rectangle_10");
            Texture2D trackTex = _textureDictionary.Get("Free Flat Hyphen Icon");
            Texture2D handleTex = _textureDictionary.Get("Free Flat Toggle Thumb Centre Icon");
            SpriteFont uiFont = _fontDictionary.Get("menufont");

            _menuManager.Initialize(_sceneManager.ActiveScene, btnTex, trackTex, handleTex, uiFont,
                _textureDictionary.Get("mainmenu_monkey"),
                _textureDictionary.Get("audiomenu_monkey"),
                _textureDictionary.Get("controlsmenu_monkey"));
        }

        private void InitializeCollidableGround(int scale = 500)
        {
            GameObject gameObject = null;
            MeshFilter meshFilter = null;
            MeshRenderer meshRenderer = null;

            gameObject = new GameObject("ground");
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            meshFilter = MeshFilterFactory.CreateQuadGridTexturedUnlit(_graphics.GraphicsDevice,
                 1,
                 1,
                 1,
                 1,
                 20,
                 20);

            gameObject.Transform.ScaleBy(new Vector3(scale, scale, 1));
            gameObject.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(-90), 0, 0), true);
            gameObject.Transform.TranslateTo(new Vector3(0, -0.5f, 0));

            gameObject.AddComponent(meshFilter);
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.Material = _matBasicUnlitGround;
            meshRenderer.Overrides.MainTexture = _textureDictionary.Get("ground_grass");

            // Add a box collider matching the ground size
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.Size = new Vector3(scale, scale, 0.025f);
            collider.Center = new Vector3(0, 0, -0.0125f);

            // Add rigidbody as Static (immovable)
            var rigidBody = gameObject.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;
            gameObject.IsStatic = true;

            gameObject.Layer = LayerMask.Ground;

            _sceneManager.ActiveScene.Add(gameObject);
        }

        private void InitializeGraphics(Integer2 resolution)
        {
            // Enable per-monitor DPI awareness so the window/UI scales crisply on multi-monitor setups with different DPIs (avoids blurriness when moving between screens).
            System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.PerMonitorV2);

            // Set preferred resolution
            ScreenResolution.SetResolution(_graphics, resolution);

            // Center on primary display (set to index of the preferred monitor)
            WindowUtility.CenterOnMonitor(this, 1);
        }

        private void InitializeMouse()
        {
            Mouse.SetPosition(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
        }

        private void InitializeContext()
        {
            EngineContext.Initialize(GraphicsDevice, Content);
        }

        /// <summary>
        /// New asset loading from JSON using AssetEntry and ContentDictionary::LoadFromManifest
        /// </summary>
        /// <param name="relativeFilePathAndName"></param>
        /// <see cref="AssetEntry"/>
        /// <see cref="ContentDictionary{T}"/>
        private void LoadAssetsFromJSON(string relativeFilePathAndName)
        {
            // Make dictionaries to store assets
            _textureDictionary = new ContentDictionary<Texture2D>();
            _modelDictionary = new ContentDictionary<Model>();
            _fontDictionary = new ContentDictionary<SpriteFont>();
            _soundDictionary = new ContentDictionary<SoundEffect>();
            _effectsDictionary = new ContentDictionary<Effect>();
            //TODO - Add dictionary loading for other assets - song, other?

            var manifests = JSONSerializationUtility.LoadData<AssetManifest>(Content, relativeFilePathAndName); // single or array
            if (manifests.Count > 0)
            {
                foreach (var m in manifests)
                {
                    _modelDictionary.LoadFromManifest(m.Models, e => e.Name, e => e.ContentPath, overwrite: true);
                    _textureDictionary.LoadFromManifest(m.Textures, e => e.Name, e => e.ContentPath, overwrite: true);
                    _fontDictionary.LoadFromManifest(m.Fonts, e => e.Name, e => e.ContentPath, overwrite: true);
                    _soundDictionary.LoadFromManifest(m.Sounds, e => e.Name, e => e.ContentPath, overwrite: true);
                    _effectsDictionary.LoadFromManifest(m.Effects, e => e.Name, e => e.ContentPath, overwrite: true);
                    //TODO - Add dictionary loading for other assets - song, other?
                }
            }
        }

        private void InitializeEffects()
        {
            #region Unlit Textured BasicEffect 
            var unlitBasicEffect = new BasicEffect(_graphics.GraphicsDevice)
            {
                TextureEnabled = true,
                LightingEnabled = false,
                VertexColorEnabled = false
            };
   
            _matBasicUnlit = new Material(unlitBasicEffect);
            _matBasicUnlit.StateBlock = RenderStates.Opaque3D();      // depth on, cull CCW
            _matBasicUnlit.SamplerState = SamplerState.LinearClamp;   // helps avoid texture seams on sky

            //ground texture where UVs above [0,0]-[1,1]
            _matBasicUnlitGround = new Material(unlitBasicEffect.Clone());
            _matBasicUnlitGround.StateBlock = RenderStates.Opaque3D();      // depth on, cull CCW
            _matBasicUnlitGround.SamplerState = SamplerState.AnisotropicWrap;   // wrap texture based on UV values

            #endregion

            #region Lit Textured BasicEffect 
            var litBasicEffect = new BasicEffect(_graphics.GraphicsDevice)
            {
                TextureEnabled = true,
                LightingEnabled = true,
                PreferPerPixelLighting = true,
                VertexColorEnabled = false
            };
            litBasicEffect.EnableDefaultLighting();
            //litBasicEffect.AmbientLightColor = Color.Red.ToVector3();
            //litBasicEffect.EmissiveColor = Color.Green.ToVector3();
            //litBasicEffect.FogEnabled = true;
            //litBasicEffect.FogColor = Color.LightGray.ToVector3();
            //litBasicEffect.FogStart = 1;
            //litBasicEffect.FogEnd = 100;
            //litBasicEffect.SpecularPower = 8;  //int, power of 2, 1, 2, 4, 8
            //litBasicEffect.SpecularColor = Color.Yellow.ToVector3();
            _matBasicLit = new Material(litBasicEffect);  
            _matBasicLit.StateBlock = RenderStates.Opaque3D();

            #endregion

            #region Alpha-test for foliage/billboards
            var alphaFx = new AlphaTestEffect(GraphicsDevice)
            {
                VertexColorEnabled = false
            };
            _matAlphaCutout = new Material(alphaFx);

            // Depth test/write on; no blending (cutout happens in the effect). 
            // Make it two-sided so the quad is visible from both sides.
            _matAlphaCutout.StateBlock = RenderStates.Cutout3D()
                .WithRaster(new RasterizerState { CullMode = CullMode.None });

            // Clamp avoids edge bleeding from transparent borders.
            // (Use LinearWrap if the foliage textures tile.)
            _matAlphaCutout.SamplerState = SamplerState.LinearClamp;

            #endregion

        }

        private void InitializeScene()
        {
            var buildContext = new ZoneBuildContext(
                EngineContext.Instance,
                _graphics,
                _sceneManager,
                _modelDictionary,
                _textureDictionary,
                _fontDictionary,
                _soundDictionary,
                _matBasicLit,
                _matBasicUnlitGround);

            _hubBuilder = new HubSceneBuilder();
            var hubScene = _hubBuilder.Build(buildContext);

            _physicsZoneBuilder = new PhysicsZoneBuilder();
            var physicsZoneScene = _physicsZoneBuilder.Build(buildContext);
            _sceneManager.AddScene(_physicsZoneBuilder.SceneName, physicsZoneScene);
            _sceneManager.AddScene(_hubBuilder.SceneName, hubScene);
            _sceneManager.SetActiveScene(_hubBuilder.SceneName);

            _audioZoneBuilder = new AudioZoneBuilder();
            var audioZoneScene = _audioZoneBuilder.Build(buildContext);
            _sceneManager.AddScene(_audioZoneBuilder.SceneName, audioZoneScene);

            _cameraZoneBuilder = new CameraZoneBuilder();
            var cameraZoneScene = _cameraZoneBuilder.Build(buildContext);
            _sceneManager.AddScene(_cameraZoneBuilder.SceneName, cameraZoneScene);

            _orchestrationZoneBuilder = new OrchestrationZoneBuilder();
            var orchestrationZoneScene = _orchestrationZoneBuilder.Build(buildContext);
            _sceneManager.AddScene(_orchestrationZoneBuilder.SceneName, orchestrationZoneScene);

            _eventsStateZoneBuilder = new EventsStateZoneBuilder();
            var eventsStateZoneScene = _eventsStateZoneBuilder.Build(buildContext);
            _sceneManager.AddScene(_eventsStateZoneBuilder.SceneName, eventsStateZoneScene);
        }

        private void InitializeSystems()
        {
            //physics, event, input, camera, render, ui, audio, impulse and iu event system are added by ZoneSystemFactory
            //only the systems not yet covered by the zone-builder pattern remain here
            InitializePhysicsDebugSystem(false);
            InitializeOrchestrationSystem(false);   //show debugger
            InitializeDebugInfo(false);
        }

        private void InitializeDebugInfo(bool showDebug)
        {
            if (showDebug)
            {
                GameObject debugGO = new GameObject("Perf Stats");
                _debugRenderer = debugGO.AddComponent<UIDebugInfo>();

                _debugRenderer.Font = _fontDictionary.Get("perf_stats_font");
                _debugRenderer.ScreenCorner = ScreenCorner.TopLeft;
                _debugRenderer.Margin = new Vector2(10f, 10f);

                var perfProvider = new PerformanceDebugInfoProvider
                {
                    Profile = DisplayProfile.Profiling,
                    ShowMemoryStats = true
                };

                //add memory related info
                _debugRenderer.Providers.Add(perfProvider);

                //add scene related info
                _debugRenderer.Providers.Add(_sceneManager);

                _sceneManager.ActiveScene.Add(debugGO);
            }
        }

        private void InitializeOrchestrationSystem(bool debugEnabled)
        {
            var orchestrationSystem = new OrchestrationSystem();
            orchestrationSystem.Configure(options =>
            {
                options.Time = Orchestrator.OrchestrationTime.Unscaled;
                options.LocalScale = 1;
                options.Paused = false;
            });
            _sceneManager.ActiveScene.Add(orchestrationSystem);

            // Debugger
            if (debugEnabled)
            {
                GameObject debugGO = new GameObject("Perf Stats");
                var _debugRenderer = debugGO.AddComponent<UIDebugInfo>();

                _debugRenderer.Font = _fontDictionary.Get("perf_stats_font");
                _debugRenderer.ScreenCorner = ScreenCorner.TopLeft;
                _debugRenderer.Margin = new Vector2(10f, 10f);

                // Register orchestration as a debug provider
                if (orchestrationSystem != null)
                    _debugRenderer.Providers.Add(orchestrationSystem);

                var perfProvider = new PerformanceDebugInfoProvider
                {
                    Profile = DisplayProfile.Profiling,
                    ShowMemoryStats = true
                };

                _debugRenderer.Providers.Add(perfProvider);

                _sceneManager.ActiveScene.Add(debugGO);
            }

        }

        private void InitializePhysicsDebugSystem(bool isEnabled)
        {
            if (isEnabled)
            {
                var physicsDebugRenderer = _sceneManager.ActiveScene.AddSystem(new PhysicsDebugSystem());

                // Toggle debug rendering on/off
                physicsDebugRenderer.Enabled = isEnabled; // or false to hide

                // Optional: Customize colors
                physicsDebugRenderer.StaticColor = Color.Green;      // Immovable objects
                physicsDebugRenderer.KinematicColor = Color.Blue;    // Animated objects
                physicsDebugRenderer.DynamicColor = Color.Yellow;    // Physics-driven objects
                physicsDebugRenderer.TriggerColor = Color.Red;       // Trigger volumes

            }

        }

        private void InitializeCameras()
        {
            Scene scene = _sceneManager.ActiveScene;

            GameObject cameraGO = null;
            Camera camera = null;
           
            #region Static birds-eye camera
            cameraGO = new GameObject(AppData.CAMERA_NAME_STATIC_BIRDS_EYE);
            camera = cameraGO.AddComponent<Camera>();
            camera.FieldOfView = MathHelper.ToRadians(80);
            //ISRoT
            cameraGO.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(-90), 0, 0));
            cameraGO.Transform.TranslateTo(Vector3.UnitY * 50);
            scene.Add(cameraGO);
            #endregion

            #region Third-person camera
            cameraGO = new GameObject(AppData.CAMERA_NAME_THIRD_PERSON);
            camera = cameraGO.AddComponent<Camera>();

            var thirdPersonController = new ThirdPersonController();
            thirdPersonController.TargetName = AppData.PLAYER_NAME;
            thirdPersonController.ShoulderOffset = 0;
            thirdPersonController.FollowDistance = 50;
            thirdPersonController.RotationDamping = 20;
            cameraGO.AddComponent(thirdPersonController);
            scene.Add(cameraGO);
            #endregion

            #region First-person capsule + camera (parent/child)

            // PARENT: physics + movement (feet at y = 0 here)
            var parentGO = new GameObject(AppData.CAMERA_NAME_FIRST_PERSON_PARENT);
            parentGO.Layer = LayerMask.IgnoreRaycast;
            parentGO.Transform.TranslateTo(new Vector3(0f, 5f, 0f));

            // Capsule + rigidbody controller (kept upright internally)
            var fpsController = parentGO.AddComponent<FirstPersonCapsuleController>();
            fpsController.MoveSpeed = 8.0f;
            fpsController.Acceleration = 50.0f;
            fpsController.GroundFriction = 10.0f;
            fpsController.JumpImpulse = 7.0f;
            fpsController.CapsuleRadius = 0.5f;
            fpsController.CapsuleHeight = 1.8f;
            fpsController.GroundCheckDistance = 0.25f;

            // camera that can pitch + yaw without affecting the collider
            cameraGO = new GameObject(AppData.CAMERA_NAME_FIRST_PERSON);
            cameraGO.Transform.SetParent(parentGO.Transform);

            // Local offset from feet → eye height
            cameraGO.Transform.TranslateTo(new Vector3(0, 0, 0));
            camera = cameraGO.AddComponent<Camera>();
            camera.FieldOfView = MathHelper.ToRadians(80.0f);
            var mouseLook = cameraGO.AddComponent<MouseYawPitchController>();
   
            // Add both objects to the scene so their components are updated
            scene.Add(parentGO);
            scene.Add(cameraGO);

            // Make this the active camera
            scene.ActiveCamera = camera;
            #endregion

            #region Curve camera
            cameraGO = new GameObject(AppData.CAMERA_NAME_INTRO_CURVE);
            cameraGO.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(-90), 0, 0));
            camera = cameraGO.AddComponent<Camera>();
            camera.FieldOfView = MathHelper.ToRadians(80);

            var curveController = cameraGO.AddComponent<CurveController>();
            curveController.PositionCurve = BuildCameraPositionCurve(CurveLoopType.Oscillate);
            curveController.TargetCurve = BuildCameraTargetCurve(CurveLoopType.Constant);
            curveController.Duration = 10;
            scene.Add(cameraGO);
            #endregion

            //replace with new SetActiveCamera that searches by string
            scene.SetActiveCamera(AppData.CAMERA_NAME_FIRST_PERSON);
        }

        private AnimationCurve3D BuildCameraPositionCurve(CurveLoopType curveLoopType)
        {
            var curve = new AnimationCurve3D(curveLoopType);

            // start
            curve.AddKey(new Vector3(-20, 10, 40), 0);

            // moving inward, slight rise
            curve.AddKey(new Vector3(-10, 10, 30), 0.25f);

            // closest to origin (single “turn”)
            curve.AddKey(new Vector3(0, 10, 30), 0.5f);

            // heading back out
            curve.AddKey(new Vector3(10, 10, 40), 0.75f);

            // end
            curve.AddKey(new Vector3(20, 10, 40), 1);

            return curve;
        }

        private AnimationCurve3D BuildCameraTargetCurve(CurveLoopType curveLoopType)
        {
            var curve = new AnimationCurve3D(curveLoopType);

            // All points “in or around” origin, y ≈ 5 so we look slightly down from y=10–12.
            curve.AddKey(new Vector3(-5,0,0), 0);
            curve.AddKey(new Vector3(5,0,0), 1);

            return curve;
        }


        /// <summary>
        /// Add parent root at origin to rotate the sky
        /// </summary>
        private void InitializeSkyParent()
        {
            var _skyParent = new GameObject("SkyParent");
            var rot = _skyParent.AddComponent<RotationController>();

            // Turntable spin around local +Y
            rot._rotationAxisNormalized = Vector3.Up;

            // Dramatised fast drift at 2 deg/sec. 
            rot._rotationSpeedInRadiansPerSecond = MathHelper.ToRadians(2f);
            _sceneManager.ActiveScene.Add(_skyParent);
        }

        private void InitializeSkyBox(int scale = 500)
        {
            Scene scene = _sceneManager.ActiveScene;
            GameObject gameObject = null;
            MeshFilter meshFilter = null;
            MeshRenderer meshRenderer = null;

            // Find the sky parent object to attach sky to so sky rotates
            GameObject skyParent = scene.Find((GameObject go) => go.Name.Equals("SkyParent"));

            // back
            gameObject = new GameObject("back");
            gameObject.Transform.ScaleTo(new Vector3(scale, scale, 1));
            gameObject.Transform.TranslateTo(new Vector3(0, 0, -scale / 2));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            gameObject.AddComponent(meshFilter);
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.Material = _matBasicUnlit;
            meshRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_back");
            scene.Add(gameObject);

            //set parent to allow rotation
            gameObject.Transform.SetParent(skyParent.Transform);

            // left
            gameObject = new GameObject("left");
            gameObject.Transform.ScaleTo(new Vector3(scale, scale, 1));
            gameObject.Transform.RotateEulerBy(new Vector3(0, MathHelper.ToRadians(90), 0), true);
            gameObject.Transform.TranslateTo(new Vector3(-scale / 2, 0, 0));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            gameObject.AddComponent(meshFilter);
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.Material = _matBasicUnlit;
            meshRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_left");
            scene.Add(gameObject);

            //set parent to allow rotation
            gameObject.Transform.SetParent(skyParent.Transform);


            // right
            gameObject = new GameObject("right");
            gameObject.Transform.ScaleTo(new Vector3(scale, scale, 1));
            gameObject.Transform.RotateEulerBy(new Vector3(0, MathHelper.ToRadians(-90), 0), true);
            gameObject.Transform.TranslateTo(new Vector3(scale / 2, 0, 0));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            gameObject.AddComponent(meshFilter);
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.Material = _matBasicUnlit;
            meshRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_right");
            scene.Add(gameObject);

            //set parent to allow rotation
            gameObject.Transform.SetParent(skyParent.Transform);

            // front
            gameObject = new GameObject("front");
            gameObject.Transform.ScaleTo(new Vector3(scale, scale, 1));
            gameObject.Transform.RotateEulerBy(new Vector3(0, MathHelper.ToRadians(180), 0), true);
            gameObject.Transform.TranslateTo(new Vector3(0, 0, scale / 2));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            gameObject.AddComponent(meshFilter);
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.Material = _matBasicUnlit;
            meshRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_front");
            scene.Add(gameObject);

            //set parent to allow rotation
            gameObject.Transform.SetParent(skyParent.Transform);

            // sky (top)
            gameObject = new GameObject("sky");
            gameObject.Transform.ScaleTo(new Vector3(scale, scale, 1));
            gameObject.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(90), 0, MathHelper.ToRadians(90)), true);
            gameObject.Transform.TranslateTo(new Vector3(0, scale / 2, 0));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            gameObject.AddComponent(meshFilter);
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.Material = _matBasicUnlit;
            meshRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_sky");
            scene.Add(gameObject);

            //set parent to allow rotation
            gameObject.Transform.SetParent(skyParent.Transform);

        }

        private void InitializeUI()
        {
            InitializeUIReticleRenderer();
        }

        private void InitializeUIReticleRenderer()
        {
            var uiReticleGO = new GameObject("HUD");

            var reticleAtlas = _textureDictionary.Get("Crosshair_21");
            var uiFont = _fontDictionary.Get("mouse_reticle_font");

            // Reticle (cursor): always on top
            var reticle = new UIReticle(reticleAtlas);
            reticle.Origin = reticleAtlas.GetCenter();
            reticle.SourceRectangle = null;
            reticle.Scale = new Vector2(0.1f, 0.1f);
            reticle.RotationSpeedDegPerSec = 55;
            reticle.LayerDepth = UILayer.Cursor;
            uiReticleGO.AddComponent(reticle);

            var textRenderer = uiReticleGO.AddComponent<UIText>();
            textRenderer.Font = uiFont;
            textRenderer.Color = Color.Transparent;

            var picker = uiReticleGO.AddComponent<UIPickerInfo>();
            picker.HitMask = LayerMask.All;
            picker.MaxDistance = 500f;
            picker.HitTriggers = false;

            // Optional custom formatting
            picker.Formatter = hit =>
            {
                var go = hit.Body?.GameObject;
                if (go == null)
                    return string.Empty;

                return $"{go.Name}  d={hit.Distance:F1}";
            };

            _sceneManager.ActiveScene.Add(uiReticleGO);

            // Hide mouse since reticle will take its place
            IsMouseVisible = false;
        }

        protected override void Update(GameTime gameTime)
        {
            if (_pendingSceneName != null)
            {
                string targetScene = _pendingSceneName;
                string leavingScene = _sceneManager.ActiveSceneName;
                _pendingSceneName = null;

                if ((leavingScene == AudioZoneBuilder.SCENE_NAME && targetScene != AudioZoneBuilder.SCENE_NAME) ||
                    (leavingScene == HubSceneBuilder.SCENE_NAME && targetScene != HubSceneBuilder.SCENE_NAME))
                {
                    EngineContext.Instance.Events.Publish(new StopMusicEvent(1f));
                }

                EngineContext.Instance.Events.Publish(new PlaySfxEvent("SFX_UI_Click_Designed_Pop_Movement_Open_1", 1f));
                _sceneManager.SetActiveScene(targetScene);
                RebindMenuToActiveScene();

                if (targetScene == PhysicsZoneBuilder.SCENE_NAME)
                {
                    _physicsZoneBuilder.ResetCrates();
                }

                else if (targetScene == AudioZoneBuilder.SCENE_NAME)
                {
                    _audioZoneBuilder.StartAmbience();
                }

                else if (targetScene == CameraZoneBuilder.SCENE_NAME)
                {
                    _cameraZoneBuilder.ResetCamera();
                }

                else if (targetScene == OrchestrationZoneBuilder.SCENE_NAME)
                {
                    _orchestrationZoneBuilder.ResetRitual();
                }

                else if(targetScene == EventsStateZoneBuilder.SCENE_NAME)
                {
                    _eventsStateZoneBuilder.ResetEventsState();
                }
            }

            //call time update
            #region Core
            Time.Update(gameTime);
            #endregion
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Override Dispose to clean up engine resources.
        /// MonoGame's Game class already implements IDisposable, so we override its Dispose method.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                base.Dispose(disposing);
                return;
            }

            if (disposing)
            {
                System.Diagnostics.Debug.WriteLine("Disposing Main...");

                // 1. Dispose Materials (which may own Effects)
                System.Diagnostics.Debug.WriteLine("Disposing Materials");
                _matBasicUnlit?.Dispose();
                _matBasicUnlit = null;

                _matBasicLit?.Dispose();
                _matBasicLit = null;

                _matAlphaCutout?.Dispose();
                _matAlphaCutout = null;

                // 2. Clear cached MeshFilters in factory registry
                System.Diagnostics.Debug.WriteLine("Clearing MeshFilter Registry");
                MeshFilterFactory.ClearRegistry();

                // 3. Dispose content dictionaries (now they implement IDisposable!)
                System.Diagnostics.Debug.WriteLine("Disposing Content Dictionaries");
                _textureDictionary?.Dispose();
                _textureDictionary = null;

                _modelDictionary?.Dispose();
                _modelDictionary = null;

                _fontDictionary?.Dispose();
                _fontDictionary = null;

                // 4. Dispose EngineContext (which owns SpriteBatch and Content)
                System.Diagnostics.Debug.WriteLine("Disposing EngineContext");
                EngineContext.Instance?.Dispose();

                // 5. Clear references to help GC
                System.Diagnostics.Debug.WriteLine("Clearing References");

                System.Diagnostics.Debug.WriteLine("Main disposal complete");
            }

            _disposed = true;

            // Always call base.Dispose
            base.Dispose(disposing);
        }

        #endregion
    }
}
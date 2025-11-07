using GDEngine.Core;
using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Debug;
using GDEngine.Core.Entities;
using GDEngine.Core.Extensions;
using GDEngine.Core.Factories;
using GDEngine.Core.Input.Data;
using GDEngine.Core.Input.Devices;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.UI;
using GDEngine.Core.Serialization;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using GDEngine.Core.Timing;
using GDGame.Demos;
using GDGame.Demos.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GDGame
{
    public class Main : Game
    {
        #region Core Fields (Common to all games)     
        private GraphicsDeviceManager _graphics;
        private ContentDictionary<Texture2D> _textureDictionary;
        private ContentDictionary<Model> _modelDictionary;
        private ContentDictionary<SpriteFont> _fontDictionary;
        private Scene _scene;
        private Camera _camera;
        private bool _disposed = false;
        #endregion

        #region Demo Fields (remove in your game)
        private AnimationCurve3D _animationPositionCurve, _animationRotationCurve;
        private Material _matBasicUnlit, _matBasicLit, _matAlphaCutout;
        private AnimationCurve _animationCurve;
        private GameObject _cameraGO;
        private UIStatsRenderer _uiStatsRenderer;
        private KeyboardState _prevKeyboard;
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
            InitializeGraphics(ScreenResolution.R1024x768);

            InitializeMouse();

            InitializeContext();

            InitializeAssetDictionaries();

            LoadAssets();

            InitializeEffects();

            InitializeScene();

            InitializeSystems();

            InitializeCamera(new Vector3(0, 5, 25));

            int scale = 500;

            InitializeSkyParent();

            InitializeSkyBox(scale);

            InitializeGround(scale);

            #endregion

            InitializeUIRenderers();

            #region Demos
            // Camera-demos
            InitializeCameraCurves();
            InitializePIPCamera(new Vector3(-35, 5, 5),
                new Viewport(0,
                0,
                400,
                200),
                -1, 0);

            // Level-demos
            DemoTestObject();
            DemoAlphaCutoutFoliage(new Vector3(0, 10 /*note Y=heightscale/2*/, 0), 12, 20);
            DemoLoadFromJSON();
            #endregion

            base.Initialize();
        }

        private void InitializePIPCamera(Vector3 position,
            Viewport viewport, int depth, int index = 0)
        {
            var pipCameraGO = new GameObject("PIP camera");
            pipCameraGO.Transform.TranslateTo(position);
            pipCameraGO.Transform.RotateEulerBy(new Vector3(0, MathHelper.ToRadians(-90), 0));

            //if (index == 0)
            //{
            //    pipCameraGO.AddComponent<KeyboardWASDController>();
            //    pipCameraGO.AddComponent<MouseYawPitchController>();
            //}

            var camera = pipCameraGO.AddComponent<Camera>();
            camera.StackRole = Camera.StackType.Overlay;
            camera.ClearFlags = Camera.ClearFlagsType.DepthOnly;
            camera.Depth = depth; //-100

            camera.Viewport = viewport; // new Viewport(0, 0, 400, 300);

            _scene.Add(pipCameraGO);
        }

        private void InitializeCameraCurves()
        {
            //1D animation curve demo (e.g. scale, audio volume, lerp factor for color, etc)
            _animationCurve = new AnimationCurve(CurveLoopType.Cycle);
            _animationCurve.AddKey(0f, 10);
            _animationCurve.AddKey(2f, 11); //up
            _animationCurve.AddKey(0f, 12); //down
            _animationCurve.AddKey(8f, 13); //up further
            _animationCurve.AddKey(0f, 13.5f); //down

            //3D animation curve demo
            _animationPositionCurve = new AnimationCurve3D(CurveLoopType.Oscillate);
            _animationPositionCurve.AddKey(new Vector3(0, 4, 0), 0);
            _animationPositionCurve.AddKey(new Vector3(5, 8, 2), 1);
            _animationPositionCurve.AddKey(new Vector3(10, 12, 4), 2);
            _animationPositionCurve.AddKey(new Vector3(0, 4, 0), 3);

            // Absolute yaw/pitch/roll angles (radians) over time
            _animationRotationCurve = new AnimationCurve3D(CurveLoopType.Oscillate);
            _animationRotationCurve.AddKey(new Vector3(0, 0, 0), 0);              // yaw, pitch, roll
            _animationRotationCurve.AddKey(new Vector3(0, MathHelper.PiOver2, 0), 1);
            _animationRotationCurve.AddKey(new Vector3(0, MathHelper.Pi, 0), 2);
            _animationRotationCurve.AddKey(new Vector3(0, 0, 0), 3);
        }

        private void InitializeGraphics(Integer2 resolution)
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            ScreenResolution.SetResolution(_graphics, resolution);

            //   System.Diagnostics.Debug.WriteLine(ScreenResolution.R1024x768.Aspect());
        }

        private void InitializeMouse()
        {
            Mouse.SetPosition(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
        }

        private void InitializeContext()
        {
            EngineContext.Initialize(_graphics.GraphicsDevice, Content);
        }

        private void InitializeAssetDictionaries()
        {
            _textureDictionary = new ContentDictionary<Texture2D>();
            _modelDictionary = new ContentDictionary<Model>();
            _fontDictionary = new ContentDictionary<SpriteFont>();

        }

        private void LoadAssets()
        {
            LoadSounds();
            LoadFonts();
            LoadTextures();
            LoadModels();
        }

        private void LoadSounds()
        {
            //TODO - LAB EXERCISE
        }

        private void LoadFonts()
        {
            //TODO - LAB EXERCISE
            _fontDictionary.Add("perfStats", "assets/fonts/perfStats");

            _fontDictionary.Add("mouse_reticule_font", "assets/fonts/testfont");
        }

        private void LoadTextures()
        {
            //ground
            _textureDictionary.Add("ground_grass", "assets/textures/foliage/ground/grass1");

            //skybox
            _textureDictionary.Add("skybox_back", "assets/textures/skybox/back");
            _textureDictionary.Add("skybox_front", "assets/textures/skybox/front");
            _textureDictionary.Add("skybox_left", "assets/textures/skybox/left");
            _textureDictionary.Add("skybox_right", "assets/textures/skybox/right");
            _textureDictionary.Add("skybox_sky", "assets/textures/skybox/sky");

            //crate
            _textureDictionary.Add("crate1", "assets/textures/props/crates/crate1");

            //tree
            _textureDictionary.Add("tree4", "assets/textures/foliage/trees/tree4");

            //test texture for models
            _textureDictionary.Add("checkerboard", "assets/textures/demo/checkerboard");
            _textureDictionary.Add("mona lisa", "assets/textures/demo/mona lisa");

            _textureDictionary.Add("mouse_reticule", "assets/textures/ui/controls/reticuleClosed");

        }

        private void LoadModels()
        {
            _modelDictionary.Add("monkey1", "assets/models/monkey1");
            _modelDictionary.Add("cube", "assets/models/cube");
            _modelDictionary.Add("teapot", "assets/models/teapot");
            _modelDictionary.Add("teapot_lowpoly", "assets/models/teapot_lowpoly");
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
            // (Use LinearWrap if your foliage textures tile.)
            _matAlphaCutout.SamplerState = SamplerState.LinearClamp;

            #endregion

        }

        private void InitializeScene()
        {
            //make a scene
            _scene = new Scene(EngineContext.Instance, "outdoors - level 1");
        }

        private void InitializeSystems()
        {
            InitializeInputSystem();
            InitializeCameraSystem();
            InitializeRenderingSystem();
            InitializeUIRenderingSystem();
        }

        private void InitializeUIRenderingSystem()
        {
            _scene.Add(new UIRenderSystem(100)); // draws in PostRender after RenderingSystem (order = -100)
        }

        private void InitializeCameraSystem()
        {
            var cameraSystem = new CameraSystem(_graphics.GraphicsDevice, -100);
            _scene.Add(cameraSystem);
        }

        private void InitializeRenderingSystem()
        {
            _scene.Add(new RenderingSystem(-100));
        }

        private void InitializeInputSystem()
        {
            //set mouse, keyboard binding keys (e.g. WASD)
            var bindings = InputBindings.Default;
            // optional tuning
            bindings.MouseSensitivity = 0.12f;  // mouse look scale
            bindings.DebounceMs = 60;           // key/mouse debounce in ms
            bindings.EnableKeyRepeat = true;    // hold-to-repeat
            bindings.KeyRepeatMs = 300;         // repeat rate in ms

            // Create the input system 
            var inputSystem = new InputSystem();

            //register all the devices, you dont have to, but its for the demo
            inputSystem.Add(new GDKeyboardInput(bindings));
            inputSystem.Add(new GDMouseInput(bindings));
            inputSystem.Add(new GDGamepadInput(PlayerIndex.One, "Gamepad P1"));

            _scene.Add(inputSystem);
        }

        private void InitializeCamera(Vector3 position)
        {
            //camera GO
            _cameraGO = new GameObject("First person camera");
            //set position 
            _cameraGO.Transform.TranslateTo(position);
            //turn around as Forward is by default (0,0,1)
            _cameraGO.Transform.RotateEulerBy(
                new Vector3(0, MathHelper.ToRadians(180), 0), true);
            //add camera component to the GO
            _camera = _cameraGO.AddComponent<Camera>();
            _camera.FarPlane = 1000;
            ////feed off whatever screen dimensions you set InitializeGraphics
            _camera.AspectRatio = (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight;

            _cameraGO.AddComponent<KeyboardWASDController>();
            _cameraGO.AddComponent<MouseYawPitchController>();

            //finally add it to the scene
            _scene.Add(_cameraGO);
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
            _scene.Add(_skyParent);
        }

        private void InitializeSkyBox(int scale = 500)
        {
            GameObject gameObject = null;
            MeshFilter meshFilter = null;
            MeshRenderer meshRenderer = null;

            // Find the sky parent object to attach sky to so sky rotates
            GameObject skyParent = _scene.Find((GameObject go) => go.Name.Equals("SkyParent"));

            // back
            gameObject = new GameObject("back");
            gameObject.Transform.ScaleTo(new Vector3(scale, scale, 1));
            gameObject.Transform.TranslateTo(new Vector3(0, 0, -scale / 2));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            gameObject.AddComponent(meshFilter);
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.Material = _matBasicUnlit;
            meshRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_back");
            _scene.Add(gameObject);

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
            _scene.Add(gameObject);

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
            _scene.Add(gameObject);

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
            _scene.Add(gameObject);

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
            _scene.Add(gameObject);

            //set parent to allow rotation
            gameObject.Transform.SetParent(skyParent.Transform);

        }

        private void InitializeGround(int scale = 500)
        {
            GameObject gameObject = null;
            MeshFilter meshFilter = null;
            MeshRenderer meshRenderer = null;

            gameObject = new GameObject("ground");
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            gameObject.Transform.ScaleBy(new Vector3(scale, scale, 1));
            gameObject.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(-90), 0, 0), true);

            gameObject.AddComponent(meshFilter);
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.Material = _matBasicUnlit;
            meshRenderer.Overrides.MainTexture = _textureDictionary.Get("ground_grass");

            _scene.Add(gameObject);
        }

        private void InitializeUIRenderers()
        {
            #region Perf Stats
            // Create a GO to host the UI
            var uiGO = new GameObject("Perf Overlay");

            // Attach stats overlay (auto-registers with UIRenderSystem in Awake)
            _uiStatsRenderer = uiGO.AddComponent<UIStatsRenderer>();

            // Set font 
            _uiStatsRenderer.Font = _fontDictionary.Get("perfStats");

            // Optional: add your own debug lines (same pattern you used before)
            _uiStatsRenderer.LinesProvider = () =>
            {
                return new[]
                {
                    "",
                    $"Draw Stats:",
                    $" - Renderer Count: {_scene.Renderers.Count}",
                    "",
                    $"Camera Stats:",
                    $" - Camera [name]: {_scene.ActiveCamera.GameObject.Name}",
                    $" - Camera [Position]: {_cameraGO.Transform.Position.ToFixed()}",
                    $" - Camera [Forward]: {_cameraGO.Transform.Forward.ToFixed()}"
                };
            };

            // Add to scene so Awake runs and it registers itself
            _scene.Add(uiGO);
            #endregion

            // Mouse reticule (distance to objective)
            var mouseUIRet = new GameObject("mouse reticule");

            var uiReticuleRenderer = mouseUIRet.AddComponent<UIReticuleRenderer>();
            uiReticuleRenderer.Texture = _textureDictionary.Get("mouse_reticule");
            uiReticuleRenderer.Font = _fontDictionary.Get("mouse_reticule_font");
            uiReticuleRenderer.Offset = new Vector2(0, 30);

            IsMouseVisible = false;

            _scene.Add(mouseUIRet);

        }

        /// <summary>
        /// Adds a single-part FBX model into the scene.
        /// </summary>
        private void InitializeModel(Vector3 position,
            Vector3 eulerRotationDegrees, Vector3 scale,
            string textureName, string modelName, string objectName)
        {

            var go = new GameObject(objectName);
            go.Transform.TranslateTo(position);
            go.Transform.RotateEulerBy(eulerRotationDegrees * MathHelper.Pi / 180f);
            go.Transform.ScaleTo(scale);

            var model = _modelDictionary.Get(modelName);
            var texture = _textureDictionary.Get(textureName);
            var meshFilter = MeshFilterFactory.CreateFromModel(model, _graphics.GraphicsDevice, 0, 0);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();

            meshRenderer.Material = _matBasicLit;
            meshRenderer.Overrides.MainTexture = texture;

            _scene.Add(go);
        }
        protected override void Update(GameTime gameTime)
        {
            //call time update
            Time.Update(gameTime);

            //update Scene
            _scene.Update(Time.DeltaTimeSecs);

            ToggleStatsWindow();

            base.Update(gameTime);
        }

        private void ToggleStatsWindow()
        {
            var kb = Keyboard.GetState();
            if (_uiStatsRenderer != null)
            {
                if (kb.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F1) && !_prevKeyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F1))
                    _uiStatsRenderer.Enabled = !_uiStatsRenderer.Enabled;
            }

            //if(_scene != null)
            //{
            //    if (kb.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F2) && !_prevKeyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F2))
            //    {
            //        _scene.Clear();
            //    }

            //    if (kb.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F3) && !_prevKeyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F3))
            //    {
            //        //re-load your JSON again
            //        foreach (var d in JSONSerializationUtility.LoadData<ModelSpawnData>(Content, "multi_model_spawn.json"))
            //                InitializeModel(d.Position, d.RotationDegrees, d.Scale, d.TextureName, d.ModelName, d.ObjectName);
            //    }
            //}

            _prevKeyboard = kb;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            //just as called update, we now have to call draw to call the draw in the renderingsystem
            _scene.Draw(Time.DeltaTimeSecs);

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

                // 1. Dispose Scene (which will cascade to GameObjects and Components)
                System.Diagnostics.Debug.WriteLine("Disposing Scene");
                _scene?.Dispose();
                _scene = null;

                // 2. Dispose Materials (which may own Effects)
                System.Diagnostics.Debug.WriteLine("Disposing Materials");
                _matBasicUnlit?.Dispose();
                _matBasicUnlit = null;

                _matBasicLit?.Dispose();
                _matBasicLit = null;

                _matAlphaCutout?.Dispose();
                _matAlphaCutout = null;

                // 3. Clear cached MeshFilters in factory registry
                System.Diagnostics.Debug.WriteLine("Clearing MeshFilter Registry");
                MeshFilterFactory.ClearRegistry();

                // 4. Dispose content dictionaries (now they implement IDisposable!)
                System.Diagnostics.Debug.WriteLine("Disposing Content Dictionaries");
                _textureDictionary?.Dispose();
                _textureDictionary = null;

                _modelDictionary?.Dispose();
                _modelDictionary = null;

                _fontDictionary?.Dispose();
                _fontDictionary = null;

                // 5. Dispose EngineContext (which owns SpriteBatch and Content)
                System.Diagnostics.Debug.WriteLine("Disposing EngineContext");
                EngineContext.Instance?.Dispose();

                // 6. Clear references to help GC
                System.Diagnostics.Debug.WriteLine("Clearing References");
                _animationCurve = null;
                _animationPositionCurve = null;
                _animationRotationCurve = null;

                System.Diagnostics.Debug.WriteLine("Main disposal complete");
            }

            _disposed = true;

            // Always call base.Dispose
            base.Dispose(disposing);
        }

        #endregion    }

        #region Demo Methods (remove in your game)
        private void DemoLoadFromJSON()
        {
            List<ModelSpawnData> mList = JSONSerializationUtility.LoadData<ModelSpawnData>(Content, "single_model_spawn.json");
           
            //load a single model
            foreach (var d in mList)
                InitializeModel(d.Position, d.RotationDegrees, d.Scale, d.TextureName, d.ModelName, d.ObjectName);

            //load multiple models
            foreach (var d in JSONSerializationUtility.LoadData<ModelSpawnData>(Content, "multi_model_spawn.json"))
                InitializeModel(d.Position, d.RotationDegrees, d.Scale, d.TextureName, d.ModelName, d.ObjectName);
        }

        private void DemoTestObject()
        {
            GameObject gameObject = null;
            MeshFilter meshFilter = null;
            MeshRenderer meshRenderer = null;

            gameObject = new GameObject("test crate textured cube");

            gameObject.Transform.TranslateTo(new Vector3(0, 5, 0));
            gameObject.Transform.ScaleTo(Vector3.One * 8);

            meshFilter = MeshFilterFactory.CreateCubeTexturedLit(_graphics.GraphicsDevice);
            gameObject.AddComponent(meshFilter);

            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.Material = _matBasicLit; //enable lighting for the crate
            meshRenderer.Overrides.MainTexture = _textureDictionary.Get("crate1");

            _scene.Add(gameObject);

            #region Demo - Curve and Input
            var posRotController = new PositionRotationController
            {
                RotationCurve = _animationRotationCurve,
                PositionCurve = _animationPositionCurve
            };
            gameObject.AddComponent(posRotController);

            //demo the new input system support for keyboard, mouse and gamepad
            gameObject.AddComponent(new InputReceiverComponent());
            
            #endregion

            //  testCrateGO.Layer = LayerMask.World;
        }

        private void DemoAlphaCutoutFoliage(Vector3 position, float width, float height)
        {
            var go = new GameObject("tree");

            // A unit quad facing +Z (your factory already supplies lit quad with UVs)
            var mf = MeshFilterFactory.CreateQuadTexturedLit(GraphicsDevice);
            go.AddComponent(mf);

            var treeRenderer = go.AddComponent<MeshRenderer>();
            treeRenderer.Material = _matAlphaCutout;

            // Per-object properties via the overrides block
            treeRenderer.Overrides.MainTexture = _textureDictionary.Get("tree4");

            // AlphaTest: pixels with alpha below ReferenceAlpha are discarded (0–255).
            // 128–160 is a good starting range for foliage; tweak to taste.
            treeRenderer.Overrides.SetInt("ReferenceAlpha", 128);
            treeRenderer.Overrides.Alpha = 1f; // overall alpha multiplier (kept at 1 for cutout)

            // Scale the quad so it looks like a tree (aspect from your PNG)
            go.Transform.ScaleTo(new Vector3(width, height, 1f));

            go.Transform.TranslateTo(position);

            _scene.Add(go);
        }
        #endregion

    }
}
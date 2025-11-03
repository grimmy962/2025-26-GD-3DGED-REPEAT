using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Components.Controllers.General.Movement;
using GDEngine.Core.Entities;
using GDEngine.Core.Factories;
using GDEngine.Core.Input.Data;
using GDEngine.Core.Input.Devices;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using GDEngine.Core.Timing;
using GDEngine.Samples;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Windows.Forms;

namespace GDGame
{
    public class Main : Game
    {
        #region Core Fields
        private GraphicsDeviceManager _graphics;
        private ContentDictionary<Texture2D> _textureDictionary;
        private ContentDictionary<Model> _modelDictionary;
        private ContentDictionary<SpriteFont> _fontDictionary;
        private Scene _scene;
        private Camera _camera;
        private bool _disposed = false;
        #endregion

        #region Demo Fields (remove in your game)
        private GameObject _cameraGO;
        private GameObject _grassQuadGO;
        private MeshRenderer _grassQuadRenderer;
        private MeshRenderer _skyBoxBackRenderer, _skyBoxLeftRenderer, _skyBoxRightRenderer, _skyBoxFrontRenderer, _skyBoxSkyRenderer;
        private AnimationCurve _animationCurve;
        private MeshRenderer _testObjRenderer;
        private AnimationCurve3D _animationPositionCurve;
        private AnimationCurve3D _animationRotationCurve;
        private Material _matBasicUnlit;
        private Material _matBasicLit;
        private Material _matAlphaCutout;
        private GameObject _pipCameraGO;
        #endregion

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            InitializeGraphics(1920, 1080);

            InitializeMouse();

            InitializeContext();

            InitializeAssetDictionaries();

            LoadAssets();

            InitializeEffects();

            InitializeScene();

            InitializeSystems();

            InitializeCameraCurves();

            InitializeCamera(new Vector3(0, 5, 25));

            int scale = 500;

            InitializeSkyBox(scale);

            InitializeGround(scale);

            InitializeTestObject();

            InitializeFoliage(new Vector3(0, 10 /*note Y=heightscale/2*/, 0), 12, 20);

            base.Initialize();
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

        private void InitializeGraphics(int width, int height)
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.ApplyChanges();
        }

        private void InitializeMouse()
        {
            //TODO
            Mouse.SetPosition(_graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2);
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
            InitializeCameraSystem();
            InitializeRenderingSystem();
            InitializeInputSystem();
        }

        private void InitializeCameraSystem()
        {
            var cameraSystem = new CameraSystem(_graphics.GraphicsDevice, -100);
            _scene.Add(cameraSystem);
        }

        private void InitializeRenderingSystem()
        {
            _scene.Add(new RenderingSystem());
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

            //dont forget to add the camera to the camera system in the scene
            _scene.GetSystem<CameraSystem>().Add(_camera);
           
            //finally add it to the scene
            _scene.Add(_cameraGO);
        }

      
        private void InitializeSkyBox(int scale = 500)
        {
            #region Add parent root at origin to rotate the sky
            //add a parent at origin so we can rotate the sky around Y-axis
            var skyboxRoot = new GameObject("SkyboxRoot");
            var rot = skyboxRoot.AddComponent<GDEngine.Core.Components.Controllers.General.Transform.RotationController>();

            // Turntable spin around local +Y
            rot._rotationAxisNormalized = Vector3.Up;

            // Dramatised fast drift at 2 deg/sec. 
            rot._rotationSpeedInRadiansPerSecond = MathHelper.ToRadians(2f);

            _scene.Add(skyboxRoot); 
            #endregion

            GameObject skyBoxQuad = null;
            MeshFilter meshFilter = null;

            // back
            skyBoxQuad = new GameObject("back");
            skyBoxQuad.Transform.ScaleTo(new Vector3(scale, scale, 1));
            skyBoxQuad.Transform.TranslateTo(new Vector3(0, 0, -scale / 2));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            skyBoxQuad.AddComponent(meshFilter);
            _skyBoxBackRenderer = skyBoxQuad.AddComponent<MeshRenderer>();
            _skyBoxBackRenderer.Material = _matBasicUnlit;
            _skyBoxBackRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_back");
            _scene.Add(skyBoxQuad);

            //set parent to allow rotation
            skyBoxQuad.Transform.SetParent(skyboxRoot.Transform);

            // left
            skyBoxQuad = new GameObject("left");
            skyBoxQuad.Transform.ScaleTo(new Vector3(scale, scale, 1));
            skyBoxQuad.Transform.RotateEulerBy(new Vector3(0, MathHelper.ToRadians(90), 0), true);
            skyBoxQuad.Transform.TranslateTo(new Vector3(-scale / 2, 0, 0));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            skyBoxQuad.AddComponent(meshFilter);
            _skyBoxLeftRenderer = skyBoxQuad.AddComponent<MeshRenderer>();
            _skyBoxLeftRenderer.Material = _matBasicUnlit;
            _skyBoxLeftRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_left");
            _scene.Add(skyBoxQuad);

            //set parent to allow rotation
            skyBoxQuad.Transform.SetParent(skyboxRoot.Transform);


            // right
            skyBoxQuad = new GameObject("right");
            skyBoxQuad.Transform.ScaleTo(new Vector3(scale, scale, 1));
            skyBoxQuad.Transform.RotateEulerBy(new Vector3(0, MathHelper.ToRadians(-90), 0), true);
            skyBoxQuad.Transform.TranslateTo(new Vector3(scale / 2, 0, 0));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            skyBoxQuad.AddComponent(meshFilter);
            _skyBoxRightRenderer = skyBoxQuad.AddComponent<MeshRenderer>();
            _skyBoxRightRenderer.Material = _matBasicUnlit;
            _skyBoxRightRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_right");
            _scene.Add(skyBoxQuad);

            //set parent to allow rotation
            skyBoxQuad.Transform.SetParent(skyboxRoot.Transform);

            // front
            skyBoxQuad = new GameObject("front");
            skyBoxQuad.Transform.ScaleTo(new Vector3(scale, scale, 1));
            skyBoxQuad.Transform.RotateEulerBy(new Vector3(0, MathHelper.ToRadians(180), 0), true);
            skyBoxQuad.Transform.TranslateTo(new Vector3(0, 0, scale / 2));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            skyBoxQuad.AddComponent(meshFilter);
            _skyBoxFrontRenderer = skyBoxQuad.AddComponent<MeshRenderer>();
            _skyBoxFrontRenderer.Material = _matBasicUnlit;
            _skyBoxFrontRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_front");
            _scene.Add(skyBoxQuad);

            //set parent to allow rotation
            skyBoxQuad.Transform.SetParent(skyboxRoot.Transform);

            // sky (top)
            skyBoxQuad = new GameObject("sky");
            skyBoxQuad.Transform.ScaleTo(new Vector3(scale, scale, 1));
            skyBoxQuad.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(90), 0, MathHelper.ToRadians(90)), true);
            skyBoxQuad.Transform.TranslateTo(new Vector3(0, scale / 2, 0));
            meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            skyBoxQuad.AddComponent(meshFilter);
            _skyBoxSkyRenderer = skyBoxQuad.AddComponent<MeshRenderer>();
            _skyBoxSkyRenderer.Material = _matBasicUnlit;
            _skyBoxSkyRenderer.Overrides.MainTexture = _textureDictionary.Get("skybox_sky");
            _scene.Add(skyBoxQuad);

            //set parent to allow rotation
            skyBoxQuad.Transform.SetParent(skyboxRoot.Transform);

        }


        private void InitializeGround(int scale = 500)
        {
            _grassQuadGO = new GameObject("ground");
            var meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            _grassQuadGO.Transform.ScaleBy(new Vector3(scale, scale, 1));
            _grassQuadGO.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(-90), 0, 0), true);

            _grassQuadGO.AddComponent(meshFilter);
            _grassQuadRenderer = _grassQuadGO.AddComponent<MeshRenderer>();
            _grassQuadRenderer.Material = _matBasicUnlit;
            _grassQuadRenderer.Overrides.MainTexture = _textureDictionary.Get("ground_grass");

            _scene.Add(_grassQuadGO);
        }


        private void InitializeTestObject()
        {
            var testCrateGO = new GameObject("test crate textured cube");

            testCrateGO.Transform.TranslateTo(new Vector3(0, 5, 0));
            testCrateGO.Transform.ScaleTo(Vector3.One * 8);

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(_graphics.GraphicsDevice);
            testCrateGO.AddComponent(meshFilter);

            _testObjRenderer = testCrateGO.AddComponent<MeshRenderer>();
            _testObjRenderer.Material = _matBasicLit; //enable lighting for the crate
            _testObjRenderer.Overrides.MainTexture = _textureDictionary.Get("crate1");

            _scene.Add(testCrateGO);

            var posRotController = new PositionRotationController
            {
                RotationCurve = _animationRotationCurve,
                PositionCurve = _animationPositionCurve
            };
            testCrateGO.AddComponent(posRotController);

            //demo the new input system support for keyboard, mouse and gamepad
            testCrateGO.AddComponent(new DemoInputReceiverComponent());
        }

        private void InitializeFoliage(Vector3 position, float width, float height)
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

        protected override void Update(GameTime gameTime)
        {
            //call time update
            Time.Update(gameTime);

            //update Scene
            _scene.Update(Time.DeltaTimeSecs);

            base.Update(gameTime);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //just as called update, we now have to call draw to call the draw in the renderingsystem
            _scene.Draw(Time.DeltaTimeSecs);

            //notice that we have to manually call render on each primitive - we really need a RenderSystem!
            //_grassQuadRenderer.Render(_graphics.GraphicsDevice, _camera);

            //_skyBoxBackRenderer.Render(_graphics.GraphicsDevice, _camera);
            //_skyBoxLeftRenderer.Render(_graphics.GraphicsDevice, _camera);
            //_skyBoxRightRenderer.Render(_graphics.GraphicsDevice, _camera);
            //_skyBoxFrontRenderer.Render(_graphics.GraphicsDevice, _camera);
            //_skyBoxSkyRenderer.Render(_graphics.GraphicsDevice, _camera);

            //_testObjRenderer.Render(_graphics.GraphicsDevice, _camera);

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
                _cameraGO = null;
                _camera = null;
                _grassQuadGO = null;
                _grassQuadRenderer = null;
                _skyBoxBackRenderer = null;
                _skyBoxLeftRenderer = null;
                _skyBoxRightRenderer = null;
                _skyBoxFrontRenderer = null;
                _skyBoxSkyRenderer = null;
                _testObjRenderer = null;
                _animationCurve = null;
                _animationPositionCurve = null;
                _animationRotationCurve = null;

                System.Diagnostics.Debug.WriteLine("Main disposal complete");
            }

            _disposed = true;

            // Always call base.Dispose
            base.Dispose(disposing);
        }
    }
}

using GDEngine.Core;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Factories;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Windows.Forms;

namespace GDGame
{
    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;
        private Scene _scene;
        private GameObject _cameraGO;
        private Camera _camera;
        private GameObject _grassQuadGO;
        private MeshRenderer _grassQuadRenderer;
        private ContentDictionary<Texture2D> _textureDictionary;
        private ContentDictionary<Model> _modelDictionary;
        private ContentDictionary<SpriteFont> _fontDictionary;
        private MeshRenderer _skyBoxBackRenderer;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            InitializeGraphics(1280, 720);

            InitializeMouse();

            InitializeContext();

            InitializeAssetDictionaries();

            LoadAssets();

            InitializeEffects();

            InitializeScene();

            InitializeCamera(new Vector3(0, 10, 5));

            InitializeSkyBox();

            InitializeGround();

            base.Initialize();
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
        }

        private void InitializeEffects()
        {
           //TODO
        }

        private void InitializeScene()
        {
            //make a scene
            _scene = new Scene(EngineContext.Instance, "outdoors - level 1");
        }

        private void InitializeCamera(Vector3 position)
        {
            //camera GO
            _cameraGO = new GameObject("First person camera");
            //set position 
            _cameraGO.Transform.TranslateTo(new Vector3(0, 5, 150));
            //turn around as Forward is by default (0,0,1)
            _cameraGO.Transform.RotateEuler(
                new Vector3(0, MathHelper.ToRadians(135), 0), true);
            //add camera component to the GO
            _camera = _cameraGO.AddComponent<Camera>();
            ////feed off whatever screen dimensions you set InitializeGraphics
            _camera.AspectRatio = (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight;
            //add to scene
            _scene.AddGameObject(_cameraGO);

            //decide on controller
            _cameraGO.AddComponent<CameraController>();
           
        }

        private void InitializeSkyBox()
        {
            GameObject skyBoxQuad = null;
            skyBoxQuad = new GameObject("back wall");
            skyBoxQuad.Transform.ScaleTo(new Vector3(500, 500, 1));
            var meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            skyBoxQuad.AddComponent(meshFilter); //setting VB and IB data
            _skyBoxBackRenderer = skyBoxQuad.AddComponent<MeshRenderer>();
            _skyBoxBackRenderer._texture = _textureDictionary.Get("skybox_back");
            _scene.AddGameObject(skyBoxQuad);
        }

        private void InitializeGround()
        {
            _grassQuadGO = new GameObject("ground");
            var meshFilter = MeshFilterFactory.CreateQuadTexturedLit(_graphics.GraphicsDevice);
            _grassQuadGO.Transform.ScaleBy(new Vector3(500, 500, 1));
            _grassQuadGO.Transform.RotateEuler(new Vector3(
                MathHelper.ToRadians(-90), 0, 0), true);

            _grassQuadGO.AddComponent(meshFilter);
            _grassQuadRenderer = _grassQuadGO.AddComponent<MeshRenderer>();
            //give the renderer the texture that it will put on the quad
            _grassQuadRenderer._texture = _textureDictionary.Get("ground_grass");
            _scene.AddGameObject(_grassQuadGO);

        }
        protected override void Update(GameTime gameTime)
        {
            //call time update
            Time.Update(gameTime);

            // stop (0), run normall (1), slow (<1) and speed time(>1)
            //Time.TimeScale = 4f;

            //update Scene
            _scene.Update(Time.DeltaTime);

            System.Diagnostics.Debug.WriteLine(_cameraGO.Transform.Position);

            
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            //notice that we have to manually call render on each primitive - we really need a RenderSystem!
            _grassQuadRenderer.Render(_graphics.GraphicsDevice, _camera);

            _skyBoxBackRenderer.Render(_graphics.GraphicsDevice, _camera);
            
            
            base.Draw(gameTime);
        }
    }
}

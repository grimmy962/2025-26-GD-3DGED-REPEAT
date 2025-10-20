using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Factories;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private GameObject _primitiveGO;
        private MeshRenderer _primitiveGORenderer;
        private GameObject _grassQuadGO;
        private MeshRenderer _grassQuadRenderer;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            InitializeGraphics(1920, 1080);

            InitializeContext();

            InitializeScene();

            InitializeCamera(new Vector3(0, 0, 5));

            InitializeSkyBox();

            InitializeGround();

            base.Initialize();
        }

     

        private void InitializeGraphics(int width, int height)
        {
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.ApplyChanges();
        }

        private void InitializeContext()
        {
            //initialize context
            EngineContext.Initialize(_graphics.GraphicsDevice, Content);

        }

        private void InitializeScene()
        {
            //make a scene
            _scene = new Scene(EngineContext.Instance, "Dungeon antechamber");

        }

        private void InitializeCamera(Vector3 position)
        {
            #region Camera
            //need a camera
            _cameraGO = new GameObject("First person camera");
            //add camera component to the GO
            _camera = _cameraGO.AddComponent<Camera>();
            //set position
            _cameraGO.Transform.TranslateBy(position);
            //feed off whatever screen dimensions you set in lines 31-32
            _camera.AspectRatio = (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight;
            //add to scene
            _scene.AddGameObject(_cameraGO);
            //rotate to face the origin so we can see the quad!
            _cameraGO.Transform.RotateEuler(new Vector3(0, MathHelper.ToRadians(180), 0));
            #endregion
        }

        private void InitializeSkyBox()
        {
         //   throw new NotImplementedException();
        }

        private void InitializeGround()
        {
            _grassQuadGO = new GameObject("ground");
            var meshFilter = MeshFilterFactory.CreateQuadColored(_graphics.GraphicsDevice);
            _grassQuadGO.Transform.RotateEuler(
                new Vector3(MathHelper.ToRadians(-90), 0, 0));
            _grassQuadGO.Transform.ScaleBy(new Vector3(25, 1, 25));
           // _grassQuadGO.Transform.TranslateBy(new Vector3(0, 0, 0));
            _grassQuadGO.AddComponent(meshFilter);
            _grassQuadRenderer = _grassQuadGO.AddComponent<MeshRenderer>();
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

            
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //notice that we have to manually call render on each primitive - we really need a RenderSystem!
            _grassQuadRenderer.Render(_graphics.GraphicsDevice, _camera);
            base.Draw(gameTime);
        }
    }
}

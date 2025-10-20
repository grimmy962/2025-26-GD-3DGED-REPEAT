using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Factories;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();

            //initialize context
            EngineContext.Initialize(_graphics.GraphicsDevice, Content);

            //make a scene
            _scene = new Scene(EngineContext.Instance, "Dungeon antechamber");

            #region Camera
            //need a camera
            _cameraGO = new GameObject("First person camera");
            //add camera component to the GO
            _camera = _cameraGO.AddComponent<Camera>();
            //set position
            _cameraGO.Transform.TranslateBy(new Vector3(0, 0, 5));
            //feed off whatever screen dimensions you set in lines 31-32
            _camera.AspectRatio = (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight;
            //add to scene
            _scene.AddGameObject(_cameraGO);
            //rotate to face the origin so we can see the quad!
            _cameraGO.Transform.RotateEuler(new Vector3(0, MathHelper.ToRadians(180), 0));
            #endregion

            #region Demo - Primitive 
            //make a game object with filter (data) and renderer (draw behaviour)
            _primitiveGO = new GameObject("my first primitive");
            //generate the data for a quad (normally you might load an FBX)
            var meshFilter = MeshFilterFactory.CreateWireBox(_graphics.GraphicsDevice);
            //add mesh filter (i.e. the verts and indices data)
            _primitiveGO.AddComponent(meshFilter);
            //add a renderer to draw the data
            _primitiveGORenderer = _primitiveGO.AddComponent<MeshRenderer>();

            #region Demo - Primitive - Controller(s) 

            var transController = _primitiveGO.AddComponent<SineTranslationController>();
            transController._maxDistance = 2;
            transController._angularSpeed = 1f;
            transController._direction = new Vector3(1, 0, 0);

            // RotationController rotController = null;
            RotationController rotController = null;
            rotController = _primitiveGO.AddComponent<RotationController>();
            rotController._rotationAxisNormalized = Vector3.UnitY;   // Y axis
            rotController._rotationSpeedInRadiansPerSecond = MathHelper.ToRadians(90);


            //if we want to see front and back of the quad then lets set the cull mode (remember this relates to the wind order (i.e. CW, CCW) of our indices
            //var rsState = new RasterizerState()
            //{
            //    CullMode = CullMode.None
            //};
            //_graphics.GraphicsDevice.RasterizerState = rsState;
            #endregion
            #endregion
            //add to scene
            _scene.AddGameObject(_primitiveGO);


            base.Initialize();
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
            _primitiveGORenderer.Render(_graphics.GraphicsDevice, _camera);

            base.Draw(gameTime);
        }
    }
}

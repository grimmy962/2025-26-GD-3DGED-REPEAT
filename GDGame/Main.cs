using GDEngine.Core;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Rendering.Factories;
using GDEngine.Core.Services;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame
{
    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;
        private Scene _scene;
        private GameObject _cameraGO;
        private Camera _camera;
        private GameObject _primitiveCQO;
        private MeshRenderer _primitiveCQORenderer;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            //initialize context
            EngineContext.Initialize(_graphics.GraphicsDevice, Content);

            //common vars that lots of entities access
            var context = EngineContext.Instance;

            //make a scene
            _scene = new Scene(context, "Dungeon antechamber");

            //need a camera
            _cameraGO = new GameObject("First person camera");
            _camera = _cameraGO.AddComponent<Camera>();
            _cameraGO.Transform.Position = new Vector3(0, 0, 3);
            _scene.AddGameObject(_cameraGO);

            //give me something to admire
            var meshFilter =
            MeshFilterFactory.CreateQuadColored(_graphics.GraphicsDevice);

            _primitiveCQO = new GameObject("Colored quad");
            _primitiveCQO.AddComponent(meshFilter);
            _primitiveCQORenderer = _primitiveCQO.AddComponent<MeshRenderer>();
            _scene.AddGameObject(_primitiveCQO);


            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            //Step 1 - call time update
            Time.Update(gameTime);


            //TODO - Wk5 - Update Scene

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}

using GDEngine.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame
{
    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;

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

            //initialize systems (input, rendering, UI, sound, physics)

            //initialize dictionaries (ContentDictionary)

            //initialize all primitives (vertices, FBX)

            //instantiate our game objects (camera, player)

            //add game objects to scene

            //add scene to scenemanager

            //start scene (setting camera, set spawn point)

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            //TODO - Wk5 - Update Time

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

using GDEngine.Core.Entities;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;

namespace GDEngine.Core.Managers
{
    public class SceneManager : DrawableGameComponent
    {
        public Dictionary<string, Scene> scenes = new();
        public string activeSceneName;
        private Scene? activeScene;
        public SceneManager(Game game) : base(game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (activeSceneName.Length == 0)
                throw new ArgumentException("Must set scene name");

            scenes.TryGetValue(activeSceneName, out activeScene);

            //update Scene
            activeScene?.Update(Time.DeltaTimeSecs);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //just as called update, we now have to call draw to call the draw in the renderingsystem
            activeScene?.Draw(Time.DeltaTimeSecs);

            base.Draw(gameTime);
        }
    }
}

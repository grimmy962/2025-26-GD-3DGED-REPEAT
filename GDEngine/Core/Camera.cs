using Microsoft.Xna.Framework;

namespace GDEngine.Core
{
    public class Camera : Component
    {
        protected override void Awake()
        {
            if(!HasStarted)
            {

            }
            base.Awake();
        }

        protected override void Update(GameTime gameTime)
        {
            if(HasStarted && Enabled)
            {

            }
            base.Update(gameTime);
        }

    }
}

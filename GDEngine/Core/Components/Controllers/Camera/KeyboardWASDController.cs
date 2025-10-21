using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDEngine.Core.Components
{
    public class KeyboardWASDController : Component
    {
        private KeyboardState _newKBState;

        protected override void Update(float deltaTime)
        {
            //get kb state
            _newKBState = Keyboard.GetState();
            Vector3 dir = Vector3.Zero;
            //read some keys
            if (_newKBState.IsKeyDown(Keys.W))
                dir += Transform.Forward;
            else if (_newKBState.IsKeyDown(Keys.S))
                dir -= Transform.Forward;

            if (_newKBState.IsKeyDown(Keys.A))
                dir += Transform.Right;
            else if (_newKBState.IsKeyDown(Keys.D))
                dir -= Transform.Right;
            //test if not zero
            if (dir.LengthSquared() > 0)
                HandleMove(dir);
        }
        
        private void HandleMove(Vector3 direction)
        {
            //make direction length = 1
            direction.Normalize();

            //scale by elapsed time (and speed later)
            direction *= Time.DeltaTime * 20;

            //apply delta vector
            Transform.TranslateBy(direction, true);
        }

        //TODO - support speed, boost key, key re-mapping
    }
}

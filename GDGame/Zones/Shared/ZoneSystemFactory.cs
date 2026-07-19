using GDEngine.Core.Collections;
using GDEngine.Core.Entities;
using GDEngine.Core.Input.Data;
using GDEngine.Core.Input.Devices;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.Zones.Shared
{
    public static class ZoneSystemFactory
    {
        private const int RENDER_ORDER = -100;
        private const float DEFAULT_MOUSE_SENSITIVITY = 0.12F;
        private const int DEFAULT_DEBOUNCE_MS = 60;
        private const int DEFAULT_KEY_REPEAT_MS = 300;

        public static void AddCoreSystems(
            Scene scene,
            GraphicsDeviceManager graphics,
            ContentDictionary<SoundEffect> sounds,
            bool includePhysics,
            Vector3 gravity
            )

        {
            scene.AddSystem(new CameraSystem(graphics.GraphicsDevice, RENDER_ORDER));
            scene.AddSystem(new RenderSystem(RENDER_ORDER));
            scene.AddSystem(new UIRenderSystem(RENDER_ORDER));

            scene.Add(BuildInputSystem());

            scene.Add(new EventSystem(EngineContext.Instance.Events));
            scene.Add(new AudioSystem(sounds));
            scene.Add(new ImpulseSystem(EngineContext.Instance.Impulses));
            scene.AddSystem(new UIEventSystem());

            if (includePhysics)
            {
                var physicsSystem = scene.AddSystem(new PhysicsSystem());
                physicsSystem.Gravity = gravity;
            }
        }

        private static InputSystem BuildInputSystem()
        {

            var bindings = InputBindings.Default;
            bindings.MouseSensitivity = DEFAULT_MOUSE_SENSITIVITY;
            bindings.DebounceMs = DEFAULT_DEBOUNCE_MS;
            bindings.EnableKeyRepeat = true;
            bindings.KeyRepeatMs = DEFAULT_KEY_REPEAT_MS;

            var inputSystem = new InputSystem();
            inputSystem.Add(new GDKeyboardInput(bindings));
            inputSystem.Add(new GDMouseInput(bindings));
            inputSystem.Add(new GDGamepadInput(PlayerIndex.One, "Gamepad P1"));

            return inputSystem;
        }
    }
}

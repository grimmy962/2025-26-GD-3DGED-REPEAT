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
    //adds the same set of systems to a scene
    //every zone needs the same baseline(render, camera, UI, input, events, audio, physics),
    //so instead of copying this block into eveery zone builder, i just call this one method
    public static class ZoneSystemFactory
    {
        #region Constants

        private const int RENDER_ORDER = -100;
        private const float DEFAULT_MOUSE_SENSITIVITY = 0.12F;
        private const int DEFAULT_DEBOUNCE_MS = 60;
        private const int DEFAULT_KEY_REPEAT_MS = 300;

        #endregion

        #region Methods

        //physics is optional since not every zone needs rigid bodies, but
        //any zone with s portal/trigger still needs it to detect them
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

        //same keyboard/mouse/gamepad setup every zone uses
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

        #endregion
    }
}

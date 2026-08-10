using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Events;
using GDEngine.Core.Factories;
using GDEngine.Core.Gameplay;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using GDEngine.Core.Timing;
using GDGame.Zones.Shared;
using Microsoft.Xna.Framework;

namespace GDGame.Zones
{
    public sealed class SwitchActivatedEvent
    {
        public string SwitchName { get; }
        public SwitchActivatedEvent(string switchName)
        {
            SwitchName = switchName;
        }
    }

    public sealed class AlarmTriggeredEvent
    {
        public string ZoneName { get; }
        public AlarmTriggeredEvent(string zoneName)
        {
            ZoneName = zoneName;
        }
    }

    public sealed class EventsStateZoneBuilder : IZoneBuilder
    {
        public const string SCENE_NAME = "EventsStateZone";
        private const int GROUND_SCALE = 100;

        public string SceneName => SCENE_NAME;

        private GameStateSystem? _gameStateSystem;
        private bool[]? _switchActivated;
        private string[]? _statusText;

        public Scene Build(ZoneBuildContext buildContext)
        {
            var scene = new Scene(buildContext.EngineContext, SCENE_NAME);

            ZoneSystemFactory.AddCoreSystems(
                scene,
                buildContext.Graphics,
                buildContext.Sounds,
                includePhysics: true,
                gravity: AppData.GRAVITY);

            var gameStateSystem = scene.AddSystem(new GameStateSystem());
            _gameStateSystem = gameStateSystem;

            BuildGround(scene, buildContext);
            ZonePlayerFactory.Create(scene, new Vector3(0f, 1.5f, 0f));

            var statusText = BuildStatusText(scene, buildContext);
            _statusText = statusText;

            var switchActivated = new bool[] { false };
            _switchActivated = switchActivated;

            RegisterEventSubscription(statusText, switchActivated, gameStateSystem);

            BuildSwitchTrigger(scene, buildContext);
            BuildAlarmTrigger(scene, buildContext);

            return scene;
        }

        private static void BuildGround(Scene scene, ZoneBuildContext buildContext)
        {
            var ground = new GameObject("ground");
            var meshFilter = MeshFilterFactory.CreateQuadGridTexturedUnlit(
                buildContext.Graphics.GraphicsDevice, 1, 1, 1, 1, 20, 20);

            ground.Transform.ScaleBy(new Vector3(GROUND_SCALE, GROUND_SCALE, 1));
            ground.Transform.RotateEulerBy(new Vector3(MathHelper.ToRadians(-90), 0, 0), true);
            ground.Transform.TranslateTo(new Vector3(0, -0.5f, 0));

            ground.AddComponent(meshFilter);
            var renderer = ground.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicUnlitGround;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("ground_grass");

            var collider = ground.AddComponent<BoxCollider>();
            collider.Size = new Vector3(GROUND_SCALE, GROUND_SCALE, 0.025f);
            collider.Center = new Vector3(0, 0, -0.0125f);

            var rigidBody = ground.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;
            ground.IsStatic = true;
            ground.Layer = LayerMask.Ground;

            scene.Add(ground);
        }

        private static string[] BuildStatusText(Scene scene, ZoneBuildContext buildContext)
        {
            var textHolder = new string[] { "Find the switch to activate the goal." };

            var statusGO = new GameObject("Events Status Text");
            var uiText = statusGO.AddComponent<UIText>();
            uiText.Font = buildContext.Fonts.Get("perf_stats_font");
            uiText.TextProvider = () => textHolder[0];
            uiText.PositionProvider = () => new Vector2(
                buildContext.Graphics.GraphicsDevice.Viewport.Width / 2f, 60f);
            uiText.Anchor = TextAnchor.Top;
            uiText.FallbackColor = Color.Gold;
            uiText.DropShadow = true;

            scene.Add(statusGO);
            return textHolder;
        }

        private static void RegisterEventSubscription(
            string[] statusText, bool[] switchActivated, GameStateSystem gameStateSystem)
        {
            EngineContext.Instance.Events
                .On<SwitchActivatedEvent>()
                .WithPriorityPreset(EventPriority.Gameplay)
                .Do(evt =>
                {
                    switchActivated[0] = true;
                    statusText[0] = evt.SwitchName + " activated! Something changed...";
                    EngineContext.Instance.Events.Publish(
                        new PlaySfxEvent("SFX_UI_Click_Designed_Pop_Generic_1", 1f, false, null));
                });

            EngineContext.Instance.Events
                .On<AlarmTriggeredEvent>()
                .WithPriorityPreset(EventPriority.UI)
                .Do(evt =>
                {
                    statusText[0] = "ALARM in " + evt.ZoneName + "!";
                    EngineContext.Instance.Events.Publish(
                        new PlaySfxEvent("explosion1", 1f, false, null));
                });

            EngineContext.Instance.Events
                .On<GameWonEvent>()
                .WithPriorityPreset(EventPriority.UI)
                .Do(evt =>
                {
                    statusText[0] = "YOU WIN! The switch changed the game state!";
                    EngineContext.Instance.Events.Publish(
                        new PlaySfxEvent("SFX_UI_Click_Designed_Pop_Movement_Open_1", 1f, false, null));
                });

            gameStateSystem.ConfigureConditions(
                winCondition: new PredicateCondition("Switch activated", () => switchActivated[0]),
                loseCondition: null);
        }

        private static void BuildSwitchTrigger(Scene scene, ZoneBuildContext buildContext)
        {
            var triggerGO = new GameObject("Main Switch");
            triggerGO.Transform.TranslateTo(new Vector3(-6f, 1.5f, 10f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            triggerGO.AddComponent(meshFilter);
            var renderer = triggerGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("checkerboard");

            var collider = triggerGO.AddComponent<BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = triggerGO.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            scene.Add(triggerGO);

            float lastTriggeredTime = float.NegativeInfinity;
            const float COOLDOWN_SECONDS = 3f;

            EngineContext.Instance.Events.Subscribe<GDEngine.Core.Events.TriggerEvent>(evt =>
            {
                if (evt.TriggerBody?.GameObject == triggerGO)
                {
                    if (Time.TimeSinceStartupSecs - lastTriggeredTime < COOLDOWN_SECONDS)
                    {
                        return;
                    }
                    lastTriggeredTime = Time.TimeSinceStartupSecs;

                    EngineContext.Instance.Events.Publish(new SwitchActivatedEvent("Main Switch"));
                }
            });
        }

        private static void BuildAlarmTrigger(Scene scene, ZoneBuildContext buildContext)
        {
            var triggerGO = new GameObject("Danger Zone");
            triggerGO.Transform.TranslateTo(new Vector3(6f, 1.5f, 10f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            triggerGO.AddComponent(meshFilter);
            var renderer = triggerGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");

            var collider = triggerGO.AddComponent<BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = triggerGO.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            scene.Add(triggerGO);

            float lastTriggeredTime = float.NegativeInfinity;
            const float COOLDOWN_SECONDS = 3f;

            EngineContext.Instance.Events.Subscribe<GDEngine.Core.Events.TriggerEvent>(evt =>
            {
                if (evt.TriggerBody?.GameObject == triggerGO)
                {
                    if (Time.TimeSinceStartupSecs - lastTriggeredTime < COOLDOWN_SECONDS)
                    {
                        return;
                    }
                    lastTriggeredTime = Time.TimeSinceStartupSecs;

                    EngineContext.Instance.Events.Publish(new AlarmTriggeredEvent("Danger Zone"));
                }
            });
        }
    }
}

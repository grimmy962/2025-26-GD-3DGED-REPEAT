using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Events;
using GDEngine.Core.Factories;
using GDEngine.Core.Orchestration;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using GDEngine.Core.Timing;
using GDGame.Zones.Shared;
using Microsoft.Xna.Framework;

namespace GDGame.Zones
{
    // R4 - Orchestration zone
    // walking into a trigger starts a named sequence that raises an artifact by a random amount and checks if it cleared a height threshold
    // genuinely a coin flip each run
    // so both outcomes are actually reachable
    public sealed class OrchestrationZoneBuilder : IZoneBuilder
    {
        public const string SCENE_NAME = "OrchestrationZone";
        private const string SEQUENCE_NAME = "artifact_ritual";
        private const float ARTIFACT_RISE_TARGET_Y = 3.5f;
        private const int GROUND_SCALE = 100;

        private GameObject? _artifactGO;
        private string[]? _statusText;
        private Vector3 _artifactStartPosition;
        private static readonly System.Random _random = new System.Random();

        public string SceneName => SCENE_NAME;

        public Scene Build(ZoneBuildContext buildContext)
        {
            var scene = new Scene(buildContext.EngineContext, SCENE_NAME);

            ZoneSystemFactory.AddCoreSystems(
                scene,
                buildContext.Graphics,
                buildContext.Sounds,
                includePhysics: true,
                gravity: AppData.GRAVITY);

            var orchestrationSystem = new OrchestrationSystem();
            orchestrationSystem.Configure(options =>
            {
                options.Time = Orchestrator.OrchestrationTime.Unscaled;
                options.LocalScale = 1;
                options.Paused = false;
            });

            scene.Add(orchestrationSystem);

            BuildGround(scene, buildContext);
            ZonePlayerFactory.Create(scene, new Vector3(0f, 1.5f, 0f));

            var artifactGO = BuildRitualArtifact(scene, buildContext);
            _artifactGO = artifactGO;
            _artifactStartPosition = artifactGO.Transform.Position;

            var statusText = BuildStatusText(scene, buildContext);
            _statusText = statusText;

            RegisterRitualSequence(orchestrationSystem, artifactGO, statusText);
            BuildStartTrigger(scene, buildContext, orchestrationSystem);

            BuildReturnPortal(scene, buildContext);

            ZoneAnnotationFactory.Create(
                scene,
                buildContext.Graphics.GraphicsDevice,
                buildContext.Fonts.Get("perf_stats_font"),
                systemName: "Orchestration System",
                apiUsed: "OrchestrationSystem, Orchestrator.Builder (WaitSeconds, Publish, Do, If)",
                description: "Walk into the glowing trigger to start the ritual sequence.\n" +
                             "8 steps span UI, Audio, and Transform, including a conditional\n" +
                             "step that changes the outcome message.");

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

        private static GameObject BuildRitualArtifact(Scene scene, ZoneBuildContext buildContext)
        {
            var artifactGO = new GameObject("Ritual Artifact");
            artifactGO.Transform.TranslateTo(new Vector3(0f, 0.5f, 10f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            artifactGO.AddComponent(meshFilter);

            var renderer = artifactGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");

            scene.Add(artifactGO);
            return artifactGO;
        }

        // text-holder is a 1-element array on purpose - a lambda can't
        // reassign a captured plain string, but it can change what's inside a captured array
        private static string[] BuildStatusText(Scene scene, ZoneBuildContext buildContext)
        {
            var textHolder = new string[] { "Walk into the trigger to begin." };

            var statusGO = new GameObject("Ritual Status Text");
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

        // the named sequence itself
        // resets the artifact back to its start position first (so repeated runs in the same visit don't stack on top of each other)
        // then rises it by a random amount, then checks if that was enough to clear the threshold
        // genuinely random each time, so both outcomes actually happen
        private static void RegisterRitualSequence(
            OrchestrationSystem orchestrationSystem, GameObject artifactGO, string[] statusText)
        {
            var orchestrator = orchestrationSystem.Orchestrator;

            orchestrator.Build(SEQUENCE_NAME)
                .Do(api => artifactGO.Transform.TranslateTo(new Vector3(0f, 0.5f, 10f)))
                .Do(api =>
                {
                    float riseAmount = ARTIFACT_RISE_TARGET_Y * (float)(0.7 + _random.NextDouble() * 0.6);
                    artifactGO.Transform.TranslateBy(new Vector3(0f, riseAmount, 0f));
                })
                .WaitSeconds(1.5f)
                .Publish(new PlaySfxEvent("SFX_UI_Click_Designed_Pop_Generic_1", 1f, false, null))
                .Do(api => statusText[0] = "The artifact begins to rise...")
                .WaitSeconds(1.5f)
                .If(
                    ctx => artifactGO.Transform.Position.Y >= ARTIFACT_RISE_TARGET_Y - 0.1f,
                    thenBranch: then => then
                        .Do(api => statusText[0] = "The ritual succeeded!")
                        .Publish(new PlaySfxEvent("explosion1", 1f, false, null)),
                    elseBranch: otherwise => otherwise
                        .Do(api => statusText[0] = "The ritual failed...")
                        .Publish(new PlaySfxEvent("SFX_UI_Click_Designed_Pop_Negative_Close_1", 1f, false, null)))
                .WaitSeconds(2.5f)
                .Do(api => statusText[0] = "Walk into the trigger to begin.")
                .Register();
        }

        private static void BuildStartTrigger(Scene scene, ZoneBuildContext buildContext, OrchestrationSystem orchestrationSystem)
        {
            var triggerGO = new GameObject("Start Ritual Trigger");
            triggerGO.Transform.TranslateTo(new Vector3(0f, 1.5f, 5f));

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
            const float COOLDOWN_SECONDS = 8f;

            EngineContext.Instance.Events.Subscribe<TriggerEvent>(evt =>
            {
                if (evt.TriggerBody?.GameObject == triggerGO)
                {
                    if (Time.TimeSinceStartupSecs - lastTriggeredTime < COOLDOWN_SECONDS)
                    {
                        return;
                    }
                    lastTriggeredTime = Time.TimeSinceStartupSecs;

                    EngineContext.Instance.Events.Publish(new PlaySfxEvent("SFX_UI_Click_Designed_Pop_Movement_Open_1", 1f, false, null));
                    orchestrationSystem.Orchestrator.Start(SEQUENCE_NAME, scene, EngineContext.Instance);
                }
            });
        }

        private static void BuildReturnPortal(Scene scene, ZoneBuildContext buildContext)
        {
            var portalGO = new GameObject("Portal to Hub");
            portalGO.Transform.TranslateTo(new Vector3(0f, 1.5f, -5f));
            portalGO.Transform.ScaleTo(new Vector3(2f, 3f, 0.2f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(buildContext.Graphics.GraphicsDevice);
            portalGO.AddComponent(meshFilter);
            var renderer = portalGO.AddComponent<MeshRenderer>();
            renderer.Material = buildContext.MatBasicLit;
            renderer.Overrides.MainTexture = buildContext.Textures.Get("crate1");

            var collider = portalGO.AddComponent<BoxCollider>();
            collider.Size = new Vector3(2f, 3f, 2f);
            collider.IsTrigger = true;

            var rigidBody = portalGO.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            var portal = portalGO.AddComponent<ZonePortal>();
            portal.TargetSceneName = HubSceneBuilder.SCENE_NAME;

            scene.Add(portalGO);
        }

        // called on entry so the artifact and text are always fresh,
        // instead of leaving whatever state they were in from a previous run within the same visit
        public void ResetRitual()
        {
            if (_artifactGO != null)
            {
                _artifactGO.Transform.TranslateTo(_artifactStartPosition);
            }

            if (_statusText != null)
            {
                _statusText[0] = "Walk into the trigger to begin...";
            }
        }
    }
}
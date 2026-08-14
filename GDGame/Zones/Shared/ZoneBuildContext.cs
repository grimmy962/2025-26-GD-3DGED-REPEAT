using GDEngine.Core.Collections;
using GDEngine.Core.Managers;
using GDEngine.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using GDEngine.Core.Rendering;

namespace GDGame.Zones.Shared
{
    //bundles everything a zone builder needs so I'm not passing a huge list or arguments into every Build() call
    //all this content gets loaded once in Main.cs and just passed around from here - no zone loads its own separate copy of anything
    public sealed class ZoneBuildContext
    {
        //engine wide stuff: GraphicsDevice, Content, EventBus, timing
        public EngineContext EngineContext { get; }
        public GraphicsDeviceManager Graphics { get; }

        //the one SceneManager every zone gets registered with
        public SceneManager SceneManager { get; }

        public ContentDictionary<Model> Models { get; }
        public ContentDictionary<Texture2D> Textures { get; }
        public ContentDictionary<SpriteFont> Fonts { get; }
        public ContentDictionary<SoundEffect> Sounds { get; }

        // shared materials, so i;m not making a new one per zone
        public Material MatBasicLit { get; }
        public Material MatBasicUnlitGround { get; }

        public ZoneBuildContext(
            EngineContext engineContext,
            GraphicsDeviceManager graphics,
            SceneManager sceneManager,
            ContentDictionary<Model> models,
            ContentDictionary<Texture2D> textures,
            ContentDictionary<SpriteFont> fonts,
            ContentDictionary<SoundEffect> sounds,
            Material matBasicLit,
            Material matBasicUnlitGround)
        {
            EngineContext = engineContext;
            Graphics = graphics;
            SceneManager = sceneManager;
            Models = models;
            Textures = textures;
            Fonts = fonts;
            Sounds = sounds;
            MatBasicLit = matBasicLit;
            MatBasicUnlitGround = matBasicUnlitGround;
        }
    }
}

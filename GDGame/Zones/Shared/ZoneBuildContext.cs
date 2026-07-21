using GDEngine.Core.Collections;
using GDEngine.Core.Managers;
using GDEngine.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using GDEngine.Core.Rendering;

namespace GDGame.Zones.Shared
{
    public sealed class ZoneBuildContext
    {
        public EngineContext EngineContext { get; }
        public GraphicsDeviceManager Graphics { get; }
        public SceneManager SceneManager { get; }

        public ContentDictionary<Model> Models { get; }
        public ContentDictionary<Texture2D> Textures { get; }
        public Material MatBasicLit { get; }
        public Material MatBasicUnlitGround { get; }
        public ContentDictionary<SpriteFont> Fonts { get; }
        public ContentDictionary<SoundEffect> Sounds { get; }


        public ZoneBuildContext(
            EngineContext engineContext,
            GraphicsDeviceManager graphics,
            SceneManager sceneManager,
            ContentDictionary<Model> models,
            ContentDictionary<Texture2D> textures,
            ContentDictionary<SpriteFont> fonts,
            ContentDictionary<SoundEffect> sounds)
        
        {
            EngineContext = engineContext;
            Graphics = graphics;
            SceneManager = sceneManager;
            Models = models;
            Textures = textures;
            Fonts = fonts;
            Sounds = sounds;
        }

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

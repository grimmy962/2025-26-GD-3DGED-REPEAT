using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core
{
    /// <summary>
    /// Service hub to store references to core engine related components
    /// </summary>
    /// <example>
    /// </example>
    /// <see cref="Scene"/>
    public class EngineContext        //TODO - ensure singleton, add thread lock, implement IDisposable
    {
        private static bool _isInstanciated;
        private static EngineContext? _instance;

        public GraphicsDevice GraphicsDevice { get; }
        public ContentManager Content { get; }
        public SpriteBatch SpriteBath { get; }
        public static EngineContext? Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("Ensure you call Initialize() first");

                return _instance;
            }
        }
        public static void Initialize(GraphicsDevice graphicsDevice,
            ContentManager content)
        {
            if (!_isInstanciated)
            {
                _isInstanciated = true;
                _instance = new EngineContext(graphicsDevice, content);
            }
        }
        private EngineContext(GraphicsDevice graphicsDevice, 
            ContentManager content)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;
            SpriteBath = new SpriteBatch(graphicsDevice);
        }

        //TODO - add, remove
    }
}

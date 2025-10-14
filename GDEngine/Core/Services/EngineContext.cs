using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Services
{
    /// <summary>
    /// Service hub to allow access to engine services and shared resources.
    /// Utilising the SOLI<b>D</b> with Dependency Inversion Principle
    /// </summary>
    /// <see cref="Scene"/>
    /// <see cref="GameObject"/>
    /// <see cref="Camera"/>
    /// <see cref="https://www.geeksforgeeks.org/system-design/solid-principle-in-programming-understand-with-real-life-examples/"/>
    public class EngineContext : IDisposable       //TODO - add thread lock, implement IDisposable
    {
        #region Static Fields
        private static EngineContext? _instance;
        #endregion

        #region Properties
        public GraphicsDevice GraphicsDevice { get; }
        public ContentManager Content { get; }
        public SpriteBatch SpriteBatch { get; }
        public static EngineContext? Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("Ensure you call Initialize() first");

                return _instance;
            }
        }
        #endregion
 
        #region Core Methods
        public static void Initialize(GraphicsDevice graphicsDevice,
    ContentManager content)
        {
            if (_instance != null)
                throw new InvalidOperationException("EngineContext already initialized");

            _instance = new EngineContext(graphicsDevice, content);
        }

        private EngineContext(GraphicsDevice graphicsDevice,
            ContentManager content)
        {
            GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            SpriteBatch = new SpriteBatch(graphicsDevice);
        }
        #endregion

        #region Housekeeping Methods
        public void Dispose()
        {
            //TODO - Wk5 - Dispose any disposables!
        } 
        #endregion
    }
}

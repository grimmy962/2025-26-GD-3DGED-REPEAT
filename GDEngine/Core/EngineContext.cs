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
        public GraphicsDevice GraphicsDevice { get; }
        public ContentManager Content { get; }
        public SpriteBatch SpriteBath { get; }

        public EngineContext(GraphicsDevice graphicsDevice, 
            ContentManager content)
        {
            GraphicsDevice = graphicsDevice;
            Content = content;
            SpriteBath = new SpriteBatch(graphicsDevice);
        }

        //TODO - add, remove
    }
}

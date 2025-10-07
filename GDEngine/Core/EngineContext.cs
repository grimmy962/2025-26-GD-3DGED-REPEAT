using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core
{
    /// <summary>
    /// Stores references to core engine related components
    /// </summary>
    /// <example>
    /// </example>
    /// <see cref="Scene"/>
    public class EngineContext
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

    }
}

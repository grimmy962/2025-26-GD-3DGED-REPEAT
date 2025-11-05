using GDEngine.Core.Components;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Rendering
{
    public interface IRender
    {
        public void Render(GraphicsDevice device, Camera camera);
    }
}
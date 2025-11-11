using GDEngine.Core.Components;
using GDEngine.Core.Systems.Draw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Rendering.UI
{
    public class UITextureRenderer : UIRenderer
    {
        private Texture2D _texture;
        private Vector2 _destinationRectangle;

        private SpriteBatch _spriteBatch;

        public Texture2D Texture { get => _texture; set => _texture = value; }
        public Vector2 DestinationRectangle { get => _destinationRectangle; set => _destinationRectangle = value; }

        protected override void Awake()
        {
            var scene = GameObject?.Scene;
            if (scene == null)
                throw new NullReferenceException("OverlayRenderer requires a GameObject in a Scene.");

            var _uiMenuSystem = scene.GetSystem<UIMenuSystem>();
            if (_uiMenuSystem == null)
                throw new InvalidOperationException("_uiMenuSystem not found. Add it to the Scene before using OverlayRenderer.");

            _uiMenuSystem.Add(this);

            if (GameObject == null || GameObject.Scene == null)
                throw new NullReferenceException("Something is null!");

            _spriteBatch = GameObject.Scene.Context.SpriteBatch;
        }
        public override void Draw(GraphicsDevice device, Camera camera)
        {
            //null checks


            _spriteBatch.Begin();
            _spriteBatch.Draw(_texture, _destinationRectangle, Color.White);
            _spriteBatch.End();
        }
    }

     
}

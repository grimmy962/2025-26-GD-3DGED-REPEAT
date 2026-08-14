using GDEngine.Core.Entities;
using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.Zones.Shared
{
    //builds the required annotation overlay
    //the little ui text in the corner that says which system a zone is showing off
    //every zone (and the hub) calls this with different text, so it's all in one place instead of copy-pasted six times
    public static class ZoneAnnotationFactory
    {
        #region Constants

        private const string ANNOTATION_GAMEOBJECT_NAME = "Zone Annotation";
        private const float SCREEN_MARGIN_PIXELS = 20F;

        #endregion

        #region Methods

        public static GameObject Create(
            Scene scene,
            GraphicsDevice graphicsDevice,
            SpriteFont font,
            string systemName,
            string apiUsed,
            string description)
        {
            string annotationText = "System: " + systemName + "\nAPI: " + apiUsed + "\n" + description;

            //bottom left corner with a small margin
            Vector2 anchorPosition = new Vector2(
                SCREEN_MARGIN_PIXELS,
                graphicsDevice.Viewport.Height - SCREEN_MARGIN_PIXELS);

            var annotationGO = new GameObject(ANNOTATION_GAMEOBJECT_NAME);
            var uiText = annotationGO.AddComponent<UIText>();
            uiText.Font = font;
            uiText.TextProvider = () => annotationText;
            uiText.PositionProvider = () => anchorPosition;
            uiText.Anchor = TextAnchor.BottomLeft;
            uiText.FallbackColor = Color.White;
            uiText.DropShadow = true;

            scene.Add(annotationGO);
            return annotationGO;
        }

        #endregion
    }
}

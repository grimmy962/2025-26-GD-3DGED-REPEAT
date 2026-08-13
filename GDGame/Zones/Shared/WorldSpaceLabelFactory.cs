using GDEngine.Core.Entities;
using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.Zones.Shared
{
	public static class WorldSpaceLabelFactory
	{
		private const float OFFSCREEN_PARK_POSITION = -10000f;

		public static void Create(
			Scene scene, GraphicsDevice graphicsDevice, SpriteFont font, Vector3 worldPosition, string text)
		{
			var labelGO = new GameObject("Label: " + text);

			var uiText = labelGO.AddComponent<UIText>();
			uiText.Font = font;
			uiText.TextProvider = () => text;
			uiText.Anchor = TextAnchor.Center;
			uiText.FallbackColor = Color.White;
			uiText.DropShadow = true;

			uiText.PositionProvider = () =>
			{
				var camera = scene.ActiveCamera;
				if (camera == null || camera.Transform == null)
				{
					return new Vector2(OFFSCREEN_PARK_POSITION, OFFSCREEN_PARK_POSITION);
				}

				Vector3 toLabel = worldPosition - camera.Transform.Position;
				if (Vector3.Dot(camera.Transform.Forward, toLabel) <= 0f)
				{
					return new Vector2(OFFSCREEN_PARK_POSITION, OFFSCREEN_PARK_POSITION);
				}

				var screenPos = graphicsDevice.Viewport.Project(worldPosition, camera.Projection, camera.View, Matrix.Identity);

				return new Vector2(screenPos.X, screenPos.Y);
			};
			scene.Add(labelGO);
		}
	}
}

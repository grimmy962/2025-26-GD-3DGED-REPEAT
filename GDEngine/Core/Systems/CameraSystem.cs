using GDEngine.Core.Components;
using GDEngine.Core.Enums;      
using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDEngine.Core.Systems
{
    /// <summary>
    /// Manages runtime cameras: registration, aspect/resize sync, clear flags, sorting, and helpers.
    /// Runs in Render lifecycle before the RenderingSystem.
    /// </summary>
    public sealed class CameraSystem : SystemBase
    {
        #region Fields
        private readonly List<Camera> _cameras = new List<Camera>();
        private readonly Dictionary<Camera, BoundingFrustum> _frusta = new Dictionary<Camera, BoundingFrustum>();

        private Camera? _activeCamera;
        private GraphicsDevice _graphicsDevice;
        private int _backbufferWidth;
        private int _backbufferHeight;

        private readonly List<Camera> _sorted = new List<Camera>();
        #endregion

        #region Properties
        public Camera? ActiveCamera
        {
            get => _activeCamera;
            set
            {
                if (_activeCamera == value)
                    return;

                _activeCamera = value;
                EnsureFrustum(_activeCamera);
            }
        }

        public IReadOnlyList<Camera> Cameras => _cameras;
        #endregion

        #region Constructors
        public CameraSystem(GraphicsDevice graphicsDevice, int order = -100)
            : base(FrameLifecycle.Render, order)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _backbufferWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            _backbufferHeight = graphicsDevice.PresentationParameters.BackBufferHeight;
        }
        #endregion

        #region Methods
        public void Add(Camera camera)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            if (_cameras.Contains(camera))
                return;

            _cameras.Add(camera);
            EnsureFrustum(camera);
            SyncAspect(camera);

            if (_activeCamera == null)
                _activeCamera = camera;
        }

        public void Remove(Camera camera)
        {
            if (camera == null)
                return;

            _cameras.Remove(camera);
            _frusta.Remove(camera);

            if (_activeCamera == camera)
            {
                if (_cameras.Count > 0)
                    _activeCamera = _cameras[0];
                else
                    _activeCamera = null;
            }
        }

        public void GetSortedStack(List<Camera> destinationList)
        {
            if (destinationList == null)
                throw new ArgumentNullException(nameof(destinationList));

            destinationList.Clear();
            destinationList.AddRange(_cameras);

            destinationList.Sort((a, b) =>
            {
                int roleCompare = a.StackRole.CompareTo(b.StackRole);
                if (roleCompare != 0)
                    return roleCompare;

                return a.Depth.CompareTo(b.Depth);
            });
        }

        public void ApplyClears(Camera camera)
        {
            if (camera == null)
                return;

            switch (camera.ClearFlags)
            {
                case CameraClearFlags.Color:
                case CameraClearFlags.Skybox:
                    _graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, camera.ClearColor, 1f, 0);
                    break;

                case CameraClearFlags.DepthOnly:
                    _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1f, 0);
                    break;

                case CameraClearFlags.None:
                    break;
            }
        }

        public void BuildVisibleSet(Camera camera, IEnumerable<MeshRenderer> allRenderers, List<MeshRenderer> destinationList)
        {
            if (destinationList == null)
                throw new ArgumentNullException(nameof(destinationList));

            destinationList.Clear();

            if (camera == null)
                return;

            LayerMask cameraMask = camera.CullingMask;
            BoundingFrustum frustum = GetFrustum(camera);

            foreach (var meshRenderer in allRenderers)
            {
                // Layer now comes from GameObject (per your preference)
                LayerMask objectLayer = meshRenderer.GameObject != null ? meshRenderer.GameObject.Layer : LayerMask.All;
                if ((objectLayer & cameraMask) == 0)
                    continue;

                // TODO: add frustum test once you expose bounds
                // if (!Intersects(frustum, meshRenderer.Bounds)) continue;

                destinationList.Add(meshRenderer);
            }
        }

        public Vector3 ScreenToWorld(Camera camera, Vector3 screenPoint)
        {
            var viewport = _graphicsDevice.Viewport;
            return viewport.Unproject(screenPoint, camera.Projection, camera.View, Matrix.Identity);
        }

        public Vector3 WorldToScreen(Camera camera, Vector3 worldPoint)
        {
            var viewport = _graphicsDevice.Viewport;
            return viewport.Project(worldPoint, camera.Projection, camera.View, Matrix.Identity);
        }

        public Ray ScreenPointToRay(Camera camera, Vector2 screenPixel)
        {
            var viewport = _graphicsDevice.Viewport;

            Vector3 nearPoint = viewport.Unproject(new Vector3(screenPixel, 0f), camera.Projection, camera.View, Matrix.Identity);
            Vector3 farPoint = viewport.Unproject(new Vector3(screenPixel, 1f), camera.Projection, camera.View, Matrix.Identity);
            Vector3 direction = Vector3.Normalize(farPoint - nearPoint);
            return new Ray(nearPoint, direction);
        }
        #endregion

        #region Lifecycle Methods

        // Runs in the Render lifecycle (Scene.Draw dispatches this). Use Draw() for “pre-render” prep.
        public override void Draw(float deltaTime)
        {
            // Handle resize which requires an update to aspect sync
            var presentation = _graphicsDevice.PresentationParameters;
            if (presentation.BackBufferWidth != _backbufferWidth || presentation.BackBufferHeight != _backbufferHeight)
            {
                _backbufferWidth = presentation.BackBufferWidth;
                _backbufferHeight = presentation.BackBufferHeight;

                for (int i = 0; i < _cameras.Count; i++)
                    SyncAspect(_cameras[i]);
            }

            // Refresh frusta from latest camera matrices (after components’ LateUpdate)
            for (int i = 0; i < _cameras.Count; i++)
                UpdateFrustumFromCamera(_cameras[i]);
        }
        #endregion

        #region Housekeeping Methods
        private void SyncAspect(Camera camera)
        {
            if (camera == null)
                return;

            camera.AspectRatio = (float)_backbufferWidth / Math.Max(1, _backbufferHeight);
        }

        private void EnsureFrustum(Camera? camera)
        {
            if (camera == null)
                return;

            if (!_frusta.ContainsKey(camera))
                _frusta[camera] = new BoundingFrustum(camera.ViewProjection);
            else
                _frusta[camera].Matrix = camera.ViewProjection;
        }

        private void UpdateFrustumFromCamera(Camera camera)
        {
            BoundingFrustum frustum = GetFrustum(camera);
            frustum.Matrix = camera.ViewProjection;
        }

        private BoundingFrustum GetFrustum(Camera camera)
        {
            EnsureFrustum(camera);
            return _frusta[camera];
        }

        private bool Intersects(BoundingFrustum frustum, BoundingBox bounds)
        {
            return frustum.Contains(bounds) != ContainmentType.Disjoint;
        }
        #endregion
    }
}

using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework;

namespace GDEngine.Core.Components
{
    /// <summary>
    /// Camera component with switchable Perspective/Orthographic projection, LayerMask culling,
    /// and per-camera clear/stack settings. Uses lazy view/projection recomputation via dirty flags.
    /// </summary>
    /// <see cref="Transform"/>
    /// <see cref="Component"/>
    public sealed class Camera : Component
    {
        #region Static Fields
        #endregion

        #region Fields
        private float _fieldOfView = MathHelper.PiOver4;
        private float _aspectRatio = 16f / 9f;
        private float _nearPlane = 0.1f;
        private float _farPlane = 1000f;

        // Orthographic controls
        private float _orthographicSize = 10f; // half-height in world units

        // Projection mode
        private ProjectionType _projectionType = ProjectionType.Perspective;

        // LayerMask culling
        private LayerMask _cullingMask = LayerMask.All;

        // Camera stack / clearing
        private CameraClearFlags _clearFlags = CameraClearFlags.Color;
        private Color _clearColor = Color.CornflowerBlue;
        private CameraStackRole _stackRole = CameraStackRole.Base;
        private int _depth;

        private Matrix _view;
        private Matrix _projection;
        private Matrix _viewProjection;

        private bool _viewDirty = true;
        private bool _projectionDirty = true;
        #endregion

        #region Properties
        /// <summary>
        /// View matrix (recalculated on demand).
        /// </summary>
        public Matrix View
        {
            get
            {
                if (_viewDirty)
                {
                    RecalculateView();
                }
                return _view;
            }
        }

        /// <summary>
        /// Projection matrix (recalculated on demand).
        /// </summary>
        public Matrix Projection
        {
            get
            {
                if (_projectionDirty)
                {
                    RecalculateProjection();
                }
                return _projection;
            }
        }

        /// <summary>
        /// ViewProjection matrix (recalculated on demand).
        /// </summary>
        public Matrix ViewProjection
        {
            get
            {
                if (_viewDirty || _projectionDirty)
                {
                    RecalculateViewProjection();
                }
                return _viewProjection;
            }
        }

        /// <summary>
        /// Perspective field of view in radians.
        /// </summary>
        public float FieldOfView
        {
            get => _fieldOfView;
            set
            {
                if (_fieldOfView != value)
                {
                    _fieldOfView = value;
                    _projectionDirty = true;
                }
            }
        }

        /// <summary>
        /// Aspect ratio (width / height).
        /// </summary>
        public float AspectRatio
        {
            get => _aspectRatio;
            set
            {
                if (_aspectRatio != value)
                {
                    _aspectRatio = value;
                    _projectionDirty = true;
                }
            }
        }

        /// <summary>
        /// Near clip plane distance.
        /// </summary>
        public float NearPlane
        {
            get => _nearPlane;
            set
            {
                if (_nearPlane != value)
                {
                    _nearPlane = value;
                    _projectionDirty = true;
                }
            }
        }

        /// <summary>
        /// Far clip plane distance.
        /// </summary>
        public float FarPlane
        {
            get => _farPlane;
            set
            {
                if (_farPlane != value)
                {
                    _farPlane = value;
                    _projectionDirty = true;
                }
            }
        }

        /// <summary>
        /// Orthographic half-height (world units). Width is derived from aspect ratio.
        /// </summary>
        public float OrthographicSize
        {
            get => _orthographicSize;
            set
            {
                if (_orthographicSize != value)
                {
                    _orthographicSize = value;
                    _projectionDirty = true;
                }
            }
        }

        /// <summary>
        /// Current projection mode.
        /// </summary>
        public ProjectionType ProjectionMode
        {
            get => _projectionType;
            set
            {
                if (_projectionType != value)
                {
                    _projectionType = value;
                    _projectionDirty = true;
                }
            }
        }

        /// <summary>
        /// Per-camera layer mask for culling.
        /// </summary>
        public LayerMask CullingMask
        {
            get => _cullingMask;
            set => _cullingMask = value;
        }

        /// <summary>
        /// Camera clear policy.
        /// </summary>
        public CameraClearFlags ClearFlags
        {
            get => _clearFlags;
            set => _clearFlags = value;
        }

        /// <summary>
        /// Color used when clearing with <see cref="CameraClearFlags.Color"/>.
        /// </summary>
        public Color ClearColor
        {
            get => _clearColor;
            set => _clearColor = value;
        }

        /// <summary>
        /// Stack role (Base or Overlay).
        /// </summary>
        public CameraStackRole StackRole
        {
            get => _stackRole;
            set => _stackRole = value;
        }

        /// <summary>
        /// Depth sort key within the same stack role (lower draws first).
        /// </summary>
        public int Depth
        {
            get => _depth;
            set => _depth = value;
        }
        #endregion

        #region Constructors
        #endregion

        #region Methods
        /// <summary>
        /// Toggle between Perspective and Orthographic projection.
        /// </summary>
        public void ToggleProjection()
        {
            if (_projectionType == ProjectionType.Perspective)
            {
                ProjectionMode = ProjectionType.Orthographic;
            }
            else
            {
                ProjectionMode = ProjectionType.Perspective;
            }
        }

        private void OnTransformChanged(Transform transform, TransformChangeFlags flags)
        {
            _viewDirty = true;
        }

        private void RecalculateView()
        {
            if (Transform == null)
            {
                throw new NullReferenceException(nameof(Transform));
            }

            Vector3 position = Transform.Position;
            Vector3 forward = Transform.Forward;
            Vector3 up = Transform.Up;

            _view = Matrix.CreateLookAt(position, position + forward, up);
            _viewDirty = false;
        }

        private void RecalculateProjection()
        {
            if (_projectionType == ProjectionType.Perspective)
            {
                _projection = Matrix.CreatePerspectiveFieldOfView(
                    _fieldOfView,
                    _aspectRatio,
                    _nearPlane,
                    _farPlane
                );
            }
            else
            {
                float height = 2f * _orthographicSize;
                float width = height * _aspectRatio;

                _projection = Matrix.CreateOrthographic(
                    width,
                    height,
                    _nearPlane,
                    _farPlane
                );
            }

            _projectionDirty = false;
        }

        private void RecalculateViewProjection()
        {
            if (_viewDirty)
            {
                RecalculateView();
            }
            if (_projectionDirty)
            {
                RecalculateProjection();
            }

            _viewProjection = _view * _projection;
        }
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            _viewDirty = true;
            _projectionDirty = true;

            if (Transform != null)
            {
                Transform.Changed += OnTransformChanged;
            }
        }
        #endregion

        #region Housekeeping Methods
        #endregion
    }

    /// <summary>
    /// Camera projection modes.
    /// </summary>
    public enum ProjectionType : sbyte
    {
        Perspective = 0,
        Orthographic = 1
    }

    /// <summary>
    /// Camera clear policies.
    /// </summary>
    public enum CameraClearFlags : sbyte
    {
        Skybox = 0,   // reserved; currently same as Color unless a skybox pass is added
        Color = 1,
        DepthOnly = 2,
        None = 3
    }

    /// <summary>
    /// Camera stack role used for sorting and composition.
    /// </summary>
    public enum CameraStackRole : sbyte
    {
        Base = 0,
        Overlay = 1
    }
}
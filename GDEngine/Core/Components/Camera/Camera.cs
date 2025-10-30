using Microsoft.Xna.Framework;
using GDEngine.Core.Rendering; 

namespace GDEngine.Core.Components
{
    /// <summary>
    /// Camera with switchable Perspective/Orthographic projection and a LayerMask culling mask.
    /// Uses lazy view/projection recomputation via dirty flags.
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

        private Matrix _view;
        private Matrix _projection;
        private Matrix _viewProjection;

        private bool _viewDirty = true;
        private bool _projectionDirty = true;
        #endregion

        #region Properties

        // Matrices (lazy)
        public Matrix View
        {
            get
            {
                if (_viewDirty)
                    RecalculateView();
                return _view;
            }
        }

        public Matrix Projection
        {
            get
            {
                if (_projectionDirty)
                    RecalculateProjection();
                return _projection;
            }
        }

        public Matrix ViewProjection
        {
            get
            {
                if (_viewDirty || _projectionDirty)
                    RecalculateViewProjection();
                return _viewProjection;
            }
        }

        // Perspective
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

        // Shared
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

        // Orthographic
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

        // Mode
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

        // Layer culling
        public LayerMask CullingMask
        {
            get => _cullingMask;
            set => _cullingMask = value;
        }
        #endregion

        #region Constructors
        #endregion

        #region Methods
        public void ToggleProjection()
        {
            ProjectionMode = _projectionType == ProjectionType.Perspective
                ? ProjectionType.Orthographic
                : ProjectionType.Perspective;
        }

        private void OnTransformChanged(Transform transform, TransformChangeFlags flags)
        {
            _viewDirty = true;
        }

        private void RecalculateView()
        {
            if (Transform == null)
                throw new NullReferenceException(nameof(Transform));

            var position = Transform.Position;
            var forward = Transform.Forward;
            var up = Transform.Up;

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
                // size = half-height; width depends on aspect ratio
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
                RecalculateView();
            if (_projectionDirty)
                RecalculateProjection();

            _viewProjection = _view * _projection;
        }
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            _viewDirty = true;
            _projectionDirty = true;

            if (Transform != null)
                Transform.Changed += OnTransformChanged;
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
}

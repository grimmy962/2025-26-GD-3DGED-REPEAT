using Microsoft.Xna.Framework;

namespace GDEngine.Core.Components
{
    /// <summary>
    /// Perspective camera with lazy view/projection using dirty flags.
    /// </summary>
    /// <see cref="Transform"/>
    /// <see cref="Component"/>
    public sealed class Camera : Component
    {
        #region Fields
        private float _fieldOfView = MathHelper.PiOver4;
        private float _aspectRatio = 16f / 9f;
        private float _nearPlane = 0.1f;
        private float _farPlane = 1000f;

        private Matrix _view;
        private Matrix _projection;
        private Matrix _viewProjection;

        private bool _viewDirty = true;
        private bool _projectionDirty = true;
        #endregion

        #region Properties

        //TODO - Wk5 - Add checks on values and deltas to avoid thrashing dirty flag
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

        public float FieldOfView
        {
            get => _fieldOfView;
            set
            {
                if (_fieldOfView != value) //TODO - Wk5/6 - thrashing
                {
                    _fieldOfView = value;
                    _projectionDirty = true;
                }
            }
        }

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
        #endregion

        #region Lifecycle Methods

        protected override void Awake()
        {
            // if we never mark at start as dirty then matrices MAY never be set (hence we will have a problem with effect.View)
            _viewDirty = true;
            _projectionDirty = true;

            if (Transform != null)
                Transform.Changed += OnTransformChanged; // subscribe

            //NO-OP in base
            //base.Awake();
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
            _projection = Matrix.CreatePerspectiveFieldOfView(
                _fieldOfView,
                _aspectRatio,
                _nearPlane,
                _farPlane
            );
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

        /*
        public Ray ScreenPointToRay(Vector2 screenPoint, Viewport viewport)
        {
            var nearPoint = viewport.Unproject(
                new Vector3(screenPoint, 0),
                Projection,
                View,
                Matrix.Identity
            );

            var farPoint = viewport.Unproject(
                new Vector3(screenPoint, 1),
                Projection,
                View,
                Matrix.Identity
            );

            var direction = Vector3.Normalize(farPoint - nearPoint);
            return new Ray(nearPoint, direction);
        }
        */

        //TODO - Wk5 - set View and Projection on Start
    }
}

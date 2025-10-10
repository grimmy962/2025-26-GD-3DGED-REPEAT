using Microsoft.Xna.Framework;

namespace GDEngine.Core
{
    public class Transform : Component
    {
        private Vector3 _localPosition;
        private Quaternion _localRotation;
        private Vector3 _localScale;

        private Transform? _parent;
        private List<Transform> _children = new List<Transform>();

        public Matrix LocalWorldMatrix
        {
            get
            {
                return Matrix.Identity
                    * Matrix.CreateScale(_localScale)
                     * Matrix.CreateFromQuaternion(_localRotation)
                      * Matrix.CreateTranslation(_localPosition);
            }
        }

        public Matrix WorldMatrix
        {
            get
            {
                return _parent == null ?
                LocalWorldMatrix : LocalWorldMatrix * _parent.WorldMatrix;
            }
        }

        public Quaternion WorldRotation
        {
            get
            {
                return _parent == null ?
                    LocalRotation : LocalRotation * _parent.WorldRotation;
            }
        }

        public Vector3 Forward => Vector3.Transform(Vector3.Forward,
            Matrix.CreateFromQuaternion(WorldRotation));

        public Vector3 Right => Vector3.Transform(Vector3.Right,
           Matrix.CreateFromQuaternion(WorldRotation));

        public Vector3 Up => Vector3.Transform(Vector3.Up,
           Matrix.CreateFromQuaternion(WorldRotation));




        public Vector3 LocalPosition { get => _localPosition; set => _localPosition = value; }
        public Quaternion LocalRotation { get => _localRotation; set => _localRotation = value; }
        public Vector3 LocalScale { get => _localScale; set => _localScale = value; }
        public Transform? Parent { get => _parent; set => _parent = value; }
        public List<Transform> Children { get => _children; set => _children = value; }
    }
}

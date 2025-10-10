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

        //world matrix = s * ro * t (ISROT)
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


    }
}

using GDEngine.Core.Entities;

namespace GDEngine.Core.Components
{
    public sealed class ThirdPersonController : Component
    {
        private string _targetName;
        private Scene? _scene;
        private GameObject? _targetObject;

        public ThirdPersonController(string targetName)
        {
            this._targetName = targetName;
        }

        protected override void Start()
        {
            if (GameObject == null)
                throw new NullReferenceException(nameof(GameObject));

            _scene = GameObject.Scene;

            if(_scene == null)
                throw new NullReferenceException(nameof(_scene));

            _targetObject = _scene.Find((GameObject go) => go.Name.Equals(_targetName));

            base.Start();
        }
        protected override void LateUpdate(float deltaTime)
        {
            if (_targetObject == null)
                return;

            var fwd = _targetObject.Transform.Forward;
            var up = _targetObject.Transform.Up;

            var cameraPosition = _targetObject.Transform.Position - 8*fwd + up;

            Transform.TranslateTo(cameraPosition);


            base.LateUpdate(deltaTime);
        }
    }
}

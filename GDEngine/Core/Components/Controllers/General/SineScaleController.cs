using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;

namespace GDEngine.Core.Components.Controllers.General
{
    public class SineScaleController : Component
    {
        private Vector3 _originalLocalScale;
        protected override void Update(float deltaTime)
        {
            float phase = (float)(Time.RealtimeSinceStartupSecs * 10);
            float scale = MathF.Sin(phase); // [1, +1]
            Transform?.ScaleTo(_originalLocalScale 
                + scale * new Vector3(0.25f, 1.25f, 0.25f) * 0.1f);      
        }

        protected override void Awake()
        {
           // if (Transform == null)
            //    throw new Exception();
            _originalLocalScale = Transform.LocalScale;
        }
    }
}

using GDEngine.Core.Timing;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDEngine.Core.Components.Controllers.General
{
    public class AnimationController : Component
    {
        private AnimationCurve _curve;
        private float totalAnimationTimeSecs = 20;
        private float _totalElapsedTimeSecs;

        public AnimationController(AnimationCurve curve)
        {
            _curve = curve;
        }

        protected override void Update(float deltaTime)
        {
            if (Transform == null)
                return;

            _totalElapsedTimeSecs += Time.UnscaledDeltaTime;

            // value in [0,1]
            float percentageComplete
                = _totalElapsedTimeSecs / totalAnimationTimeSecs;

           var upDelta = Transform.Up * _curve.Evaluate(percentageComplete);
           Transform.TranslateBy(upDelta, true);

        }
    }
}

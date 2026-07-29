using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Services;
using Microsoft.Xna.Framework;

namespace GDGame.Zones.Shared
{
	public sealed class PeriodicSpatialSfxEmitter : Component
	{
		private string _clipName = string.Empty;
		private float _intervalSeconds = 3f;
		private float _volume = 1f;
		private float _timer;

		public string ClipName
		{
			get => _clipName;
			set => _clipName = value ?? string.Empty;
		}

		public float IntervalSeconds
		{
			get => _intervalSeconds;
            set => _intervalSeconds = MathHelper.Max(0.1f, value);
		}

        public float Volume
        {
            get => _volume;
            set => _volume = value;
        }

        protected override void Update(float deltaTime)
        {
            _timer += deltaTime;

            if (_timer >= _intervalSeconds)
            {
                _timer = 0f;

                if (Transform != null)
                {
                    EngineContext.Instance.Events.Publish(
                        new PlaySfxEvent(_clipName, _volume, true, Transform));
                }
            }
        }
    }
}
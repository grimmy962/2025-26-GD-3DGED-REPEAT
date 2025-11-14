using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Services;

namespace GDEngine.Core.Events
{
    public class DamageListener : Component
    {
        private Scene? _scene;
        private EngineContext? _context;

        protected override void Awake()
        {
            base.Awake();

            // Get scene + context from the GameObject we are attached to
            _scene = GameObject?.Scene;
            _context = _scene?.Context;

            // Subscribe to DamageEvent via the EventBus
            _context?.Events.Subscribe<DamageEvent>(
                HandleDamage,
                0,
                null,
                false);
        }

        private void HandleDamage(DamageEvent @event)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[DamageListener] Target={@event.TargetName}, " +
                $"Source={@event.SourceName}, Amount={@event.Amount}, " +
                $"Type={@event.Type}, Critical={@event.IsCritical}, " +
                $"HitPos={@event.HitPosition}");
        }
    }
}

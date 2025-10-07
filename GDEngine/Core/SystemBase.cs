namespace GDEngine.Core
{
    /// <summary>
    /// Engine that calls update and draw on all systems
    /// </summary>
    public class SystemBase
    {
        public Scene? Scene { get; set; }
        public EngineContext? Context => Scene?.Context;

        public virtual void Update() { }
        public virtual void Draw() { }
    }
}

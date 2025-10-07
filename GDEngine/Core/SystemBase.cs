namespace GDEngine.Core
{
    /// <summary>
    /// Base class that calls update and draw on the system
    /// </summary>
    public class SystemBase
    {
        public Scene? Scene { get; set; }
        public EngineContext? Context => Scene?.Context;

        public virtual void Update() { }
        public virtual void Draw() { }
    }
}

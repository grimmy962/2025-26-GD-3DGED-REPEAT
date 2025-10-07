namespace GDEngine.Core
{
    public class SystemBase
    {
        public Scene Scene { get; set; }
        public EngineContext Context => Scene.Context;

        public virtual void Update() { }
        public virtual void Draw() { }
    }
}

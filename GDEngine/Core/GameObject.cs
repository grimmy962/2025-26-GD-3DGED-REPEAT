namespace GDEngine.Core
{
    /// <summary>
    /// Represents a game entity (drawn, updated, both, either)
    /// For example a Player, Enemy, Pickup, Wall, Door
    /// </summary>
    public class GameObject
    {
        #region Fields
        private readonly List<Component> _components; 
        #endregion

        #region Properties
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public Transform Transform { get; }
        public IEnumerable<Component> Components => _components; 
        #endregion

        public GameObject(string name)
        {
            _components = new List<Component>();
           // Name = name.Trim();
            Transform = new Transform();
            AddComponent(Transform);
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T component = new T();
            AddComponent(component);
            return component;
        }

        public void AddComponent(Component component)
        {
            component.GameObject = this;
            _components.Add(component);
        }

        //TODO - add GetComponent, GetComponents
    }
}

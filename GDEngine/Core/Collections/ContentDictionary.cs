using GDEngine.Core.Services;
using Microsoft.Xna.Framework.Content;

namespace GDEngine.Core
{
    /// <summary>
    /// Generic content dictionary with just Add/Remove/Get/Clear.
    /// Uses cached <see cref="EngineContext"/> to load assets on demand.
    /// </summary>
    public sealed class ContentDictionary<T> where T : class
    {
        #region Static Fields
        #endregion

        #region Fields
        private readonly Dictionary<string, T> _items;
        private readonly Dictionary<string, string> _paths;
        private readonly ContentManager _content;
        private readonly string _name;
        #endregion

        #region Properties
        // Optional label for debugging (e.g., "Texture2D").
        public string Name => _name;

        // Number of cached items.
        public int Count => _items.Count;
        #endregion

        #region Constructors
        /// <summary>
        /// Uses <see cref="EngineContext.Instance"/> and default name of typeof(T).Name.
        /// </summary>
        public ContentDictionary()
            : this(typeof(T).Name)
        {
        }

        /// <summary>
        /// Uses <see cref="EngineContext.Instance"/> with a custom name.
        /// </summary>
        public ContentDictionary(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Dictionary name must be non-empty.", nameof(name));

            if(EngineContext.Instance == null)
                throw new InvalidOperationException("EngineContext.Content is null. Ensure Content is initialized.");

            _content = EngineContext.Instance.Content;
            _items = new Dictionary<string, T>(128, StringComparer.OrdinalIgnoreCase);
            _paths = new Dictionary<string, string>(128, StringComparer.OrdinalIgnoreCase);
            _name = name;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds an item by key. If the key is new, loads from Content using path and stores it.
        /// </summary>
        /// <returns>If loaded then true, else false.</returns>
        public bool Add(string key, string path)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key must be non-empty.", nameof(key));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path must be non-empty.", nameof(path));

            // Do we already have asset loaded?
            if (!_paths.ContainsKey(key))
            {
                var loaded = _content.Load<T>(path);
                _items[key] = loaded;
                _paths[key] = path;
                return true;
            }

            return false;
           
        }

        /// <summary>
        /// Gets a previously added item by key. Throws if not present.
        /// </summary>
        public T? Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key must be non-empty.", nameof(key));

            T? value;
            if (_items.TryGetValue(key, out value))
                return value;
            else
                return null;
        }

        /// <summary>
        /// Removes a key if present. Returns true if removed.
        /// </summary>
        public bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            _paths.Remove(key);
            return _items.Remove(key);
        }

        /// <summary>
        /// Clears all cached references. Does not dispose assets (ContentManager owns them).
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _paths.Clear();
        }
        #endregion

        #region Lifecycle Methods
        #endregion

        #region Housekeeping Methods
        public override string ToString()
        {
            return $"ContentDictionary<{typeof(T).Name}>(Name={_name}, Count={_items.Count})";
        }
        #endregion
    }
}

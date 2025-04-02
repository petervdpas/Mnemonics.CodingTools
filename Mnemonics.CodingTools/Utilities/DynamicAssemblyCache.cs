using System.Collections.Generic;
using System.Reflection;

namespace Mnemonics.CodingTools.Utilities
{
    /// <summary>
    /// Provides a simple caching mechanism for dynamically loaded assemblies.
    /// Prevents multiple loads of the same assembly from file, ensuring reference identity consistency.
    /// </summary>
    public static class DynamicAssemblyCache
    {
        /// <summary>
        /// Internal cache mapping assembly file paths to loaded <see cref="Assembly"/> instances.
        /// </summary>
        private static readonly Dictionary<string, Assembly> _cache = [];

        /// <summary>
        /// Loads an assembly from the specified path, or retrieves it from the internal cache if already loaded.
        /// </summary>
        /// <param name="path">The file path of the assembly (.dll) to load.</param>
        /// <returns>
        /// The loaded <see cref="Assembly"/> instance. If the assembly was previously loaded from the same path,
        /// the cached instance is returned.
        /// </returns>
        public static Assembly LoadOrGet(string path)
        {
            if (_cache.TryGetValue(path, out var asm))
                return asm;

            var loaded = Assembly.LoadFrom(path);
            _cache[path] = loaded;
            return loaded;
        }
    }
}

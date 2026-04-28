

using System.Collections.Concurrent;
using System.Reflection;
using CompositLib.Components.Mixins;

namespace CompositLib.Components;

public static class ComponentTypeResolver
{
    private static readonly ConcurrentDictionary<Type, Type[]> _cache = new();

    public static Type[] Resolve(Type concrete)
    {
        return _cache.GetOrAdd(concrete, t =>
        {
            var baseTypes = new HashSet<Type>();
            Type[]? overrideTypes = null;

            var current = t;
            while (current != null)
            {
                var baseAttr = current.GetCustomAttribute<RegisterAsBaseAttribute>(false);
                if (baseAttr != null)
                {
                    foreach (var x in baseAttr.Types)
                        baseTypes.Add(x);
                }

                var overrideAttr = current.GetCustomAttribute<RegisterAsAttribute>(false);
                if (overrideAttr != null)
                {
                    overrideTypes = overrideAttr.Types;
                }

                current = current.BaseType;
            }

            if (overrideTypes != null)
            {
                foreach (var x in overrideTypes)
                    baseTypes.Add(x);
            }

            if (baseTypes.Count == 0)
                baseTypes.Add(t);

            return baseTypes.ToArray();
        });
    }
    private static readonly ConcurrentDictionary<Type, Type[]> _registrationCache = new();

    public static Type[] GetRegistrationKeys(Type type)
    {
        return _registrationCache.GetOrAdd(type, t =>
        {
            var set = new HashSet<Type>();

            // include full hierarchy (cached separately if you want)
            var current = t;
            while (current != null)
            {
                set.Add(current);

                foreach (var i in current.GetInterfaces())
                    set.Add(i);

                current = current.BaseType;
            }

            // include attribute-based types
            var resolved = ComponentTypeResolver.Resolve(t);
            foreach (var r in resolved)
                set.Add(r);

            // filter once
            return set
                .Where(x => typeof(IComponentBase).IsAssignableFrom(x))
                .ToArray();
        });
    }
}



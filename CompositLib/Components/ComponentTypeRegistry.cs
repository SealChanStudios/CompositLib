

using System.Collections.Concurrent;
using System.Reflection;
using CompositLib.Components.Mixins;

namespace CompositLib.Components;

public static class ComponentTypeResolver
{
    private static readonly ConcurrentDictionary<Type, Type[]> _cache = new();

    public static Type[] Resolve(Type type)
    {
        return _cache.GetOrAdd(type, t =>
        {
            var result = new HashSet<Type>();

            // walk full graph (classes + interfaces)
            var allTypes = GetHierarchyWithInterfaces(t);

            Type[]? overrideTypes = null;

            foreach (var current in allTypes)
            {
                // base registrations (merge)
                var baseAttr = current.GetCustomAttribute<RegisterAsBaseAttribute>(false);
                if (baseAttr != null)
                {
                    foreach (var x in baseAttr.Types)
                        result.Add(x);
                }

                // override registration (last wins)
                var overrideAttr = current.GetCustomAttribute<RegisterAsAttribute>(false);
                if (overrideAttr != null)
                {
                    overrideTypes = overrideAttr.Types;
                }
            }

            if (overrideTypes != null)
            {
                foreach (var x in overrideTypes)
                    result.Add(x);
            }

            if (result.Count == 0)
                result.Add(t);

            return result.ToArray();
        });
    }
    private static readonly ConcurrentDictionary<Type, Type[]> RegistrationCache = new();

    public static Type[] GetRegistrationKeys(Type type)
    {
        return RegistrationCache.GetOrAdd(type, t =>
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
    private static Type[] GetHierarchyWithInterfaces(Type type)
    {
        var result = new HashSet<Type>();
        var visited = new HashSet<Type>();
        var stack = new Stack<Type>();

        stack.Push(type);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == null || !visited.Add(current))
                continue;

            result.Add(current);

            // class hierarchy
            if (current.BaseType != null)
                stack.Push(current.BaseType);

            // interface hierarchy
            foreach (var i in current.GetInterfaces())
                stack.Push(i);
        }

        return result.ToArray();
    }
}



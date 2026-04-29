

using System.Collections.Concurrent;
using System.Reflection;
using CompositLib.Components.Mixins;

namespace CompositLib.Components;


public static class ComponentTypeResolver
{
    private static readonly Dictionary<Type, Type[]> Cache = new();
    private static readonly Dictionary<Type, Type[]> RegistrationCache = new();

    public static void ClearCaches()
    {
        Cache.Clear();
        RegistrationCache.Clear();
    }

    public static Type[] Resolve(Type type)
    {
        if (Cache.TryGetValue(type, out var cached))
        {
            return cached;
        }

        var result = new HashSet<Type>();
        var current = type;
        Type[]? overrideTypes = null;

        var stack = new Stack<Type>();
        while (current != null)
        {
            stack.Push(current);
            current = current.BaseType;
        }

        while (stack.Count > 0)
        {
            var t = stack.Pop();

            var baseAttr = t.GetCustomAttributes(typeof(RegisterAsBaseAttribute), false)
                            .FirstOrDefault() as RegisterAsBaseAttribute;

            if (baseAttr != null)
            {
                foreach (var x in baseAttr.Types)
                {
                    result.Add(x);
                }
            }

            var overrideAttr = t.GetCustomAttributes(typeof(RegisterAsAttribute), false)
                                .FirstOrDefault() as RegisterAsAttribute;

            if (overrideAttr != null)
            {
                overrideTypes = overrideAttr.Types;
            }
        }

        foreach (var i in type.GetInterfaces())
        {
            var baseAttr = i.GetCustomAttributes(typeof(RegisterAsBaseAttribute), false)
                            .FirstOrDefault() as RegisterAsBaseAttribute;

            if (baseAttr == null)
            {
                continue;
            }

            foreach (var x in baseAttr.Types)
            {
                result.Add(x);
            }
        }

        if (overrideTypes != null)
        {
            foreach (var x in overrideTypes)
            {
                result.Add(x);
            }
        }

        if (result.Count == 0)
        {
            result.Add(type);
        }

        var arr = result.ToArray();
        Cache[type] = arr;
        return arr;
    }

    public static Type[] GetRegistrationKeys(Type type)
    {
        if (RegistrationCache.TryGetValue(type, out var cached))
        {
            return cached;
        }

        var set = new HashSet<Type>();
        var current = type;

        while (current != null)
        {
            set.Add(current);

            foreach (var i in current.GetInterfaces())
            {
                set.Add(i);
            }

            current = current.BaseType;
        }

        foreach (var r in Resolve(type))
        {
            set.Add(r);
        }

        var arr = set.Where(x => typeof(IComponentBase).IsAssignableFrom(x)).ToArray();

        RegistrationCache[type] = arr;
        return arr;
    }
}
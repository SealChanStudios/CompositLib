using System.Collections.Concurrent;
using System.Reflection;
using CompositeLib.Components.Mixins;

namespace CompositeLib.Components;

public static class ComponentTypeResolver
{
    private static readonly ConcurrentDictionary<Type, Type[]> Cache = new();
    private static readonly ConcurrentDictionary<Type, Type[]> RegistrationCache = new();

    private static readonly ConcurrentDictionary<Type, RegisterAsBaseAttribute?> BaseAttrCache = new();

    private static readonly ConcurrentDictionary<Type, RegisterAsAttribute?> OverrideAttrCache = new();

    private static readonly ConcurrentDictionary<Type, Type[]> InterfaceCache = new();

    public static void ClearCaches()
    {
        Cache.Clear();
        RegistrationCache.Clear();
        BaseAttrCache.Clear();
        OverrideAttrCache.Clear();
        InterfaceCache.Clear();
    }

    public static Type[] Resolve(Type type)
    {
        return Cache.GetOrAdd(type, ResolveInternal);
    }

    private static Type[] ResolveInternal(Type type)
    {
        HashSet<Type> result = new HashSet<Type>();
        RegisterAsAttribute? overrideAttr = null;

        for (Type? current = type; current != null; current = current.BaseType)
        {
            RegisterAsBaseAttribute? baseAttr = GetBaseAttr(current);

            if (baseAttr != null)
            {
                foreach (Type x in baseAttr.Types)
                {
                    result.Add(x);
                }
            }

            if (overrideAttr == null)
            {
                overrideAttr = GetOverrideAttr(current);
            }
        }

        foreach (Type i in GetInterfaces(type))
        {
            RegisterAsBaseAttribute? baseAttr = GetBaseAttr(i);

            if (baseAttr == null)
            {
                continue;
            }

            foreach (Type x in baseAttr.Types)
            {
                result.Add(x);
            }
        }

        if (overrideAttr != null)
        {
            foreach (Type x in overrideAttr.Types)
            {
                result.Add(x);
            }
        }

        if (result.Count == 0)
        {
            result.Add(type);
        }

        return result.ToArray();
    }

    public static Type[] GetRegistrationKeys(Type type)
    {
        return RegistrationCache.GetOrAdd(type, GetRegistrationKeysInternal);
    }

    private static Type[] GetRegistrationKeysInternal(Type type)
    {
        HashSet<Type> set = new HashSet<Type>();

        for (Type? current = type; current != null; current = current.BaseType)
        {
            set.Add(current);

            foreach (Type i in GetInterfaces(current))
            {
                set.Add(i);
            }
        }

        foreach (Type r in Resolve(type))
        {
            set.Add(r);
        }

        List<Type> list = new List<Type>(set.Count);

        foreach (Type x in set)
        {
            if (!typeof(IComponentBase).IsAssignableFrom(x)) continue;
            list.Add(x);
        }

        return list.ToArray();
    }

    private static RegisterAsBaseAttribute? GetBaseAttr(Type t)
    {
        return BaseAttrCache.GetOrAdd(t, static type =>
        {
            return type.GetCustomAttribute<RegisterAsBaseAttribute>(false);
        });
    }

    private static RegisterAsAttribute? GetOverrideAttr(Type t)
    {
        return OverrideAttrCache.GetOrAdd(t, static type =>
        {
            return type.GetCustomAttribute<RegisterAsAttribute>(false);
        });
    }

    private static Type[] GetInterfaces(Type t)
    {
        return InterfaceCache.GetOrAdd(t, static type =>
        {
            return type.GetInterfaces();
        });
    }
}
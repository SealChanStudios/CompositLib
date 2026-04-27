using System.Collections.Concurrent;
using System.Reflection;

namespace CompositLib.Components;

using System;

public static class ComponentTypeResolver
{
  private static readonly ConcurrentDictionary<Type, Type> _cache = new();

  public static Type Resolve(Type concrete)
  {
    return _cache.GetOrAdd(concrete, t =>
    {
      var attr = t.GetCustomAttribute<RegisterAsAttribute>(inherit: true);
      return attr?.Type ?? t;
    });
  }
}

using Chickensoft.Introspection;
using Godot;

namespace CompositeLib.Components.Mixins;

[Mixin]
public interface IComponentHost : IMixin<IComponentHost>
{
  void IMixin<IComponentHost>.Handler() { }

  static Node GetSelf(IComponentHost self)
  {
    return self.MixinState.Get<ComponentHostState>().Self;
  }
  Node GetSelf()
  {
    return MixinState.Get<ComponentHostState>().Self;
  }

  static void InvokeNotificationMethods(object? obj, int what)
  {
    if (obj is not IComponentHost host || obj is not Node)
    {
      return;
    }

    if (what != Node.NotificationReady) return;
    host.MixinState.Get<ComponentHostState>().Self = (Node)obj;
    host.RegisterComponentsOnReady();
    host.OnHostReady();
  }

  void RegisterComponentsOnReady()
  {
    if (this is not Node)
    {
      throw new InvalidOperationException("This is not a IComponentHost");
    }
    RegisterComponents(this);
  }

  void OnHostReady() { }

  // -----------------------------
  // REGISTER
  // -----------------------------
  static void RegisterComponents(IComponentHost parent)
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException("Parent is not IComponentHost");
    }

    var state = host.MixinState.Get<ComponentHostState>();

    state.Components.Clear();
    state.ComponentNodes.Clear();

    foreach (var child in host.GetSelf().GetChildren())
    {
      if (child is not IComponent component)
      {
        continue;
      }

      component.SetOwner(host);

      RegisterAllTypes(state, component, child);
    }
  }

  // -----------------------------
  // GET
  // -----------------------------
  static T? GetComponent<T>(IComponentHost host) where T : class, IComponentBase
  {
    var state = host.MixinState.Get<ComponentHostState>();

    if (!state.Components.TryGetValue(typeof(T), out var cached))
    {
      return null;
    }

    return cached as T;

  }

  static bool HasComponent<T>(IComponentHost host) where T : class, IComponentBase
  {
    var state = host.MixinState.Get<ComponentHostState>();
    return state.Components.ContainsKey(typeof(T));
  }

  static IComponent[] GetComponents(IComponentHost host)
  {
    var state = host.MixinState.Get<ComponentHostState>();
    return state.Components.Values.Distinct().ToArray();
  }

  // -----------------------------
  // ADD
  // -----------------------------

  static void AddComponent<T>(IComponentHost host, Node node)
    where T : class, IComponentBase
  {
    if (node is not IComponent component)
    {
      throw new InvalidOperationException();
    }

    var state = host.MixinState.Get<ComponentHostState>();

    component.SetOwner<T>(host);
    host.GetSelf().AddChild(node);

    RegisterAllTypes(state, component, node);
  }
  
  // -----------------------------
  // REMOVE
  // -----------------------------
  static void RemoveComponent<T>(IComponentHost host)
    where T : class, IComponentBase
  {
    var state = host.MixinState.Get<ComponentHostState>();

    if (!state.ComponentNodes.TryGetValue(typeof(T), out var node))
    {
      throw new InvalidOperationException();
    }

    var component = state.Components[typeof(T)];
    var keys = ComponentTypeResolver.Resolve(component.GetType());

    foreach (var key in keys) // cached
    {
      state.Components.Remove(key);
      state.ComponentNodes.Remove(key);
    }

    host.GetSelf().RemoveChild(node);
    node.QueueFree();
  }

  static Type[] GetComponentTypes(IComponentHost host)
  {
    var state = host.MixinState.Get<ComponentHostState>();
    return state.Components.Keys.ToArray();
  }

  // -----------------------------
  // INTERNAL TYPE SYSTEM
  // -----------------------------
  private static readonly Dictionary<Type, Type[]> TypeCache = new();
  
  private static void RegisterAllTypes(
    ComponentHostState state,
    IComponent component,
    Node node)
  {
    var keys = ComponentTypeResolver.Resolve(component.GetType());

    foreach (var key in keys) // cached → cheap
    {
      state.Components[key] = component;
      state.ComponentNodes[key] = node;
    }
  }

  private static Type[] GetAllTypes(Type type)
  {
    if (TypeCache.TryGetValue(type, out var cached))
    {
      return cached;
    }

    var list = new List<Type>();

    var current = type;
    while (current != null)
    {
      list.Add(current);

      foreach (var i in current.GetInterfaces())
      {
        list.Add(i);
      }

      current = current.BaseType;
    }

    var result = list.Distinct().ToArray();
    TypeCache[type] = result;
    return result;
  }
}
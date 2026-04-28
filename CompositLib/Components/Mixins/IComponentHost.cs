using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

namespace CompositLib.Components.Mixins;

[Mixin]
public interface IComponentHost : IMixin<IComponentHost>, IAutoNode
{
  void IMixin<IComponentHost>.Handler() { }
  new void Handler() => (this as IAutoNode).Handler();

  void IAutoOn.OnReady()
  {
    ((Node)this).RegisterComponents();
    OnHostReady();
  }

  void OnHostReady() { }

  // -----------------------------
  // REGISTER
  // -----------------------------
  static void RegisterComponents(Node parent)
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException("Parent is not IComponentHost");
    }

    var state = host.MixinState.Get<ComponentHostState>();

    state.Components.Clear();
    state.ComponentNodes.Clear();

    foreach (var child in parent.GetChildren())
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
  static T? GetComponent<T>(Node parent) where T : class, IComponentBase
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException("Parent is not IComponentHost");
    }

    var state = host.MixinState.Get<ComponentHostState>();

    if (!state.Components.TryGetValue(typeof(T), out var cached)) return null; 
    
    return cached as T;

  }

  static bool HasComponent<T>(Node parent) where T : class, IComponentBase
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException();
    }

    var state = host.MixinState.Get<ComponentHostState>();
    return state.Components.ContainsKey(typeof(T));
  }

  static IComponent[] GetComponents(Node parent)
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException("Parent is not IComponentHost");
    }

    var state = host.MixinState.Get<ComponentHostState>();
    return state.Components.Values.Distinct().ToArray();
  }

  // -----------------------------
  // ADD
  // -----------------------------

  static void AddComponent<T>(Node parent, Node node)
    where T : class, IComponentBase
  {
    if (parent is not IComponentHost host || node is not IComponent component)
    {
      throw new InvalidOperationException();
    }

    var state = host.MixinState.Get<ComponentHostState>();

    component.SetOwner<T>(host);
    parent.AddChild(node);

    RegisterAllTypes(state, component, node);
  }
  
  // -----------------------------
  // REMOVE
  // -----------------------------
  static void RemoveComponent<T>(Node parent)
    where T : class, IComponentBase
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException();
    }

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

    parent.RemoveChild(node);
    node.QueueFree();
  }

  static Type[] GetComponentTypes(Node parent)
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException("Parent is not IComponentHost");
    }

    var state = host.MixinState.Get<ComponentHostState>();
    return state.Components.Keys.ToArray();
  }

  // -----------------------------
  // INTERNAL TYPE SYSTEM
  // -----------------------------
  private static readonly Dictionary<Type, Type[]> _typeCache = new();
  
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
    if (_typeCache.TryGetValue(type, out var cached))
    {
      return cached;
    }

    var list = new List<Type>();

    var current = type;
    while (current != null)
    {
      list.Add(current);

      foreach (var i in current.GetInterfaces())
        list.Add(i);

      current = current.BaseType;
    }

    var result = list.Distinct().ToArray();
    _typeCache[type] = result;
    return result;
  }
}
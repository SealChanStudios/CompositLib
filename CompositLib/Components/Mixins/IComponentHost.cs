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
      throw new InvalidOperationException("Parent is not IComponentHost");

    var state = host.MixinState.Get<ComponentHostState>();

    state.Components.Clear();
    state.ComponentNodes.Clear();

    foreach (var child in parent.GetChildren())
    {
      if (child is not IComponent component)
        continue;

      var registeredType = ComponentTypeResolver.Resolve(component.GetType());

      component.SetOwner(host);

      RegisterAllTypes(state, component, registeredType, child);
    }
  }

  // -----------------------------
  // GET
  // -----------------------------
  static T? GetComponent<T>(Node parent) where T : class, IComponentBase
  {
    if (parent is not IComponentHost host)
      throw new InvalidOperationException("Parent is not IComponentHost");

    var state = host.MixinState.Get<ComponentHostState>();

    if (state.Components.TryGetValue(typeof(T), out var cached))
      return cached as T;

    return null;
  }

  static bool HasComponent<T>(Node parent) where T : class, IComponentBase
  {
    if (parent is not IComponentHost host)
      throw new InvalidOperationException("Parent is not IComponentHost");

    var state = host.MixinState.Get<ComponentHostState>();
    return state.Components.ContainsKey(typeof(T));
  }

  static IComponent[] GetComponents(Node parent)
  {
    if (parent is not IComponentHost host)
      throw new InvalidOperationException("Parent is not IComponentHost");

    var state = host.MixinState.Get<ComponentHostState>();
    return state.Components.Values.Distinct().ToArray();
  }

  // -----------------------------
  // ADD
  // -----------------------------
  static void AddComponent<T>(Node parent, Node node)
    where T : class, IComponentBase
  {
    if (parent is not IComponentHost host)
      throw new InvalidOperationException("Parent is not IComponentHost");

    if (node is not IComponent component)
      throw new InvalidOperationException("Node is not a component");

    var state = host.MixinState.Get<ComponentHostState>();
    var key = ComponentTypeResolver.Resolve(typeof(T));

    if (state.Components.ContainsKey(key))
      throw new InvalidOperationException("Component already exists");

    component.SetOwner<T>(host);

    parent.AddChild(node);

    RegisterAllTypes(state, component, key, node);
  }

  // -----------------------------
  // REMOVE
  // -----------------------------
  static void RemoveComponent<T>(Node parent)
    where T : class, IComponentBase
  {
    if (parent is not IComponentHost host)
      throw new InvalidOperationException("Parent is not IComponentHost");

    var state = host.MixinState.Get<ComponentHostState>();
    var key = typeof(T);

    if (!state.ComponentNodes.TryGetValue(key, out var node))
      throw new InvalidOperationException("Component not found");

    var component = state.Components[key];

    var types = GetAllTypes(component.GetType());

    foreach (var t in types)
    {
      state.Components.Remove(t);
      state.ComponentNodes.Remove(t);
    }

    parent.RemoveChild(node);
    node.QueueFree();
  }

  static Type[] GetComponentTypes(Node parent)
  {
    if (parent is not IComponentHost host)
      throw new InvalidOperationException("Parent is not IComponentHost");

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
    Type registeredType,
    Node node)
  {
    var types = GetAllTypes(component.GetType());

    foreach (var type in types)
    {
      if (!typeof(IComponentBase).IsAssignableFrom(type))
        continue;

      state.Components[type] = component;
      state.ComponentNodes[type] = node;
    }

    // override wins
    state.Components[registeredType] = component;
    state.ComponentNodes[registeredType] = node;
  }

  private static Type[] GetAllTypes(Type type)
  {
    if (_typeCache.TryGetValue(type, out var cached))
      return cached;

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
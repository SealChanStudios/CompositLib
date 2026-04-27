namespace CompositLib.Components.Mixins;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[Mixin]
public interface IComponentHost : IMixin<IComponentHost>,IAutoNode
{
  void IMixin<IComponentHost>.Handler() { }
  new void Handler() => (this as IAutoNode).Handler();

  void IAutoOn.OnReady()
  {
    ((Node)this).RegisterComponents();
    OnHostReady();
  }

  //Called after registering components from scene tree
  // if you overwrite OnReady you have to implement the call your self
  // to register components
  void OnHostReady()
  {

  }

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

      var registeredType = ComponentTypeResolver.Resolve(component.GetType());

      component.SetOwner(host);

      state.Components[registeredType] = component;
      state.ComponentNodes[registeredType] = child;
    }
  }

  static T? GetComponent<T>(Node parent) where T : class, IComponent
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException("Parent is not IComponentHost");
    }

    var state = host.MixinState.Get<ComponentHostState>();

    if (state.Components.TryGetValue(typeof(T), out var cached))
    {
      return cached as T;
    }

    return null;
  }

  static bool HasComponent<T>(Node parent) where T : class, IComponent
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException("Parent is not IComponentHost");
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

    return state.Components.Values.ToArray();
  }

  static void AddComponent<T>(Node parent, T component) where T : Node, IComponent => AddComponent<T>(parent, (Node)component);

  static void AddComponent<T>(Node parent, Node component)
    where T : class, IComponent
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException("Parent is not IComponentHost");
    }

    if (component is not IComponent c)
    {
      return;
    }

    var state = host.MixinState.Get<ComponentHostState>();

    var key = ComponentTypeResolver.Resolve(typeof(T));

    if (state.Components.ContainsKey(key))
    {
      throw new InvalidOperationException("Component already exists");
    }

    c.SetOwner<T>(host);

    parent.AddChild(component);

    state.Components[key] = c;
    state.ComponentNodes[key] = component;
  }

  static void RemoveComponent<T>(Node parent) where T : class, IComponent
  {
    if (parent is not IComponentHost host)
    {
      throw new InvalidOperationException("Parent is not IComponentHost");
    }

    var state = host.MixinState.Get<ComponentHostState>();
    var key = typeof(T);

    if (!state.ComponentNodes.TryGetValue(key, out var node))
    {
      throw new InvalidOperationException("Component not found");
    }

    parent.RemoveChild(node);

    state.ComponentNodes.Remove(key);
    state.Components.Remove(key);
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

}

public class ComponentHostState
{
  public Dictionary<Type, IComponent> Components { get; } = new();
  public Dictionary<Type, Node> ComponentNodes{ get; } = new();
}

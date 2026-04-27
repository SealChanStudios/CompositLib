using System.Runtime.CompilerServices;
using Chickensoft.Introspection;
using CompositLib.Components.Mixins;
using Godot;

namespace CompositLib.Components;

public static class ComponentExtensions
{
  /// <summary>
  ///
  ///
  /// </summary>
  /// <param name="obj">Godot Node.</param>
  public static T? GetComponent<T>(this Node obj) where T : class, IComponent
  {
    obj.__SetupIComponentHostStateIfNeeded();

    if (obj is not IComponentHost)
    {
      throw new InvalidOperationException("Node must implement IComponentHost to use GetComponent.");
    }

    return IComponentHost.GetComponent<T>(obj);
  }

  public static bool HasComponent<T>(this Node obj) where T : class, IComponent
  {
    obj.__SetupIComponentHostStateIfNeeded();

    if (obj is not IComponentHost)
    {
      throw new InvalidOperationException("Node must implement IComponentHost to use GetComponent.");
    }
    return IComponentHost.HasComponent<T>(obj);
  }

  public static IComponent[] GetComponents(this Node obj)
  {
    obj.__SetupIComponentHostStateIfNeeded();
    if (obj is not IComponentHost)
    {
      throw new InvalidOperationException("Node must implement IComponentHost to use GetComponent.");
    }
    return IComponentHost.GetComponents(obj);
  }


  //prints the actual type of the component irrelevant of what it says it is in the
  //Component registry
  public static void PrintComponents(this Node host)
  {
    host.__SetupIComponentHostStateIfNeeded();

    if (host is not IComponentHost)
    {
      throw new InvalidOperationException("Node must implement IComponentHost to use PrintComponents.");
    }

    var components = IComponentHost.GetComponents(host);

    foreach (var component in components)
    {
      GD.Print(component.GetRuntimeType());
    }
  }

  //prints types of components as registered in the component registry
  public static void PrintComponentsType(this Node host)
  {
    host.__SetupIComponentHostStateIfNeeded();

    var components = IComponentHost.GetComponentTypes(host);

    foreach (var component in components)
    {
      GD.Print(component);
    }
  }

  public static void AddComponent<T>(this Node obj, Node component) where T : class, IComponent
  {
    obj.__SetupIComponentHostStateIfNeeded();
    IComponentHost.AddComponent<T>(obj, component);
  }
  public static void AddComponent<T>(this Node obj, T component) where T : Node, IComponent
  {
    obj.__SetupIComponentHostStateIfNeeded();
    IComponentHost.AddComponent(obj, component);
  }

  public static void RemoveComponent<T>(this Node obj) where T : class, IComponent
  {
    obj.__SetupIComponentHostStateIfNeeded();
    IComponentHost.RemoveComponent<T>(obj);
  }

  public static void RegisterComponents(this Node obj)
  {
    obj.__SetupIComponentHostStateIfNeeded();
    IComponentHost.RegisterComponents(obj);
  }


#pragma warning disable IDE1006
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void __SetupIComponentHostStateIfNeeded(this Node obj)
  {
    if (obj is not IIntrospectiveRef introspective)
    {
      return;
    }

    if (!introspective.MixinState.Has<IComponentHostState>())
    {
      introspective.MixinState.Overwrite(new IComponentHostState());
    }
  }


  // -------- COMPONENT --------

  public static void SetOwner<TComponent>(this IComponent component, IComponentHost host) where TComponent : class, IComponent
  {
    component.__SetupIComponentStateIfNeeded<TComponent>(host);
    var state = component.MixinState.Get<ComponentState>();
    state.ComponentOwner = host;
  }

  public static void SetOwner(this IComponent component, IComponentHost host)
  {
    if (component is not IIntrospectiveRef introspective)
    {
      return;
    }

    var registered = ComponentTypeResolver.Resolve(component.GetType());

    if (!introspective.MixinState.Has<ComponentState>())
    {
      introspective.MixinState.Overwrite(
        new ComponentState(host, registered)
      );
    }

    var state = introspective.MixinState.Get<ComponentState>();
    state.ComponentOwner = host;
    state.ComponentType = registered;
  }

  public static Type GetRuntimeType(this IComponent component) => component.GetType();

  public static Type GetRegisteredType(this IComponent component)
  {
    var state = component.MixinState.Get<ComponentState>();
    return state.ComponentType;
  }

  public static IComponentHost GetOwner(this IComponent component)
  {
    var state = component.MixinState.Get<ComponentState>();
    return state.ComponentOwner;
  }

#pragma warning disable IDE1006
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void __SetupIComponentStateIfNeeded<T>(this IComponent obj,IComponentHost componentHost) where T : IComponent
  {
    if (obj is not IIntrospectiveRef introspective)
    {
      return;
    }

    if (!introspective.MixinState.Has<ComponentState>())
    {
      introspective.MixinState.Overwrite(new ComponentState(componentHost, typeof(T)));
    }
  }
}

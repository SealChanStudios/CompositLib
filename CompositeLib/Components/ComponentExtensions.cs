using System.Runtime.CompilerServices;
using Chickensoft.Introspection;
using CompositeLib.Components.Mixins;
using Godot;

namespace CompositeLib.Components;

public static class ComponentExtensions
{
  public static T? GetComponent<T>(this IComponentHost obj) where T : class, IComponentBase
  {
    obj.__SetupIComponentHostStateIfNeeded();
    return IComponentHost.GetComponent<T>(obj);
  }

  public static void Setup(this Node obj,int what)
  {
    obj.__SetupIComponentHostStateIfNeeded();
    IComponentHost.InvokeNotificationMethods(obj,what);
  }

  public static bool HasComponent<T>(this IComponentHost obj) where T : class, IComponentBase
  {
    obj.__SetupIComponentHostStateIfNeeded();
    return IComponentHost.HasComponent<T>(obj);
  }

  public static IComponent[] GetComponents(this IComponentHost obj)
  {
    obj.__SetupIComponentHostStateIfNeeded();
    return IComponentHost.GetComponents(obj);
  }


  //prints the actual type of the component irrelevant of what it says it is in the
  //Component registry
  public static void PrintComponents(this IComponentHost host)
  {
    host.__SetupIComponentHostStateIfNeeded();

    var components = IComponentHost.GetComponents(host);

    foreach (var component in components)
    {
      GD.Print(component.GetRuntimeType());
    }
  }

  //prints types of components as registered in the component registry
  public static void PrintComponentsType(this IComponentHost host)
  {
    host.__SetupIComponentHostStateIfNeeded();

    var components = IComponentHost.GetComponentTypes(host);

    foreach (var component in components)
    {
      GD.Print(component);
    }
  }

  public static void AddComponent<T>(this IComponentHost obj, Node component) where T : class, IComponentBase
  {
    obj.__SetupIComponentHostStateIfNeeded();
    IComponentHost.AddComponent<T>(obj, component);
  }
  public static void AddComponent<T>(this IComponentHost obj, T component) where T : Node,IComponentBase
  {
    obj.__SetupIComponentHostStateIfNeeded();
    IComponentHost.AddComponent<T>(obj, component);
  }

  public static void RemoveComponent<T>(this IComponentHost obj) where T : class, IComponentBase
  {
    obj.__SetupIComponentHostStateIfNeeded();
    IComponentHost.RemoveComponent<T>(obj);
  }

  public static void RegisterComponents(this IComponentHost obj)
  {
    obj.__SetupIComponentHostStateIfNeeded();
    IComponentHost.RegisterComponents(obj);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void __SetupIComponentHostStateIfNeeded(this Node obj)
  {
    if (obj is not IComponentHost host)
    {
      throw new ArgumentNullException($"can't setup component host state on non component host on null object");
    }
    // ReSharper disable once SuspiciousTypeConversion.Global
    host.__SetupIComponentHostStateIfNeeded();
  }
  
  
#pragma warning disable IDE1006
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void __SetupIComponentHostStateIfNeeded(this IComponentHost obj)
  {
    
    if (obj is not IIntrospectiveRef introspective)
    {
      return;
    }

    if (!introspective.MixinState.Has<ComponentHostState>())
    {
      introspective.MixinState.Overwrite(new ComponentHostState());
    }
  }


  // -------- COMPONENT --------

  public static void SetOwner<TComponent>(this IComponentBase component, IComponentHost host) where TComponent : class, IComponentBase
  {
    (component as IComponent)?.__SetupIComponentStateIfNeeded<TComponent>(host);
    (component as IComponent)?.MixinState.Get<ComponentState>();
    IComponent.SetOwner(component, host);
  }

  public static void SetOwner(this IComponentBase component, IComponentHost host)
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
    IComponent.SetOwner(component, host);
  }

  public static Type GetRuntimeType(this IComponentBase component) => component.GetType();

  public static Type[] GetRegisteredType(this IComponentBase component)
  {
    var state =  (component as IComponent)?.MixinState.Get<ComponentState>();
    return state!.ComponentTypes;
  }

  public static IComponentHost GetOwner(this IComponentBase component)
  {
    var state = (component as IComponent)?.MixinState.Get<ComponentState>();
    return state!.ComponentOwner;
  }

#pragma warning disable IDE1006
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void __SetupIComponentStateIfNeeded<T>(this IComponentBase obj,IComponentHost componentHost) where T : IComponentBase
  {
    if (obj is not IIntrospectiveRef introspective)
    {
      return;
    }

    if (!introspective.MixinState.Has<ComponentState>())
    {
      introspective.MixinState.Overwrite(new ComponentState(componentHost, ComponentTypeResolver.Resolve(typeof(T))));
    }
  }
}

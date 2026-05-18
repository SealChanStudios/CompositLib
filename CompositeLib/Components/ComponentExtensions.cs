using System.Runtime.CompilerServices;
using Chickensoft.Introspection;
using CompositeLib.Components.Mixins;
using Godot;

namespace CompositeLib.Components;

public static partial class ComponentExtensions
{
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

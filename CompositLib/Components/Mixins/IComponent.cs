using Chickensoft.AutoInject;
using Chickensoft.Introspection;

namespace CompositLib.Components.Mixins;

[Mixin]
public interface IComponent : IMixin<IComponent>, IAutoNode, IComponentBase
{
  void IMixin<IComponent>.Handler() { }

  new void Handler()
  {
    (this as IAutoNode).Handler();
    (this as IComponentBase).Handler();
  }

  static IComponentHost GetOwner(IComponent component)
  {
    var state = component.MixinState.Get<ComponentState>();
    return state.ComponentOwner;
  }

  static Type GetRegisteredType(IComponent component)
  {
    var state = component.MixinState.Get<ComponentState>();
    return state.ComponentType;
  }

  static void SetOwner<T>(IComponent component, IComponentHost owner)
    where T : class, IComponentBase
  {
    var state = component.MixinState.Get<ComponentState>();
    var oldOwner = state.ComponentOwner;

    state.ComponentOwner = owner;
    state.ComponentType = typeof(T);

    component.OnOwnershipTransferred(oldOwner);
  }

  void OnOwnershipTransferred(IComponentHost oldOwner) { }
}
using Chickensoft.AutoInject;
using Chickensoft.Introspection;

namespace CompositLib.Components.Mixins;

[Mixin]
public interface IComponent : IMixin<IComponent>,IAutoNode
{
  void IMixin<IComponent>.Handler() { }
  new void Handler() => (this as IAutoNode).Handler();

  static Type ComponentType(IComponent component) => component.GetType();

  static IComponentHost ComponentOwner(IComponent component)
  {
    var state = component.MixinState.Get<ComponentState>();
    return state.ComponentOwner;
  }

  static Type GetComponentType(IComponent component)
  {
    var state = component.MixinState.Get<ComponentState>();
    return state.ComponentType;
  }

  static void SetComponentOwner<T>(IComponent component, IComponentHost owner) where T : IComponent
  {
    var state = component.MixinState.Get<ComponentState>();
    var oldOwner = ComponentOwner(component);
    state.ComponentOwner = owner;
    state.ComponentType = typeof(T);
    component.OnOwnershipTransferred(oldOwner);
    
  }

  void OnOwnershipTransferred(IComponentHost host) { }
}

public class ComponentState(IComponentHost componentOwner, Type componentType)
{
  public IComponentHost ComponentOwner { get; set; } = componentOwner;
  public Type ComponentType { get; set; } = componentType;
}

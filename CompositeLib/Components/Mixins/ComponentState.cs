namespace CompositeLib.Components.Mixins;

public class ComponentState(IComponentHost owner, Type[] types)
{
    public IComponentHost ComponentOwner { get; set; } = owner;
    public Type[] ComponentTypes { get; set; } = types;
}
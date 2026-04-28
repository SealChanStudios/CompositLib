namespace CompositLib.Components.Mixins;

public class ComponentState(IComponentHost owner, Type type)
{
    public IComponentHost ComponentOwner { get; set; } = owner;
    public Type ComponentType { get; set; } = type;
}
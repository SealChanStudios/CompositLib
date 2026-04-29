using Godot;

namespace CompositeLib.Components.Mixins;

public class ComponentHostState
{
    public Dictionary<Type, IComponent> Components { get; } = new();
    public Dictionary<Type, Node> ComponentNodes { get; } = new();
}
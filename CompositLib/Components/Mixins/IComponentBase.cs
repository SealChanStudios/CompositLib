using Chickensoft.Introspection;

namespace CompositLib.Components.Mixins;


[Mixin]
public interface IComponentBase : IMixin<IComponentBase>
{
    void IMixin<IComponentBase>.Handler()
    {
        
    }
}
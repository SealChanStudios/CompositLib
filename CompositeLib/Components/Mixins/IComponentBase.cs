using Chickensoft.Introspection;

namespace CompositeLib.Components.Mixins;


[Mixin]
public interface IComponentBase : IMixin<IComponentBase>
{
    void IMixin<IComponentBase>.Handler()
    {
        
    }
    void OnOwnershipTransferred(IComponentHost oldOwner) { }
}
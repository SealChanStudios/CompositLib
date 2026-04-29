namespace CompositeLib.Components;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public sealed class RegisterAsBaseAttribute : Attribute
{
    public Type[] Types { get; }

    public RegisterAsBaseAttribute(params Type[] type)
    {
        Types = type;
    }
}

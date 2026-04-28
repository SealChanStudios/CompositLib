namespace CompositLib.Components;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RegisterAsBaseAttribute : Attribute
{
    public Type[] Types { get; }

    public RegisterAsBaseAttribute(params Type[] type)
    {
        Types = type;
    }
}

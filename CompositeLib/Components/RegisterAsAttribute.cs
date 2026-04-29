namespace CompositeLib.Components;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RegisterAsAttribute : Attribute
{
  public Type[] Types { get; }

  public RegisterAsAttribute(params Type[] type)
  {
    Types = type;
  }
}

namespace CompositLib.Components;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RegisterAsAttribute : Attribute
{
  public Type Type { get; }

  public RegisterAsAttribute(Type type)
  {
    Type = type;
  }
}

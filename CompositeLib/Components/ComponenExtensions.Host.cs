using CompositeLib.Components.Mixins;
using Godot;

namespace CompositeLib.Components;

public static partial class ComponentExtensions
{
  public static T? GetHostComponent<T>(this IComponentBase obj) where T : class, IComponentBase
  {
    return obj.GetOwner().GetComponent<T>();
  }

  public static bool HostHasComponent<T>(this IComponentBase obj) where T : class, IComponentBase
  {
    return obj.GetOwner().HasComponent<T>();
  }

  public static IComponent[] GetHostComponents(this IComponentBase obj)
  {
    return obj.GetOwner().GetComponents();
  }
  

  public static void AddHostComponent<T>(this IComponentBase obj, Node component) where T : class, IComponentBase
  {
    obj.GetOwner().AddComponent<T>(component);
  }
  public static void AddHostComponent<T>(this IComponentBase obj, T component) where T : Node,IComponentBase
  {
    obj.GetOwner().AddComponent(component);
  }

  public static void RemoveHostComponent<T>(this IComponentBase obj) where T : class, IComponentBase
  {
    obj.GetOwner().RemoveComponent<T>();
  }
}
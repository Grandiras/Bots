using System.Reflection;

namespace TenBot.Helpers;
public static class ReflectionHelpers
{
    public static object? InvokeGenericMethod(this object instance, string methodName, Type type, bool isPublic, params object?[] parameters)
        => instance.GetType()
            .GetMethods(BindingFlags.Instance | (isPublic ? BindingFlags.Public : BindingFlags.NonPublic))
            .First(x => x.Name == methodName && x.IsGenericMethod && x.GetParameters().Length == parameters.Length)
            .MakeGenericMethod(type)
            .Invoke(instance, parameters);
}

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
    public static object? InvokeGenericStaticMethod(this object instance, string methodName, Type type, bool isPublic, params object?[] parameters)
        => instance.GetType()
            .GetMethods(BindingFlags.Static | (isPublic ? BindingFlags.Public : BindingFlags.NonPublic))
            .First(x => x.Name == methodName && x.IsGenericMethod && x.GetParameters().Length == parameters.Length)
            .MakeGenericMethod(type)
            .Invoke(instance, parameters);

    public static object? CastToGeneric(this object obj, Type type)
    {
        var method = typeof(ReflectionHelpers).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.NonPublic);
        var generic = method!.MakeGenericMethod(type);
        return generic.Invoke(null, [obj]);
    }
    private static TOther CastTo<TOther>(object obj) => (TOther)obj;
}

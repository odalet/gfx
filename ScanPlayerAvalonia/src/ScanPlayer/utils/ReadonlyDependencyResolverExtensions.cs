using System;
using Splat;

namespace ScanPlayer
{
    internal static class ReadonlyDependencyResolverExtensions
    {
        public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver)
        {
            var service = resolver.GetService<T>();
            return service is null
                ? throw new InvalidOperationException($"Failed to resolve object of type {typeof(T)}")
                : service;
        }

        public static object GetRequiredService(this IReadonlyDependencyResolver resolver, Type type)
        {
            var service = resolver.GetService(type);
            return service is null
                ? throw new InvalidOperationException($"Failed to resolve object of type {type}")
                : service;
        }
    }
}

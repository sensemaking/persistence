using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sensemaking.Persistenc.Query
{
    public class TypeScanner
    {
        private static readonly Dictionary<Type, Type> TypeCache = new Dictionary<Type, Type>();

        public static T Get<T>() where T : class
        {
            if (TypeCache.ContainsKey(typeof(T)))
                return (Activator.CreateInstance(TypeCache[typeof(T)]) as T)!;

            lock (TypeCache)
            {
                if (TypeCache.ContainsKey(typeof(T)))
                    return (Activator.CreateInstance(TypeCache[typeof(T)]) as T)!;

                TypeCache.Add(typeof(T), GetImplementingType<T>());
                return (Activator.CreateInstance(TypeCache[typeof(T)]) as T)!;
            }
        }

        private static Type GetImplementingType<T>()
        {
            var implementors = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetPublicTypes)
                .Where(type => type.GetInterfaces().Any(i => i.FullName == typeof(T).FullName))
                .ToList();

            if (implementors.Count != 1)
                throw new Exception($"Unable to resolve to a single type implementing {typeof(T).Name}");

            return implementors.Single();
        }

        private static Type[] GetPublicTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException || ex is NotSupportedException)
                    return new Type[] { };

                throw;
            }
        }
    }
}

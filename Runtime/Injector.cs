using System.Linq;
using System.Reflection;
using Baruah.Nexus.Attributes;

namespace Baruah.Nexus
{
    public static class Injector
    {
        const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static void Inject(object target, SceneInstaller installer)
        {
            Inject(target, installer.GetContainer());
        }
        
        public static void Inject(object target, DiContainer container)
        {
            var type = target.GetType();

            // Field injection
            foreach (var field in type.GetFields(FLAGS))
                if (field.GetCustomAttribute<InjectAttribute>() != null)
                    field.SetValue(target, container.Resolve(field.FieldType));

            // Property injection
            foreach (var prop in type.GetProperties(FLAGS))
                if (prop.CanWrite && prop.GetCustomAttribute<InjectAttribute>() != null)
                    prop.SetValue(target, container.Resolve(prop.PropertyType));

            // Method injection
            foreach (var method in type.GetMethods(FLAGS))
                if (method.GetCustomAttribute<InjectMethodAttribute>() != null)
                {
                    var args = method.GetParameters()
                        .Select(p => container.Resolve(p.ParameterType))
                        .ToArray();
                    method.Invoke(target, args);
                }
        }
    }
}

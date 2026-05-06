using System;

namespace Baruah.Nexus.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class InjectMethodAttribute : Attribute { }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class InjectableAttribute : Attribute { }
}

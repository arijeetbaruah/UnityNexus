using System;
using System.Collections.Generic;
using Baruah.Nexus.Exception;

namespace Baruah.Nexus
{
    public sealed class DiContainer
    {
        private readonly Dictionary<Type, object> _singletons = new();

        public void Bind<TInterface>(TInterface instance) where TInterface : class
        {
            _singletons[typeof(TInterface)] = instance;
        }

        public void Bind(System.Type type, object instance)
        {
            _singletons[type] = instance;
        }

        public void UnBind<TInterface>() where TInterface : class
        {
            _singletons.Remove(typeof(TInterface));
        }
        
        public T Resolve<T>() => (T)Resolve(typeof(T));

        public object Resolve(Type type)
        {
            if (_singletons.TryGetValue(type, out var obj)) return obj;
            
            throw new NoBindingException(type);
        }
    }
}

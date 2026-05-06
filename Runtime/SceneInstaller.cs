using System;
using System.Collections.Generic;
using UnityEngine;

namespace Baruah.Nexus
{
    public class SceneInstaller : MonoBehaviour
    {
        private DiContainer _container;
        
        [SerializeField] private InjectorDatum[] _injectorData;
        
        private void Awake()
        {
            _container = new DiContainer();

            foreach (var datum in _injectorData)
            {
                _container.Bind(datum.ClassType, datum.ToObject());
            }

            Inject(GetGameObjectsAtStart());
        }
        
        public DiContainer GetContainer() => _container;

        private void Inject(IReadOnlyList<MonoBehaviour> gameObjects)
        {
            foreach (var go in gameObjects)
            {
                Injector.Inject(go, _container);
            }
        }

        private IReadOnlyList<MonoBehaviour> GetGameObjectsAtStart()
        {
            return GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        }
    }

    [System.Serializable]
    public struct InjectorDatum
    {
        public string className;
        public ParameterDatum[] parameters;

        public System.Type ClassType => Type.GetType(className);
        public object ToObject()
        {
            object[] param = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                param[i] = parameters[i].ToObject();
            }
            
            return Activator.CreateInstance(ClassType, param);
        }
    }
    
    public enum ParameterSourceKind
    {
        Primitive,
        UnityObject,
        CustomClass,
        Injectable  // resolved from container at runtime
    }
 
    [Serializable]
    public class ParameterDatum
    {
        public string parameterName;
        public string typeName;               // AssemblyQualifiedName
        public ParameterSourceKind sourceKind;
 
        // Primitive values — only one is used depending on typeName
        public string  stringValue;
        public int     intValue;
        public float   floatValue;
        public bool    boolValue;
        public Color    colorValue;
        public ParameterDatum[] childParameters;
 
        // Unity objects (GameObject, Component, ScriptableObject, etc.)
        [SerializeField] public UnityEngine.Object unityObjectValue;

        public object ToObject()
        {
            switch (sourceKind)
            {
                case ParameterSourceKind.UnityObject:
                    return unityObjectValue;
 
                case ParameterSourceKind.Primitive:
                    Type type = Type.GetType(typeName);
                    if (type == typeof(int))    return intValue;
                    if (type == typeof(float))  return floatValue;
                    if (type == typeof(bool))   return boolValue;
                    if (type == typeof(string)) return stringValue;
                    if (type == typeof(Color)) return colorValue;
                    Debug.LogError($"[NexusInjector] Unsupported primitive type: {typeName}");
                    return null;
 
                case ParameterSourceKind.CustomClass:
                    return ConstructCustomClass();
 
                case ParameterSourceKind.Injectable:
                    Debug.LogError($"[NexusInjector] Parameter '{parameterName}' must be resolved from the container.");
                    return null;
 
                default:
                    Debug.LogError($"[NexusInjector] Unknown ParameterSourceKind on '{parameterName}'.");
                    return null;
            }
        }
        
        private object ConstructCustomClass()
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                Debug.LogError($"[NexusInjector] Could not resolve type: {typeName}");
                return null;
            }
 
            object instance = Activator.CreateInstance(type);
 
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            for (int i = 0; i < fields.Length && i < childParameters.Length; i++)
            {
                object value = childParameters[i].ToObject();
                if (value != null)
                    fields[i].SetValue(instance, value);
            }
 
            return instance;
        }
    }
}

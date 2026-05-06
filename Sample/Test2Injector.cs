using Baruah.Nexus.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Baruah.Nexus.Sample
{
    [Injectable]
    public class Test2Injectable
    {
        private InputActionAsset _go;
        private string _name;
        
        public Test2Injectable(InputActionAsset go, string name)
        {
            _go = go;
            _name = name;
            
            Debug.Log("Test2Injectable");
        }
        
        public void DoSomething()
        {
            Debug.Log($"Hello {_name}!");
        }
    }
}

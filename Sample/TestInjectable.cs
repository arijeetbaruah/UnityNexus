using Baruah.Nexus.Attributes;
using UnityEngine;

namespace Baruah.Nexus.Sample
{
    [Injectable]
    public class TestInjectable
    {
        private GameObject _go;
        
        public TestInjectable(GameObject go)
        {
            _go = go;
        }
        
        public void DoSomething()
        {
            GameObject.Instantiate(_go);
        }
    }
}

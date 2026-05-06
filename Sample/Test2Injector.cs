using Baruah.Nexus.Attributes;
using UnityEngine;

namespace Baruah.Nexus.Sample
{
    [Injectable]
    public class Test2Injectable
    {
        private Color _color;
        
        public Test2Injectable(Color color)
        {
            _color = color;
        }
        
        public void DoSomething(Material material)
        {
            material.color = _color;
        }
    }
}

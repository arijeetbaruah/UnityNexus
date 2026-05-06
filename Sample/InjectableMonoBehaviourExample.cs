using System;
using Baruah.Nexus;
using Baruah.Nexus.Attributes;
using Baruah.Nexus.Sample;
using UnityEngine;

public class InjectableMonoBehaviourExample : InjectableMonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    
    [Inject]
    public Test2Injectable Test;

    private void Start()
    {
        Material material = new Material(_renderer.material);
        _renderer.material = material;
        
        Test.DoSomething(material);
    }
}

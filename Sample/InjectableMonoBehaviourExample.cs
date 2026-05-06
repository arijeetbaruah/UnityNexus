using Baruah.Nexus;
using Baruah.Nexus.Attributes;
using Baruah.Nexus.Sample;
using UnityEngine;

public class InjectableMonoBehaviourExample : InjectableMonoBehaviour
{
    [InjectMethod]
    public void SetInjected(Test2Injectable test)
    {
        test.DoSomething();
    }
}

using Baruah.Nexus.Attributes;
using Baruah.Nexus.Sample;
using UnityEngine;

public class TestMono : MonoBehaviour
{
    [InjectMethod]
    public void SetTestInjector(TestInjectable testInjector)
    {
        testInjector.DoSomething();
    }
}

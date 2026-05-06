using System;
using UnityEngine;

namespace Baruah.Nexus
{
    public class InjectableMonoBehaviour : MonoBehaviour
    {
        public virtual void Awake()
        {
            var installers = GameObject.FindObjectsByType<SceneInstaller>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            foreach (var installer in installers)
            {
                Injector.Inject(this, installer);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.XR.Management;

namespace MageWorkshop
{
    public class SimulatorBootstrap : MonoBehaviour
    {
        public GameObject simulatorPrefab;

        void Start()
        {
            if (Application.isEditor && XRGeneralSettings.Instance.Manager.activeLoader == null)
                Instantiate(simulatorPrefab);
        }
    }
}

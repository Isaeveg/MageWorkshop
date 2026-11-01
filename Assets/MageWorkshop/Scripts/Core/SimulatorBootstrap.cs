using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace MageWorkshop
{
    public class SimulatorBootstrap : MonoBehaviour
    {
        public GameObject simulatorPrefab;
        [Tooltip("Ile sekund czekać na wykrycie gogli przed włączeniem symulatora.")]
        public float headsetWaitTime = 1.5f;

        IEnumerator Start()
        {
            if (!Application.isEditor)
                yield break;

            var devices = new List<InputDevice>();
            for (float waited = 0f; waited < headsetWaitTime; waited += Time.unscaledDeltaTime)
            {
                InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);
                if (devices.Count > 0)
                    yield break;
                yield return null;
            }

            Instantiate(simulatorPrefab);
        }
    }
}

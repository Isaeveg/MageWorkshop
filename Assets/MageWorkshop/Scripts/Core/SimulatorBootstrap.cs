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
        [Tooltip("Włącz symulator nawet przy podłączonych goglach.")]
        public bool forceSimulator;
        [Tooltip("Pozwól na symulator także w buildzie (domyślnie tylko w edytorze).")]
        public bool allowInBuild;

        IEnumerator Start()
        {
            if (simulatorPrefab == null || (!Application.isEditor && !allowInBuild))
                yield break;

            if (!forceSimulator)
            {
                var devices = new List<InputDevice>();
                float waited = 0f;
                while (waited < headsetWaitTime)
                {
                    InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);
                    if (devices.Count > 0)
                    {
                        Debug.Log($"[MageWorkshop] Wykryto gogle: {devices[0].name}. Symulator wyłączony.");
                        yield break;
                    }
                    waited += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            Debug.Log("[MageWorkshop] Nie wykryto gogli — uruchamiam XR Interaction Simulator.");
            var simulator = Instantiate(simulatorPrefab);
            simulator.name = simulatorPrefab.name;
        }
    }
}

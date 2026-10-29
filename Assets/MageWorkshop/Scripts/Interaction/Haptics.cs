using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MageWorkshop
{
    public static class Haptics
    {
        public static void Pulse(XRBaseInteractable interactable, float amplitude, float duration)
        {
            if (interactable == null)
                return;

            foreach (var interactor in interactable.interactorsSelecting)
            {
                if (interactor is XRBaseInputInteractor inputInteractor)
                    inputInteractor.SendHapticImpulse(amplitude, duration);
            }
        }

        public static void PulseAll(float amplitude, float duration)
        {
            foreach (var interactor in Object.FindObjectsByType<XRBaseInputInteractor>(FindObjectsSortMode.None))
                interactor.SendHapticImpulse(amplitude, duration);
        }
    }
}

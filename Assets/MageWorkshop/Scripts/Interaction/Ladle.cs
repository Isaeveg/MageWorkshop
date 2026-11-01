using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MageWorkshop
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Ladle : MonoBehaviour
    {
        [Tooltip("Środek czerpaka — ten punkt musi zataczać koła w kotle.")]
        public Transform tip;

        public XRGrabInteractable Grab { get; private set; }

        public bool IsHeld => Grab.isSelected && !(Grab.firstInteractorSelecting is XRSocketInteractor);

        void Awake()
        {
            Grab = GetComponent<XRGrabInteractable>();
        }
    }
}

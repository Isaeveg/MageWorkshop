using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MageWorkshop
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Ladle : MonoBehaviour
    {
        static readonly List<Ladle> s_Active = new List<Ladle>();
        public static IReadOnlyList<Ladle> Active => s_Active;

        [Tooltip("Środek czerpaka — ten punkt musi zataczać koła w kotle.")]
        public Transform tip;

        public XRGrabInteractable Grab { get; private set; }

        public bool IsHeld => Grab.isSelected && !(Grab.firstInteractorSelecting is XRSocketInteractor);

        void Awake()
        {
            Grab = GetComponent<XRGrabInteractable>();
            if (tip == null)
                tip = transform;
        }

        void OnEnable() => s_Active.Add(this);
        void OnDisable() => s_Active.Remove(this);

        public void Pulse(float amplitude, float duration) => Haptics.Pulse(Grab, amplitude, duration);
    }
}

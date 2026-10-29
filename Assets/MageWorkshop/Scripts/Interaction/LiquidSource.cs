using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MageWorkshop
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class LiquidSource : MonoBehaviour
    {
        public IngredientDefinition liquid;
        [Tooltip("Punkt na szyjce butelki, z którego leci strumień.")]
        public Transform spout;
        [Tooltip("Maksymalny przepływ w ml/s (przy butelce odwróconej do góry dnem).")]
        public float flowRate = 120f;
        [Tooltip("Kąt odchylenia od pionu (stopnie), od którego zaczyna się nalewanie.")]
        public float pourAngle = 75f;
        public float maxStreamLength = 1.5f;
        public LineRenderer stream;

        XRGrabInteractable m_Grab;
        float m_NextHapticTime;

        void Awake()
        {
            m_Grab = GetComponent<XRGrabInteractable>();
            if (stream != null)
            {
                stream.positionCount = 2;
                stream.enabled = false;
                if (liquid != null)
                    stream.startColor = stream.endColor = liquid.color;
            }
        }

        void Update()
        {
            float tilt = Vector3.Angle(transform.up, Vector3.up);
            bool pouring = IsHeldByHand() && tilt > pourAngle;

            if (stream != null)
                stream.enabled = pouring;

            if (!pouring)
                return;

            float flow = flowRate * Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(pourAngle, 160f, tilt));
            var origin = spout != null ? spout.position : transform.position;
            var end = origin + Vector3.down * maxStreamLength;

            if (Physics.Raycast(origin, Vector3.down, out var hit, maxStreamLength, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            {
                end = hit.point;
                var cauldron = hit.collider.GetComponentInParent<Cauldron>();
                if (cauldron != null)
                    cauldron.AddLiquid(liquid, flow * Time.deltaTime);
            }

            if (stream != null)
            {
                stream.SetPosition(0, origin);
                stream.SetPosition(1, end);
            }

            if (Time.time >= m_NextHapticTime)
            {
                Haptics.Pulse(m_Grab, 0.15f, 0.1f);
                m_NextHapticTime = Time.time + 0.1f;
            }
        }

        bool IsHeldByHand()
        {
            return m_Grab.isSelected && !(m_Grab.firstInteractorSelecting is XRSocketInteractor);
        }
    }
}

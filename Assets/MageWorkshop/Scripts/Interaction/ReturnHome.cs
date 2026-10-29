using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MageWorkshop
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ReturnHome : MonoBehaviour
    {
        [Tooltip("Po ilu sekundach leżenia poza zasięgiem przedmiot wraca.")]
        public float returnDelay = 2f;
        [Tooltip("Poniżej tej wysokości (m) przedmiot uznaje się za leżący na podłodze.")]
        public float floorHeight = 0.2f;
        [Tooltip("Odległość w poziomie od głowy gracza (m), powyżej której przedmiot jest poza zasięgiem rąk.")]
        public float reachRadius = 1.2f;

        XRGrabInteractable m_Grab;
        Rigidbody m_Body;
        Vector3 m_HomePosition;
        Quaternion m_HomeRotation;
        float m_OutOfReachTime;

        void Awake()
        {
            m_Grab = GetComponent<XRGrabInteractable>();
            m_Body = GetComponent<Rigidbody>();
            m_HomePosition = transform.position;
            m_HomeRotation = transform.rotation;
        }

        void Update()
        {
            if (m_Grab.isSelected || !IsOutOfReach())
            {
                m_OutOfReachTime = 0f;
                return;
            }

            m_OutOfReachTime += Time.deltaTime;
            if (m_OutOfReachTime >= returnDelay)
                SendHome();
        }

        bool IsOutOfReach()
        {
            if (transform.position.y < floorHeight)
                return true;

            var head = Camera.main;
            if (head == null)
                return false;

            var offset = transform.position - head.transform.position;
            offset.y = 0f;
            return offset.magnitude > reachRadius;
        }

        public void SendHome()
        {
            m_OutOfReachTime = 0f;
            if (m_Body != null)
            {
                if (!m_Body.isKinematic)
                {
                    m_Body.linearVelocity = Vector3.zero;
                    m_Body.angularVelocity = Vector3.zero;
                }
                m_Body.position = m_HomePosition;
                m_Body.rotation = m_HomeRotation;
            }
            transform.SetPositionAndRotation(m_HomePosition, m_HomeRotation);
        }
    }
}

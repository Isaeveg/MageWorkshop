using Unity.XR.CoreUtils;
using UnityEngine;

namespace MageWorkshop
{
    [RequireComponent(typeof(XROrigin))]
    public class PlayerCollision : MonoBehaviour
    {
        [Tooltip("Promień ciała gracza (m).")]
        public float radius = 0.15f;
        [Tooltip("Wysokość nad podłogą, od której zaczyna się ciało (m) — niżej przedmioty nie blokują gracza.")]
        public float feetClearance = 0.3f;
        public LayerMask layers = Physics.DefaultRaycastLayers;

        XROrigin m_Origin;
        CapsuleCollider m_Body;
        readonly Collider[] m_Hits = new Collider[32];

        void Awake()
        {
            m_Origin = GetComponent<XROrigin>();
            var body = new GameObject("Player Body");
            body.transform.SetParent(transform, false);
            m_Body = body.AddComponent<CapsuleCollider>();
            m_Body.isTrigger = true;
            m_Body.radius = radius;
        }

        void LateUpdate()
        {
            var head = m_Origin.Camera.transform.position;
            float top = head.y;
            float bottom = Mathf.Min(m_Origin.Origin.transform.position.y + feetClearance, top - radius * 2f);
            var center = new Vector3(head.x, (top + bottom) / 2f, head.z);

            m_Body.transform.SetPositionAndRotation(center, Quaternion.identity);
            m_Body.radius = radius;
            m_Body.height = top - bottom;

            var push = Vector3.zero;
            int count = Physics.OverlapCapsuleNonAlloc(
                new Vector3(head.x, bottom + radius, head.z), new Vector3(head.x, top - radius, head.z),
                radius, m_Hits, layers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var hit = m_Hits[i];
                if (hit == m_Body || hit.attachedRigidbody != null || hit.transform.IsChildOf(transform))
                    continue;

                if (Physics.ComputePenetration(m_Body, center + push, Quaternion.identity, hit, hit.transform.position, hit.transform.rotation, out var direction, out var distance))
                {
                    direction.y = 0f;
                    push += direction * distance;
                }
            }

            if (push.sqrMagnitude > 1e-8f)
                m_Origin.Origin.transform.position += push;
        }
    }
}

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MageWorkshop
{
    [RequireComponent(typeof(NearFarInteractor))]
    public class GrabDistanceControl : MonoBehaviour
    {
        [Tooltip("Prędkość przybliżania i oddalania trzymanego przedmiotu (m/s).")]
        public float speed = 0.6f;
        [Tooltip("Maksymalna odległość przedmiotu od dłoni (m).")]
        public float maxDistance = 1f;

        NearFarInteractor m_Interactor;
        InteractionAttachController m_AttachController;

        void Awake()
        {
            m_Interactor = GetComponent<NearFarInteractor>();
            m_AttachController = (InteractionAttachController)m_Interactor.interactionAttachController;
        }

        void Update()
        {
            if (m_AttachController.hasOffset)
                return;

            var anchor = m_Interactor.attachTransform;
            if (!m_Interactor.hasSelection)
            {
                anchor.localPosition = Vector3.zero;
                return;
            }

            if (m_AttachController.manipulationInput.TryReadValue(out var input))
            {
                float distance = Mathf.Clamp(anchor.localPosition.z + input.y * speed * Time.deltaTime, 0f, maxDistance);
                anchor.localPosition = new Vector3(0f, 0f, distance);
            }
        }
    }
}

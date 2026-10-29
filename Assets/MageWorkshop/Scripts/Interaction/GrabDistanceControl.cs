using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
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
        [Range(0f, 1f)] public float deadZone = 0.2f;

        NearFarInteractor m_Interactor;
        InteractionAttachController m_AttachController;

        void Awake()
        {
            m_Interactor = GetComponent<NearFarInteractor>();
            m_AttachController = m_Interactor.interactionAttachController as InteractionAttachController;
        }

        void OnEnable()
        {
            m_Interactor.selectExited.AddListener(OnSelectExited);
        }

        void OnDisable()
        {
            m_Interactor.selectExited.RemoveListener(OnSelectExited);
        }

        void Update()
        {
            if (m_AttachController == null || !m_Interactor.hasSelection || m_AttachController.hasOffset)
                return;

            if (!m_AttachController.manipulationInput.TryReadValue(out var input) || Mathf.Abs(input.y) < deadZone)
                return;

            var anchor = m_Interactor.attachTransform;
            var position = anchor.localPosition;
            position.z = Mathf.Clamp(position.z + input.y * speed * Time.deltaTime, 0f, maxDistance);
            anchor.localPosition = position;
        }

        void OnSelectExited(SelectExitEventArgs args)
        {
            if (m_AttachController != null && !m_AttachController.hasOffset && !m_Interactor.hasSelection)
                m_Interactor.attachTransform.localPosition = Vector3.zero;
        }
    }
}

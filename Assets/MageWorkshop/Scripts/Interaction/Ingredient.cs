using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MageWorkshop
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Ingredient : MonoBehaviour
    {
        public IngredientDefinition definition;

        public XRGrabInteractable Grab { get; private set; }

        float m_LastConsumedTime = -10f;

        void Awake()
        {
            Grab = GetComponent<XRGrabInteractable>();
        }

        public bool TryConsume()
        {
            if (Time.time - m_LastConsumedTime < 0.5f)
                return false;

            m_LastConsumedTime = Time.time;
            if (TryGetComponent(out ReturnHome home))
                home.SendHome();
            else
                Destroy(gameObject);
            return true;
        }
    }
}

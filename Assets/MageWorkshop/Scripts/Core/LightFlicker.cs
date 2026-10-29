using UnityEngine;

namespace MageWorkshop
{
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        [Range(0f, 1f)] public float amount = 0.25f;
        public float speed = 4f;

        Light m_Light;
        float m_BaseIntensity;
        float m_Seed;

        void Awake()
        {
            m_Light = GetComponent<Light>();
            m_BaseIntensity = m_Light.intensity;
            m_Seed = Random.value * 100f;
        }

        void Update()
        {
            float noise = Mathf.PerlinNoise(m_Seed, Time.time * speed) - 0.5f;
            m_Light.intensity = m_BaseIntensity * (1f + noise * 2f * amount);
        }
    }
}

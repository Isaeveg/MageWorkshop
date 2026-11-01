using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MageWorkshop
{
    public class Cauldron : MonoBehaviour
    {
        [Header("Receptura")]
        public Recipe recipe;

        [Header("Ciecz")]
        [Tooltip("Pojemność kotła w ml.")]
        public float capacity = 600f;
        public Transform liquidSurface;
        public Renderer liquidRenderer;
        public float emptySurfaceHeight = 0.37f;
        public float fullSurfaceHeight = 0.8f;
        public Color baseLiquidColor = new Color(0.35f, 0.3f, 0.25f);
        [Tooltip("Średnica lustra cieczy w zależności od wysokości (dla kotła z brzuścem).")]
        public AnimationCurve surfaceDiameter = new AnimationCurve();

        [Header("Mieszanie")]
        [Tooltip("Środek dna kotła — oś, wokół której liczone są obroty.")]
        public Transform stirCenter;
        public float stirRadius = 0.23f;
        public float stirHeight = 0.5f;
        [Tooltip("Ile jednostek receptury przybywa na sekundę mieszania (1 = sekundy).")]
        public float stirRate = 1f;
        [Tooltip("Minimalna prędkość chochli (m/s), od której liczy się mieszanie.")]
        public float minStirSpeed = 0.1f;

        [Header("Efekty")]
        public TMP_Text statusText;
        public ParticleSystem explosionEffect;
        public ParticleSystem successEffect;
        public float explosionForce = 2.5f;
        public float explosionRadius = 1f;
        public float resetDelay = 3f;

        readonly List<BrewAction> m_Log = new List<BrewAction>();
        BrewResult m_Result;
        bool m_Resolving;
        string m_LastMessage;
        float m_LiquidVolume;

        Ladle m_Ladle;
        Vector3 m_PreviousTip;
        float m_NextStirHapticTime;

        void Start()
        {
            m_Ladle = FindFirstObjectByType<Ladle>();
            m_PreviousTip = m_Ladle.tip.position;
            ResetBrew();
        }

        void Update()
        {
            var tip = m_Ladle.tip.position;
            var local = tip - stirCenter.position;
            bool inCauldron = new Vector2(local.x, local.z).magnitude <= stirRadius && local.y >= -0.05f && local.y <= stirHeight;
            bool moving = (tip - m_PreviousTip).magnitude > minStirSpeed * Time.deltaTime;
            m_PreviousTip = tip;

            if (!m_Ladle.IsHeld || !inCauldron || !moving || m_Resolving)
                return;

            Record(BrewActionKind.Stir, null, stirRate * Time.deltaTime);

            if (Time.time >= m_NextStirHapticTime)
            {
                Haptics.Pulse(m_Ladle.Grab, 0.1f, 0.05f);
                m_NextStirHapticTime = Time.time + 0.15f;
            }
        }

        void OnTriggerStay(Collider other)
        {
            var ingredient = other.GetComponentInParent<Ingredient>();
            if (ingredient == null || ingredient.GetComponent<XRGrabInteractable>().isSelected)
                return;

            ingredient.GetComponent<ReturnHome>().SendHome();
            if (!m_Resolving)
                Record(BrewActionKind.Ingredient, ingredient.definition, 1f);
        }

        public void AddLiquid(IngredientDefinition liquid, float milliliters)
        {
            if (m_Resolving)
                return;

            m_LiquidVolume = Mathf.Min(capacity, m_LiquidVolume + milliliters);
            Record(BrewActionKind.Liquid, liquid, milliliters);
        }

        void Record(BrewActionKind kind, IngredientDefinition ingredient, float amount)
        {
            var last = m_Log.Count > 0 ? m_Log[m_Log.Count - 1] : null;
            if (last != null && last.kind == kind && last.ingredient == ingredient)
                last.amount += amount;
            else
                m_Log.Add(new BrewAction { kind = kind, ingredient = ingredient, amount = amount });

            m_Result = recipe.Evaluate(m_Log);
            if (m_Result.status == BrewStatus.Failed)
            {
                PushNearbyObjects();
                StartCoroutine(Finish($"<color=#b0281a>BUM! {m_Result.message}</color>", explosionEffect, 0.9f, resetDelay));
            }
            else if (m_Result.status == BrewStatus.Success)
            {
                StartCoroutine(Finish($"<color=#2e7d32>{m_Result.message}</color>", successEffect, 0.3f, resetDelay + 1f));
            }

            RefreshVisuals();
        }

        void PushNearbyObjects()
        {
            var center = stirCenter.position + Vector3.up * 0.4f;
            foreach (var hit in Physics.OverlapSphere(center, explosionRadius))
            {
                var body = hit.attachedRigidbody;
                if (body != null && !body.isKinematic)
                    body.AddExplosionForce(explosionForce, center, explosionRadius, 0.5f, ForceMode.Impulse);
            }
        }

        IEnumerator Finish(string message, ParticleSystem effect, float hapticStrength, float delay)
        {
            m_Resolving = true;
            m_LastMessage = message;
            effect.Play();
            Haptics.PulseAll(hapticStrength, 0.3f);

            yield return new WaitForSeconds(delay);
            ResetBrew();
        }

        void ResetBrew()
        {
            m_Log.Clear();
            m_LiquidVolume = 0f;
            m_Resolving = false;
            m_LastMessage = null;
            m_Result = recipe.Evaluate(m_Log);
            RefreshVisuals();
        }

        float SurfaceHeight(float volume)
        {
            return Mathf.Lerp(emptySurfaceHeight, fullSurfaceHeight, Mathf.Clamp01(volume / capacity));
        }

        void RefreshVisuals()
        {
            float height = SurfaceHeight(m_LiquidVolume);
            float diameter = surfaceDiameter.Evaluate(height);
            liquidSurface.gameObject.SetActive(m_Log.Count > 0);
            liquidSurface.localPosition = new Vector3(0f, height, 0f);
            liquidSurface.localScale = new Vector3(diameter, 1f, diameter);

            liquidRenderer.material.color = m_Result.status == BrewStatus.Success ? recipe.resultColor : MixColor();
            statusText.text = BuildStatusText();
        }

        Color MixColor()
        {
            var color = Color.black;
            float weight = 0f;
            foreach (var action in m_Log)
            {
                if (action.ingredient == null)
                    continue;
                float w = action.kind == BrewActionKind.Liquid ? action.amount / 100f : action.amount;
                color += action.ingredient.color * w;
                weight += w;
            }
            return weight > 0f ? Color.Lerp(baseLiquidColor, color / weight, 0.8f) : baseLiquidColor;
        }

        string BuildStatusText()
        {
            var text = new StringBuilder();
            text.AppendLine($"<b>Zamówienie: {recipe.displayName}</b>");
            for (int i = 0; i < recipe.steps.Count; i++)
            {
                var step = recipe.steps[i];
                bool done = i < m_Result.completedSteps;
                bool current = i == m_Result.completedSteps && m_Result.status == BrewStatus.InProgress;

                text.Append(done ? "<color=#2e7d32>[x]</color> " : current ? "<color=#b36b00>[>]</color> " : "[  ] ");
                text.Append(step.Describe());

                if (current && i < m_Log.Count)
                    text.Append($"  <color=#b36b00>[{m_Log[i].amount:0}/{step.amount:0}]</color>");
                text.AppendLine();
            }

            text.AppendLine().Append(m_LastMessage);
            return text.ToString();
        }
    }
}

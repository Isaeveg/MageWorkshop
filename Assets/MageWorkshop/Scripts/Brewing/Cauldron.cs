using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

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
        [Tooltip("Średnica lustra cieczy w zależności od wysokości (dla kotła z brzuścem). Pusta = bez zmian.")]
        public AnimationCurve surfaceDiameter = new AnimationCurve();
        [Tooltip("Znacznik poziomu płynu wymaganego przez recepturę.")]
        public Transform liquidMarker;

        [Header("Mieszanie")]
        [Tooltip("Środek dna kotła — oś, wokół której liczone są obroty.")]
        public Transform stirCenter;
        public float stirRadius = 0.23f;
        public float stirHeight = 0.5f;
        [Tooltip("Blisko osi kąt jest niestabilny, więc ten obszar jest ignorowany.")]
        public float stirDeadZone = 0.03f;

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

        bool m_HasPreviousAngle;
        float m_PreviousAngle;
        float m_AccumulatedAngle;
        float m_NextStirHapticTime;
        float m_NextTextRefreshTime;
        float m_LiquidVolume;

        void Start()
        {
            if (stirCenter == null)
                stirCenter = transform;
            ResetBrew();
        }

        void Update()
        {
            TrackStirring();

            if (m_HasPreviousAngle && Time.time >= m_NextTextRefreshTime)
            {
                RefreshVisuals();
                m_NextTextRefreshTime = Time.time + 0.2f;
            }
        }

        void OnTriggerStay(Collider other)
        {
            var ingredient = other.GetComponentInParent<Ingredient>();
            if (ingredient == null || ingredient.definition == null || ingredient.Grab.isSelected)
                return;

            if (!ingredient.TryConsume() || m_Resolving)
                return;

            Record(BrewActionKind.Ingredient, ingredient.definition, 1f, false);
        }

        public void AddLiquid(IngredientDefinition liquid, float milliliters)
        {
            if (m_Resolving || liquid == null || milliliters <= 0f)
                return;

            m_LiquidVolume = Mathf.Min(capacity, m_LiquidVolume + milliliters);
            Record(BrewActionKind.Liquid, liquid, milliliters, false);
        }

        void AddStirRevolution(bool clockwise)
        {
            if (m_Resolving)
                return;
            Record(BrewActionKind.Stir, null, 1f, clockwise);
        }

        void Record(BrewActionKind kind, IngredientDefinition ingredient, float amount, bool clockwise)
        {
            var last = m_Log.Count > 0 ? m_Log[m_Log.Count - 1] : null;
            if (last != null && last.CanMergeWith(kind, ingredient, clockwise))
                last.amount += amount;
            else
                m_Log.Add(new BrewAction { kind = kind, ingredient = ingredient, amount = amount, clockwise = clockwise });

            Evaluate();
        }

        void Evaluate()
        {
            if (recipe == null)
            {
                RefreshVisuals();
                return;
            }

            m_Result = recipe.Evaluate(m_Log);
            if (m_Result.status == BrewStatus.Failed)
                StartCoroutine(Explode(m_Result.message));
            else if (m_Result.status == BrewStatus.Success)
                StartCoroutine(Succeed(m_Result.message));

            RefreshVisuals();
        }

        void TrackStirring()
        {
            Ladle stirring = null;
            Vector3 local = default;
            foreach (var ladle in Ladle.Active)
            {
                if (!ladle.IsHeld)
                    continue;
                local = ladle.tip.position - stirCenter.position;
                var horizontal = new Vector2(local.x, local.z).magnitude;
                if (horizontal <= stirRadius && horizontal >= stirDeadZone && local.y >= -0.05f && local.y <= stirHeight)
                {
                    stirring = ladle;
                    break;
                }
            }

            if (stirring == null)
            {
                m_HasPreviousAngle = false;
                m_AccumulatedAngle = 0f;
                return;
            }

            float angle = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
            if (m_HasPreviousAngle)
            {
                float delta = Mathf.DeltaAngle(m_PreviousAngle, angle);
                m_AccumulatedAngle += delta;

                if (Mathf.Abs(delta) > 0.5f && Time.time >= m_NextStirHapticTime)
                {
                    stirring.Pulse(0.1f, 0.05f);
                    m_NextStirHapticTime = Time.time + 0.15f;
                }

                if (Mathf.Abs(m_AccumulatedAngle) >= 360f)
                {
                    bool clockwise = m_AccumulatedAngle > 0f;
                    m_AccumulatedAngle -= clockwise ? 360f : -360f;
                    stirring.Pulse(0.5f, 0.1f);
                    AddStirRevolution(clockwise);
                }
            }

            m_PreviousAngle = angle;
            m_HasPreviousAngle = true;
        }

        IEnumerator Explode(string message)
        {
            m_Resolving = true;
            m_LastMessage = "<color=#b0281a>BUM! " + message + "</color>";

            if (explosionEffect != null)
                explosionEffect.Play();

            var center = stirCenter.position + Vector3.up * 0.4f;
            foreach (var hit in Physics.OverlapSphere(center, explosionRadius))
            {
                var body = hit.attachedRigidbody;
                if (body != null && !body.isKinematic)
                    body.AddExplosionForce(explosionForce, center, explosionRadius, 0.5f, ForceMode.Impulse);
            }

            Haptics.PulseAll(0.9f, 0.4f);
            RefreshVisuals();

            yield return new WaitForSeconds(resetDelay);
            ResetBrew();
        }

        IEnumerator Succeed(string message)
        {
            m_Resolving = true;
            m_LastMessage = "<color=#2e7d32>" + message + "</color>";

            if (successEffect != null)
                successEffect.Play();
            Haptics.PulseAll(0.3f, 0.2f);
            RefreshVisuals();

            yield return new WaitForSeconds(resetDelay + 1f);
            ResetBrew();
        }

        public void ResetBrew()
        {
            StopAllCoroutines();
            m_Log.Clear();
            m_LiquidVolume = 0f;
            m_AccumulatedAngle = 0f;
            m_HasPreviousAngle = false;
            m_Resolving = false;
            m_LastMessage = null;
            m_Result = recipe != null ? recipe.Evaluate(m_Log) : default;

            if (liquidMarker != null && recipe != null)
            {
                var markerPosition = liquidMarker.localPosition;
                markerPosition.y = SurfaceHeight(recipe.TotalLiquid);
                liquidMarker.localPosition = markerPosition;
                if (surfaceDiameter.length > 0)
                    liquidMarker.localScale = Vector3.one * (surfaceDiameter.Evaluate(markerPosition.y) / 0.5f);
            }

            RefreshVisuals();
        }

        float SurfaceHeight(float volume)
        {
            return Mathf.Lerp(emptySurfaceHeight, fullSurfaceHeight, Mathf.Clamp01(volume / capacity));
        }

        void RefreshVisuals()
        {
            if (liquidSurface != null)
            {
                liquidSurface.gameObject.SetActive(m_Log.Count > 0);
                var position = liquidSurface.localPosition;
                position.y = SurfaceHeight(m_LiquidVolume);
                liquidSurface.localPosition = position;
                if (surfaceDiameter.length > 0)
                {
                    float diameter = surfaceDiameter.Evaluate(position.y);
                    liquidSurface.localScale = new Vector3(diameter, 1f, diameter);
                }
            }

            if (liquidRenderer != null)
                liquidRenderer.material.color = m_Result.status == BrewStatus.Success && recipe != null ? recipe.resultColor : MixColor();

            if (statusText != null)
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
            if (recipe == null)
                return "Brak receptury";

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
                else if (current && step.kind == BrewActionKind.Stir && Mathf.Abs(m_AccumulatedAngle) > 30f)
                    text.Append($"  <color=#b36b00>[{Mathf.Abs(m_AccumulatedAngle) / 3.6f:0}%]</color>");
                text.AppendLine();
            }

            if (!string.IsNullOrEmpty(m_LastMessage))
                text.AppendLine().Append(m_LastMessage);

            return text.ToString();
        }
    }
}

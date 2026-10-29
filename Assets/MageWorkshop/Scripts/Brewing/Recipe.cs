using System;
using System.Collections.Generic;
using UnityEngine;

namespace MageWorkshop
{
    public enum BrewStatus
    {
        InProgress,
        Success,
        Failed,
    }

    public struct BrewResult
    {
        public BrewStatus status;
        public int completedSteps;
        public string message;
    }

    [Serializable]
    public class RecipeStep
    {
        public BrewActionKind kind;
        [Tooltip("Puste dla kroku mieszania.")]
        public IngredientDefinition ingredient;
        [Tooltip("Sztuki, mililitry albo liczba obrotów.")]
        public float amount = 1f;
        [Tooltip("Dopuszczalne odchylenie ilości (dla płynów).")]
        public float tolerance;
        public bool clockwise = true;

        public string Describe()
        {
            switch (kind)
            {
                case BrewActionKind.Ingredient:
                    return $"{amount:0}× {Name}";
                case BrewActionKind.Liquid:
                    return $"{Name} do znacznika ({amount:0} ml)";
                default:
                    return $"{amount:0} obr. chochlą {(clockwise ? "zgodnie z ruchem wskazówek zegara" : "przeciwnie do ruchu wskazówek zegara")}";
            }
        }

        string Name => ingredient != null ? ingredient.displayName : "?";
    }

    [CreateAssetMenu(menuName = "Mage Workshop/Recipe", fileName = "Recipe")]
    public class Recipe : ScriptableObject
    {
        const float k_Epsilon = 0.001f;

        public string displayName = "Mikstura";
        public Color resultColor = Color.red;
        public List<RecipeStep> steps = new List<RecipeStep>();

        public float TotalLiquid
        {
            get
            {
                float total = 0f;
                foreach (var step in steps)
                {
                    if (step.kind == BrewActionKind.Liquid)
                        total += step.amount;
                }
                return total;
            }
        }

        public BrewResult Evaluate(IReadOnlyList<BrewAction> log)
        {
            for (int i = 0; i < log.Count; i++)
            {
                if (i >= steps.Count)
                    return Failed(i, "Zbędny składnik!");

                var action = log[i];
                var step = steps[i];
                bool isLast = i == log.Count - 1;

                if (action.kind != step.kind || action.ingredient != step.ingredient)
                    return Failed(i, $"Zły krok! Oczekiwano: {step.Describe()}");

                if (step.kind == BrewActionKind.Stir && action.clockwise != step.clockwise)
                    return Failed(i, "Zły kierunek mieszania!");

                if (action.amount > step.amount + step.tolerance + k_Epsilon)
                    return Failed(i, $"Za dużo: {StepName(step)}!");

                if (action.amount < step.amount - step.tolerance - k_Epsilon)
                {
                    if (isLast)
                        return new BrewResult { status = BrewStatus.InProgress, completedSteps = i };
                    return Failed(i, $"Za mało: {StepName(step)}!");
                }
            }

            if (log.Count == steps.Count)
                return new BrewResult { status = BrewStatus.Success, completedSteps = steps.Count, message = $"{displayName} gotowy!" };

            return new BrewResult { status = BrewStatus.InProgress, completedSteps = log.Count };
        }

        static BrewResult Failed(int completed, string message)
        {
            return new BrewResult { status = BrewStatus.Failed, completedSteps = completed, message = message };
        }

        static string StepName(RecipeStep step)
        {
            return step.kind == BrewActionKind.Stir ? "obrotów chochlą" : step.ingredient.displayName;
        }
    }
}

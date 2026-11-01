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
        [Tooltip("Sztuki, mililitry albo sekundy mieszania.")]
        public float amount = 1f;
        [Tooltip("Dopuszczalne odchylenie ilości (dla płynów i mieszania).")]
        public float tolerance;

        public string Name => ingredient != null ? ingredient.displayName : "mieszanie";

        public string Describe()
        {
            switch (kind)
            {
                case BrewActionKind.Ingredient:
                    return $"{amount:0}× {Name}";
                case BrewActionKind.Liquid:
                    return $"{Name} ({amount:0} ml)";
                default:
                    return $"Mieszaj chochlą przez {amount:0} s";
            }
        }
    }

    [CreateAssetMenu(menuName = "Mage Workshop/Recipe", fileName = "Recipe")]
    public class Recipe : ScriptableObject
    {
        public string displayName = "Mikstura";
        public Color resultColor = Color.red;
        public List<RecipeStep> steps = new List<RecipeStep>();

        public BrewResult Evaluate(IReadOnlyList<BrewAction> log)
        {
            for (int i = 0; i < log.Count; i++)
            {
                if (i >= steps.Count)
                    return Failed(i, "Zbędny składnik!");

                var action = log[i];
                var step = steps[i];

                if (action.kind != step.kind || action.ingredient != step.ingredient)
                    return Failed(i, $"Zły krok! Oczekiwano: {step.Describe()}");

                if (action.amount > step.amount + step.tolerance)
                    return Failed(i, $"Za dużo: {step.Name}!");

                if (action.amount < step.amount - step.tolerance)
                {
                    if (i == log.Count - 1)
                        return new BrewResult { status = BrewStatus.InProgress, completedSteps = i };
                    return Failed(i, $"Za mało: {step.Name}!");
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
    }
}

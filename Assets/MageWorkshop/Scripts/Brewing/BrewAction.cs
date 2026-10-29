using System;

namespace MageWorkshop
{
    public enum BrewActionKind
    {
        Ingredient,
        Liquid,
        Stir,
    }

    [Serializable]
    public class BrewAction
    {
        public BrewActionKind kind;
        public IngredientDefinition ingredient;
        public float amount;
        public bool clockwise;

        public bool CanMergeWith(BrewActionKind otherKind, IngredientDefinition otherIngredient, bool otherClockwise)
        {
            return kind == otherKind && ingredient == otherIngredient && (kind != BrewActionKind.Stir || clockwise == otherClockwise);
        }
    }
}

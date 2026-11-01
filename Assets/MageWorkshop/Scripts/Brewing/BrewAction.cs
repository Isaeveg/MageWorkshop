namespace MageWorkshop
{
    public enum BrewActionKind
    {
        Ingredient,
        Liquid,
        Stir,
    }

    public class BrewAction
    {
        public BrewActionKind kind;
        public IngredientDefinition ingredient;
        public float amount;
    }
}

using UnityEngine;

namespace MageWorkshop
{
    [CreateAssetMenu(menuName = "Mage Workshop/Ingredient", fileName = "Ingredient")]
    public class IngredientDefinition : ScriptableObject
    {
        public string displayName = "Składnik";
        public Color color = Color.white;
    }
}

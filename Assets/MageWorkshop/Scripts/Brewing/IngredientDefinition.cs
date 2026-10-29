using UnityEngine;

namespace MageWorkshop
{
    [CreateAssetMenu(menuName = "Mage Workshop/Ingredient", fileName = "Ingredient")]
    public class IngredientDefinition : ScriptableObject
    {
        public string displayName = "Składnik";
        public Color color = Color.white;
        [Tooltip("Płyny nalewa się z butelki (ilość w ml), stałe składniki wrzuca się do kotła (ilość w sztukach).")]
        public bool isLiquid;
    }
}

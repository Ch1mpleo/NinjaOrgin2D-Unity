using UnityEngine;
using UnityEngine.UI;

public class RecipeCard : MonoBehaviour
{
    [Header("Config")] 
    [SerializeField] private Image recipeIcon;

    public Recipe RecipeLoaded { get; set; }

    public void InitRecipeCard(Recipe recipe)
    {
        RecipeLoaded = recipe; 
        recipeIcon.sprite = recipe.FinalItem.Icon;
    }

    public void ClickRecipe()
    {
        CraftingManager.Instance.ShowRecipe(RecipeLoaded);
    }
}
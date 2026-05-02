using SAE.EdgeRuntime.Modules.Inventory.Domain;

namespace SAE.EdgeRuntime.Modules.Inventory.Services;

public class RecipeManager
{
    private readonly Dictionary<Guid, Recipe> _recipes = new();

    public void RegisterRecipe(Recipe recipe)
    {
        _recipes[recipe.ItemId] = recipe;
    }

    public IEnumerable<InventoryMovement> Decompose(Guid itemId, decimal quantity)
    {
        if (!_recipes.TryGetValue(itemId, out var recipe))
        {
            // If no recipe, just move the item itself (standard stock item)
            yield return new InventoryMovement(itemId, -quantity, "Sale", DateTime.UtcNow);
            yield break;
        }

        foreach (var ingredient in recipe.Ingredients)
        {
            yield return new InventoryMovement(
                ingredient.IngredientItemId, 
                -(ingredient.Quantity * quantity), 
                "RecipeDeduction", 
                DateTime.UtcNow
            );
        }
    }
}

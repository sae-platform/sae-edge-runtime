namespace SAE.EdgeRuntime.Modules.Inventory.Domain;

public record Recipe(Guid ItemId, List<IngredientUsage> Ingredients);

public record IngredientUsage(Guid IngredientItemId, decimal Quantity);

public record InventoryMovement(
    Guid ItemId,
    decimal Quantity,
    string Type, // Sale, Adjustment, Production
    DateTime Timestamp
);

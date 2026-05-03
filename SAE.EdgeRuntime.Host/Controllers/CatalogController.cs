using Microsoft.AspNetCore.Mvc;
using SAE.EdgeRuntime.Modules.Catalog.Persistence;
using SAE.EdgeRuntime.Modules.Catalog.Domain;

namespace SAE.EdgeRuntime.Host.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController(CatalogRepository repository) : ControllerBase
{
    private readonly CatalogRepository _repository = repository;

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _repository.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] Guid categoryId)
    {
        var products = await _repository.GetProductsByCategoryAsync(categoryId);
        return Ok(products);
    }

    [HttpGet("products/{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _repository.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }
}

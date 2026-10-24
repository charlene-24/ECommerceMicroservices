using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context) { _context = context; }

    // 1. Syncing
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    // 2. Adding
    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] Product newProduct)
    {
        if (newProduct == null || string.IsNullOrEmpty(newProduct.Name))
            return BadRequest("Product name is required.");

        if (newProduct.StockQuantity == 0) newProduct.StockQuantity = 10;

        _context.Products.Add(newProduct);
        await _context.SaveChangesAsync();
        return Ok(newProduct);
    }

    // 3. Updating
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.Name = updatedProduct.Name;
        product.Category = updatedProduct.Category;
        product.Description = updatedProduct.Description;
        product.Price = updatedProduct.Price;
        product.StockQuantity = updatedProduct.StockQuantity;
        product.ImageUrl = updatedProduct.ImageUrl;

        await _context.SaveChangesAsync();
        return Ok(product);
    }

    // 4. Deleting
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Product deleted successfully." });
    }

    // Service Update
    [HttpPatch("{id}/reduce-stock")]
    public async Task<IActionResult> ReduceStock(int id, [FromBody] int quantity)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (product.StockQuantity < quantity)
        {
            return BadRequest("Not enough stock available.");
        }

        product.StockQuantity -= quantity;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Stock updated", newStock = product.StockQuantity });
    }

}
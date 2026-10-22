using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context) { _context = context; }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] Product newProduct)
    {
        _context.Products.Add(newProduct);
        await _context.SaveChangesAsync();
        return Ok(newProduct);
    }

}
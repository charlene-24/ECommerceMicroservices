using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    // We need an HttpClient to talk to the ProductService
    private readonly IHttpClientFactory _httpClientFactory;

    public OrdersController(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order newOrder)
    {
        // Save Order to Database
        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        // Deducting Stock
        var client = _httpClientFactory.CreateClient();

        foreach (var item in newOrder.Items)
        {
            try
            {
                if (int.TryParse(item.ProductName, out int productId))
                {
                    var response = await client.PatchAsJsonAsync($"http://localhost:5065/api/products/{productId}/reduce-stock", item.Quantity);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to reduce stock for product {productId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to ProductService: {ex.Message}");
            }
        }

        return Ok(new { message = "Order saved and stock updated!", orderId = newOrder.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        // Fetch orders and include their items
        var orders = await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string newStatus)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = newStatus;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Status updated" });
    }

}
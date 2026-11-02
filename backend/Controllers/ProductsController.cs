using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
  private readonly AppDbContext _context;

  public ProductsController(AppDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
  {
    _ = page;
    _ = pageSize;

    return await _context.Products
      .Include(p => p.Category)
      .OrderBy(p => p.Id)
      .ToListAsync();
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Product>> GetProduct(int id)
  {
    var product = await _context.Products
        .Include(p => p.Category)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (product == null) return NotFound(new { message = "Produit introuvable." });
    return Ok(product);
  }

  [HttpPost]
  public async Task<ActionResult<Product>> CreateProduct([FromBody] Product newProductData)
  {
    if (string.IsNullOrWhiteSpace(newProductData.Name))
      return BadRequest(new { message = "Le nom est obligatoire." });

    if (newProductData.Price <= 0)
      return BadRequest(new { message = "Le prix doit être supérieur à 0." });

    if (newProductData.Stock < 0)
      return BadRequest(new { message = "Le stock ne peut pas être négatif." });

    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == newProductData.CategoryId);
    if (!categoryExists)
      return BadRequest(new { message = "La catégorie sélectionnée est invalide." });

    newProductData.CreatedAt = DateTime.UtcNow;

    _context.Products.Add(newProductData);
    await _context.SaveChangesAsync();

    var createdProduct = await _context.Products
      .Include(p => p.Category)
      .FirstAsync(p => p.Id == newProductData.Id);

    return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] Product updatedData)
  {
    var product = await _context.Products
        .Include(p => p.Category)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (product == null) return NotFound(new { message = "Produit introuvable." });

    if (string.IsNullOrWhiteSpace(updatedData.Name))
      return BadRequest(new { message = "Le nom est obligatoire." });

    if (updatedData.Price <= 0)
      return BadRequest(new { message = "Le prix doit être supérieur à 0." });

    if (updatedData.Stock < 0)
      return BadRequest(new { message = "Le stock ne peut pas être négatif." });

    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updatedData.CategoryId);
    if (!categoryExists)
      return BadRequest(new { message = "La catégorie sélectionnée est invalide." });

    product.Name = updatedData.Name;
    product.Description = updatedData.Description;
    product.Price = updatedData.Price;
    product.Stock = updatedData.Stock;
    product.CategoryId = updatedData.CategoryId;

    await _context.SaveChangesAsync();

    await _context.Entry(product).Reference(p => p.Category).LoadAsync();

    return Ok(product);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteProduct(int id)
  {
    var product = await _context.Products.FindAsync(id);
    if (product == null) return NotFound(new { message = "Produit introuvable." });

    _context.Products.Remove(product);
    await _context.SaveChangesAsync();

    return NoContent();
  }
}

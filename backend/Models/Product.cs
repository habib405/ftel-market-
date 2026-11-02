using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models;

public class Product
{
  public int Id { get; set; }

  [Required]
  [StringLength(200)]
  public string Name { get; set; } = string.Empty;

  [StringLength(1000)]
  public string? Description { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  [Range(0.01, 1000000)]
  public decimal Price { get; set; }

  [Range(0, int.MaxValue)]
  public int Stock { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  [Range(1, int.MaxValue)]
  public int CategoryId { get; set; }

  [JsonIgnore]
  public Category? Category { get; set; }

  [NotMapped]
  public string CategoryName => Category?.Name ?? string.Empty;
}

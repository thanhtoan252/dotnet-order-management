using System.ComponentModel.DataAnnotations;

namespace Catalog.Infrastructure.Excel;

internal sealed class ProductRow
{
    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Sku { get; set; }

    [Required]
    public decimal? Price { get; set; }

    [Required]
    public string? Currency { get; set; }

    public string? Description { get; set; }

    [Display(Name = "initial stock quantity")]
    public int? InitialStockQuantity { get; set; }
}

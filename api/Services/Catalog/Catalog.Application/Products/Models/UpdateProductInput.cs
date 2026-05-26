namespace Catalog.Application.Products.Models;

public sealed record UpdateProductInput(string? Name, decimal? Price, string? Currency);

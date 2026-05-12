using Catalog.Application.Products.Commands;

namespace Catalog.Application.Abstractions;

public interface IProductExcelParser
{
    IReadOnlyList<ImportProductsRow> Parse(Stream xlsxStream);
}

public sealed class ProductExcelParseException(string message) : Exception(message);

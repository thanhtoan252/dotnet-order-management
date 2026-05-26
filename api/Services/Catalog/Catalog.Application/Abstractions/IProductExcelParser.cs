using Catalog.Application.Products.Commands;

namespace Catalog.Application.Abstractions;

public interface IProductExcelParser
{
    ProductExcelParseResult Parse(Stream xlsxStream);
}

public sealed record ProductExcelParseResult(IReadOnlyList<ImportProductsRow> Rows, IReadOnlyDictionary<string, string[]> Errors)
{
    public bool IsValid => Errors.Count == 0;
}

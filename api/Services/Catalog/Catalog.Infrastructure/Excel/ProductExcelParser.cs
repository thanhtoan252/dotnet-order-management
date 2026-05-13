using Catalog.Application.Abstractions;
using Catalog.Application.Products.Commands;
using ClosedXML.Excel;

namespace Catalog.Infrastructure.Excel;

public sealed class ProductExcelParser : IProductExcelParser
{
    public IReadOnlyList<ImportProductsRow> Parse(Stream xlsxStream)
    {
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(xlsxStream);
        }
        catch (Exception ex)
        {
            throw new ProductExcelParseException($"Cannot read the file as an Excel workbook: {ex.Message}");
        }

        using (workbook)
        {
            var worksheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new ProductExcelParseException("The workbook contains no worksheets.");

            var columnMap = ExcelMapper<ProductRow>.BuildColumnMap(worksheet.Row(1));

            var missing = ExcelMapper<ProductRow>.MissingRequiredColumns(columnMap);
            if (missing.Count > 0)
            {
                throw new ProductExcelParseException(
                    $"Missing required header column(s): {string.Join(", ", missing)}. " +
                    "Expected: Name, SKU, Price, Currency, Description, InitialStockQuantity.");
            }
            
            var dataRows = worksheet.RowsUsed().Skip(1).ToList();
            var result = new List<ImportProductsRow>(dataRows.Count);

            foreach (var row in dataRows)
            {
                var mapped = ExcelMapper<ProductRow>.MapRow(row, columnMap);
                result.Add(new ImportProductsRow(
                    RowNumber: row.RowNumber(),
                    Name: mapped.Name,
                    Sku: mapped.Sku,
                    Price: mapped.Price,
                    Currency: mapped.Currency,
                    Description: mapped.Description,
                    InitialStockQuantity: mapped.InitialStockQuantity));
            }

            return result;
        }
    }
}

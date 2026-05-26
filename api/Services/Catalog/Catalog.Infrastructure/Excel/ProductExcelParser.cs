using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Application.Products.Commands;
using ClosedXML.Excel;

namespace Catalog.Infrastructure.Excel;

public sealed class ProductExcelParser : IProductExcelParser
{
    public ProductExcelParseResult Parse(Stream xlsxStream)
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
            var rows = new List<ImportProductsRow>(dataRows.Count);

            foreach (var row in dataRows)
            {
                var mapped = ExcelMapper<ProductRow>.MapRow(row, columnMap);
                rows.Add(new ImportProductsRow(
                    RowNumber: row.RowNumber(),
                    Name: mapped.Name,
                    Sku: mapped.Sku,
                    Price: mapped.Price,
                    Currency: mapped.Currency,
                    Description: mapped.Description,
                    InitialStockQuantity: mapped.InitialStockQuantity));
            }

            var errors = ValidateRows(rows);

            return new ProductExcelParseResult(rows, errors);
        }
    }

    private static Dictionary<string, string[]> ValidateRows(IReadOnlyList<ImportProductsRow> rows)
    {
        var errors = new Dictionary<string, string[]>();

        if (rows.Count == 0)
        {
            errors["file"] = ["No data rows found in the file."];
            return errors;
        }

        foreach (var row in rows)
        {
            ValidateRow(row, errors);
        }

        AddDuplicateSkuErrors(rows, errors);

        return errors;
    }

    private static void ValidateRow(ImportProductsRow row, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(row.Name))
        {
            AddRowError(row, nameof(row.Name), "'Name' must not be empty.", errors);
        }
        else if (row.Name.Length > 200)
        {
            AddRowError(row, nameof(row.Name), "'Name' must be less than or equal to 200 characters.", errors);
        }

        if (string.IsNullOrWhiteSpace(row.Sku))
        {
            AddRowError(row, nameof(row.Sku), "'Sku' must not be empty.", errors);
        }
        else if (row.Sku.Length > 50)
        {
            AddRowError(row, nameof(row.Sku), "'Sku' must be less than or equal to 50 characters.", errors);
        }

        if (!row.Price.HasValue)
        {
            AddRowError(row, nameof(row.Price), "'Price' must not be empty.", errors);
        }
        else if (row.Price <= 0)
        {
            AddRowError(row, nameof(row.Price), "Price must be positive.", errors);
        }

        if (string.IsNullOrWhiteSpace(row.Currency))
        {
            AddRowError(row, nameof(row.Currency), "'Currency' must not be empty.", errors);
        }
        else if (row.Currency.Length != 3)
        {
            AddRowError(row, nameof(row.Currency), "'Currency' must be 3 characters in length.", errors);
        }

        if (!string.IsNullOrEmpty(row.Description) && row.Description.Length > 1000)
        {
            AddRowError(row, nameof(row.Description), "'Description' must be less than or equal to 1000 characters.", errors);
        }

        if (row.InitialStockQuantity < 0)
        {
            AddRowError(row, nameof(row.InitialStockQuantity), "Initial stock quantity cannot be negative.", errors);
        }
    }

    private static void AddDuplicateSkuErrors(IReadOnlyList<ImportProductsRow> rows, Dictionary<string, string[]> errors)
    {
        var skuGroups = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Sku))
            .GroupBy(r => r.Sku!.Trim().ToUpperInvariant())
            .Where(g => g.Count() > 1);

        foreach (var group in skuGroups)
        {
            var firstRowNumber = group.First().RowNumber;
            foreach (var duplicate in group.Skip(1))
            {
                errors[$"row[{duplicate.RowNumber}].sku"] =
                    [$"Duplicate SKU '{group.Key}' in file (first seen at row {firstRowNumber})."];
            }
        }
    }

    private static void AddRowError(
        ImportProductsRow row,
        string propertyName,
        string errorMessage,
        Dictionary<string, string[]> errors)
    {
        errors[$"row[{row.RowNumber}].{ToCamelCase(propertyName)}"] = [errorMessage];
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}

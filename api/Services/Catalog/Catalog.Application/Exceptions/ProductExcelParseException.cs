namespace Catalog.Application.Exceptions;

public sealed class ProductExcelParseException(string message) : Exception(message);

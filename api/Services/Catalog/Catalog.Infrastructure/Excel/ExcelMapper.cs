using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using ClosedXML.Excel;

namespace Catalog.Infrastructure.Excel;

internal static class ExcelMapper<T> where T : new()
{
    private sealed record PropMap(PropertyInfo Property, string Canonical, bool Required);

    private static readonly PropMap[] PropMaps = typeof(T)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(p => new PropMap(
            p,
            p.Name,
            p.GetCustomAttribute<RequiredAttribute>() is not null))
        .ToArray();

    private static readonly IReadOnlyDictionary<string, string> AliasLookup = BuildAliasLookup();

    private static Dictionary<string, string> BuildAliasLookup()
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pm in PropMaps)
        {
            dict[pm.Canonical] = pm.Canonical;
            if (pm.Property.GetCustomAttribute<DisplayAttribute>()?.Name is { } displayName)
                dict[displayName] = pm.Canonical;
        }
        return dict;
    }

    public static Dictionary<string, int> BuildColumnMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var header = cell.GetString().Trim();
            if (AliasLookup.TryGetValue(header, out var canonical))
                map[canonical] = cell.Address.ColumnNumber;
        }
        return map;
    }

    public static IReadOnlyList<string> MissingRequiredColumns(IReadOnlyDictionary<string, int> columnMap) =>
        PropMaps
            .Where(pm => pm.Required && !columnMap.ContainsKey(pm.Canonical))
            .Select(pm => pm.Canonical)
            .ToArray();

    public static T MapRow(IXLRow row, IReadOnlyDictionary<string, int> columnMap)
    {
        var instance = new T();
        foreach (var pm in PropMaps)
        {
            if (!columnMap.TryGetValue(pm.Canonical, out var colNum)) continue;
            var value = ConvertCell(row.Cell(colNum), pm.Property.PropertyType);
            pm.Property.SetValue(instance, value);
        }
        return instance;
    }

    private static object? ConvertCell(IXLCell cell, Type targetType)
    {
        if (cell.IsEmpty()) return null;

        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (type == typeof(string))
        {
            var s = cell.GetString().Trim();
            return string.IsNullOrEmpty(s) ? null : s;
        }
        if (type == typeof(decimal))
        {
            if (cell.DataType == XLDataType.Number) return (decimal)cell.GetDouble();
            return decimal.TryParse(cell.GetString().Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
                ? (object)d : null;
        }
        if (type == typeof(int))
        {
            if (cell.DataType == XLDataType.Number) return (int)cell.GetDouble();
            return int.TryParse(cell.GetString().Trim(), out var i) ? (object)i : null;
        }
        return null;
    }
}

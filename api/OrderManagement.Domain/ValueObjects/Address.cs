namespace OrderManagement.Domain.ValueObjects;

/// <summary>
///     Immutable value object — represents a physical address.
///     Two addresses are equal if all their components are equal.
/// </summary>
public sealed record Address(
    string Street,
    string City,
    string Province,
    string ZipCode)
{
    public static Address Create(string street, string city, string province, string zipCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(province);
        ArgumentException.ThrowIfNullOrWhiteSpace(zipCode);

        return new Address(street.Trim(), city.Trim(), province.Trim(), zipCode.Trim());
    }

    public override string ToString()
    {
        return $"{Street}, {City}, {Province} {ZipCode}";
    }
}
namespace Order.Application.Orders.Models;

public sealed record AddressInput(string Street, string City, string Province, string ZipCode);

using System;

namespace PlaceRentalApi.API.ValueObjects;

public record Address(string Street, string Number, string District, string ZipCode, string City, string State, string Country)
{
    public string GetFullAddress() => $"{Street}, {Number}, {District}, {ZipCode}, {City}, {State}, {Country}";
}

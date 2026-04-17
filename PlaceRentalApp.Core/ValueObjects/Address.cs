namespace PlaceRentalApp.Core.ValueObjects
{
    public record Address(string Street, string Number, string ZipCode, string District, string City, string State, string Country)
    {
        public string GetFullAddress()
            => $"{Street}, {Number}, {ZipCode}, {District}, {City}, {State}, {Country}";
    }
}

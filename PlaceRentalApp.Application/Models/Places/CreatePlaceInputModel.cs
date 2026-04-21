namespace PlaceRentalApp.Application.Models;

public class CreatePlaceInputModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DailyPrice { get; set; }
    public AddressInputModel Address { get; set; }
    public int AllowedNumberPerson { get; set; }
    public bool AllowPets { get; set; }
    public int CreatedBy { get; set; }
}

public class AddressInputModel
{
    public string Street { get; set; }
    public string Number { get; set; }
    public string ZipCode { get; set; }
    public string District { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
}


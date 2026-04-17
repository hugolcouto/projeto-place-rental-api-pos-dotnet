namespace PlaceRentalApp.Core.Entities;

public class PlaceAmenity : BaseEntity
{
    protected PlaceAmenity() { }
    public PlaceAmenity(string description, int idPlace)
        : base()
    {
        Description = description;
        IdPlace = idPlace;
    }

    public string Description { get; private set; }
    public int IdPlace { get; private set; }
}

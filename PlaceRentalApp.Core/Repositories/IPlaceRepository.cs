using System;
using PlaceRentalApp.Core.Entities;

namespace PlaceRentalApp.Core.Repositories;

public interface IPlaceRepository
{
    List<Place>? GetAllAvailable(string search, DateTime startDate, DateTime endDate);
    Place? GetById(int id);
    int Add(Place place);
    void Update(Place place);
    void AddBook(PlaceBook book);
    void AddAmenity(PlaceAmenity amenity);
    void Delete(Place place);
}

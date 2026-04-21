using Microsoft.EntityFrameworkCore;
using PlaceRentalApp.API.Persistence;
using PlaceRentalApp.Core.Entities;
using PlaceRentalApp.Core.Repositories;

namespace PlaceRentalApp.Infrastructure.Persistence.Repositories;

public class PlaceRepository : IPlaceRepository
{
    private readonly PlaceRentalDbContext _context;

    public PlaceRepository(PlaceRentalDbContext context) => _context = context;

    public int Add(Place place)
    {
        _context.Places.Add(place);
        _context.SaveChanges();

        return place.Id;
    }

    public void AddAmenity(PlaceAmenity amenity)
    {
        _context.PlaceAmenities.Add(amenity);
        _context.SaveChanges();
    }

    public void AddBook(PlaceBook book)
    {
        _context.PlaceBooks.Add(book);
        _context.SaveChanges();
    }

    public List<Place>? GetAllAvailable(string search, DateTime startDate, DateTime endDate)
    {
        List<Place> availablePlaces = _context
            .Places
            .Include(p => p.User)
            .Where(p =>
                p.Title.Contains(search) &&
                !p.Books.Any(b =>
                (startDate >= b.StartDate && startDate <= b.EndDate) ||
                (endDate >= b.StartDate && endDate <= b.EndDate) ||
                (startDate <= b.StartDate && endDate >= b.EndDate))
                && !p.IsDeleted
            )
            .ToList();

        return availablePlaces;
    }

    public Place? GetById(int id)
    {
        Place? place = _context
            .Places
            .Include(p => p.Amenities)
            .Include(p => p.User)
            .SingleOrDefault(p => p.Id == id);

        return place;
    }

    public void Update(Place place)
    {
        _context.Places.Update(place!);
        _context.SaveChanges();
    }

    public void Delete(Place place)
    {
        _context.Places.Update(place);
        _context.SaveChanges();
    }

}

using PlaceRentalApp.API.Persistence;
using PlaceRentalApp.Application.Exceptions;
using PlaceRentalApp.Application.Models;
using PlaceRentalApp.Core.Entities;
using PlaceRentalApp.Core.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlaceRentalApp.Application.Services;

public class PlaceService : IPlaceService
{
    private readonly PlaceRentalDbContext _context;

    public PlaceService(PlaceRentalDbContext context) => _context = context;

    // CREATE
    public ResultViewModel<int> Inset(CreatePlaceInputModel model)
    {
        var address = new Address(
            model.Address.Street,
            model.Address.Number,
            model.Address.ZipCode,
            model.Address.District,
            model.Address.City,
            model.Address.State,
            model.Address.Country
        );

        var place = new Place(
            model.Title,
            model.Description,
            model.DailyPrice,
            address,
            model.AllowedNumberPerson,
            model.AllowPets,
            model.CreatedBy
        );

        _context.Places.Add(place);
        _context.SaveChanges();

        return ResultViewModel<int>.Success(place.Id);
    }

    public ResultViewModel Book(int id, CreateBookInputModel model)
    {
        bool exists = _context.Places.Any(p => p.Id == id);

        if (!exists) return ResultViewModel.Error("Not Found");

        PlaceBook book = new PlaceBook(model.IdUser, model.IdPlace, model.StartDate, model.EndDate, model.Comments);

        _context.PlaceBooks.Add(book);
        _context.SaveChanges();

        return ResultViewModel.Success();
    }

    public ResultViewModel<int> InsertAmenity(int id, CreatePlaceAmenityInputModel model)
    {
        bool exists = _context.Places.Any(p => p.Id == id);

        if (!exists) return (ResultViewModel<int>)ResultViewModel.Error("Not Found");

        PlaceAmenity amenity = new PlaceAmenity(model.Description, id);

        _context.PlaceAmenities.Add(amenity);
        _context.SaveChanges();

        return ResultViewModel<int>.Success(amenity.Id);
    }


    // READ
    public ResultViewModel<List<PlaceViewModel>> GetAllAvailable(string search, DateTime startDate, DateTime endDate)
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

        List<PlaceViewModel>? model = availablePlaces.Select(
            PlaceViewModel.FromEntity
        ).ToList()!;

        return ResultViewModel<List<PlaceViewModel>>.Success(model);
    }

    public ResultViewModel<PlaceDetailsViewModel?> GetById(int id)
    {
        Place? place = _context.Places.SingleOrDefault(p => p.Id == id);

        return ResultViewModel<PlaceDetailsViewModel?>.Success(
            PlaceDetailsViewModel.FromEntity(place)
        )!;
    }

    // UPDATE
    public ResultViewModel Update(int id, UpdatePlaceInputModel model)
    {
        Place? place = _context
            .Places
            .SingleOrDefault(p => p.Id == id);

        if (place is null) ResultViewModel.Error("Not Found");

        place!.Update(model.Title, model.Description, model.DailyPrice);

        _context.Places.Update(place!);
        _context.SaveChanges();

        return ResultViewModel.Success();
    }


    // DELETE
    public ResultViewModel Delete(int id)
    {
        var place = _context
            .Places
            .SingleOrDefault(p => p.Id == id);

        if (place is null) return ResultViewModel.Error("Not Found");

        place.SetAsDeleted();

        _context.Places.Update(place);
        _context.SaveChanges();

        return ResultViewModel.Success();
    }

    public ResultViewModel<string> PostPlacePhoto(int id, IFormFile file)
    {
        using MemoryStream ms = new MemoryStream();

        file.CopyTo(ms);

        byte[] fileBytes = ms.ToArray();
        string base64 = Convert.ToBase64String(fileBytes);

        return ResultViewModel<string>.Success(base64);
    }
}
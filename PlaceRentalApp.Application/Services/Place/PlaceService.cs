using PlaceRentalApp.Application.Models;
using PlaceRentalApp.Core.Entities;
using PlaceRentalApp.Core.ValueObjects;
using Microsoft.AspNetCore.Http;
using PlaceRentalApp.Core.Repositories;

namespace PlaceRentalApp.Application.Services;

public class PlaceService : IPlaceService
{
    private readonly IPlaceRepository _placeRepository;

    public PlaceService(IPlaceRepository placeRepository) => _placeRepository = placeRepository;

    // CREATE
    public ResultViewModel<int> Inset(CreatePlaceInputModel model)
    {
        Address address = new Address(
            model.Address.Street,
            model.Address.Number,
            model.Address.ZipCode,
            model.Address.District,
            model.Address.City,
            model.Address.State,
            model.Address.Country
        );

        Place place = new Place(
            model.Title,
            model.Description,
            model.DailyPrice,
            address,
            model.AllowedNumberPerson,
            model.AllowPets,
            model.CreatedBy
        );

        _placeRepository.Add(place);

        return ResultViewModel<int>.Success(place.Id);
    }

    public ResultViewModel Book(int id, CreateBookInputModel model)
    {
        Place? place = _placeRepository.GetById(id);

        if (place is null) return ResultViewModel.Error("Not Found");

        PlaceBook book = new PlaceBook(
            model.IdUser,
            model.IdPlace,
            model.StartDate,
            model.EndDate,
            model.Comments
        );

        _placeRepository.AddBook(book);

        return ResultViewModel.Success();
    }

    public ResultViewModel<int> InsertAmenity(int id, CreatePlaceAmenityInputModel model)
    {
        Place? place = _placeRepository.GetById(id);

        if (place is null) return (ResultViewModel<int>)ResultViewModel.Error("Not Found");

        PlaceAmenity amenity = new PlaceAmenity(model.Description, id);

        _placeRepository.AddAmenity(amenity);

        return ResultViewModel<int>.Success(amenity.Id);
    }


    // READ
    public ResultViewModel<List<PlaceViewModel>> GetAllAvailable(string search, DateTime startDate, DateTime endDate)
    {
        List<Place>? availablePlaces = _placeRepository.GetAllAvailable(search, startDate, endDate);

        List<PlaceViewModel>? model = availablePlaces!.Select(
            PlaceViewModel.FromEntity
        ).ToList()!;

        return ResultViewModel<List<PlaceViewModel>>.Success(model);
    }

    public ResultViewModel<PlaceDetailsViewModel?> GetById(int id)
    {
        Place place = _placeRepository.GetById(id)!;

        return ResultViewModel<PlaceDetailsViewModel?>.Success(
            PlaceDetailsViewModel.FromEntity(place)
        )!;
    }

    // UPDATE
    public ResultViewModel Update(int id, UpdatePlaceInputModel model)
    {
        Place? place = _placeRepository.GetById(id);

        if (place is null) ResultViewModel.Error("Not Found");

        place!.Update(model.Title, model.Description, model.DailyPrice);

        _placeRepository.Update(place);

        return ResultViewModel.Success();
    }


    // DELETE
    public ResultViewModel Delete(int id)
    {
        Place? place = _placeRepository.GetById(id);

        if (place is null) return ResultViewModel.Error("Not Found");

        place.SetAsDeleted();

        _placeRepository.Delete(place);

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
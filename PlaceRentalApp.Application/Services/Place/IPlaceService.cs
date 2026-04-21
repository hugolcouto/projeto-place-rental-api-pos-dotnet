using System;
using PlaceRentalApp.Application.Models;
using Microsoft.AspNetCore.Http;

namespace PlaceRentalApp.Application.Services;

public interface IPlaceService
{
    ResultViewModel<List<PlaceViewModel>> GetAllAvailable(string search, DateTime startDate, DateTime endDate);
    ResultViewModel Book(int id, CreateBookInputModel model);
    ResultViewModel<int> Inset(CreatePlaceInputModel model);
    ResultViewModel<int> InsertAmenity(int id, CreatePlaceAmenityInputModel model);
    ResultViewModel Update(int id, UpdatePlaceInputModel model);
    ResultViewModel Delete(int id);
    ResultViewModel<string> PostPlacePhoto(int id, IFormFile file);
    ResultViewModel<PlaceDetailsViewModel?> GetById(int id);
}

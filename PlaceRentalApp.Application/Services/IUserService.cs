using PlaceRentalApp.Application.Models;
using PlaceRentalApp.Core.Entities;

namespace PlaceRentalApp.Application.Services;

public interface IUserService
{
    User? GetById(int id);
    int Insert(CreateUserInputModel model);
}

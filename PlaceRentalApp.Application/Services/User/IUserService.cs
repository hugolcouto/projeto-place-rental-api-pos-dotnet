using PlaceRentalApp.Application.Models;
using PlaceRentalApp.Core.Entities;

namespace PlaceRentalApp.Application.Services;

public interface IUserService
{
    ResultViewModel<UserDetailsViewModel?> GetById(int id);
    ResultViewModel<int> Post(User user);
    ResultViewModel<LoginViewModel> Login(LoginInputModel model);
}

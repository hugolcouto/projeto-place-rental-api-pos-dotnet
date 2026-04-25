using PlaceRentalApp.Application.Models;
using PlaceRentalApp.Core.Entities;
using PlaceRentalApp.Core.Repositories;
using PlaceRentalApp.Infrastructure.Auth;

namespace PlaceRentalApp.Application.Services;

public class UserService : IUserService
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public ResultViewModel<UserDetailsViewModel?> GetById(int id)
    {
        User user = _userRepository.GetById(id)!;

        return ResultViewModel<UserDetailsViewModel?>.Success(
            UserDetailsViewModel.FromEntity(user)
        )!;
    }

    public ResultViewModel<LoginViewModel> Login(LoginInputModel model)
    {
        string hash = _authService.ComputeHash(model.Password);

        User? user = _userRepository.GetUserByAuth(model.Email, hash);

        if (user is null) return (ResultViewModel<LoginViewModel>)ResultViewModel.Error("User not found");

        string token = _authService.GenerateToken(user.Email, user.Role);

        LoginViewModel viewModel = new LoginViewModel(token);

        return ResultViewModel<LoginViewModel>.Success(viewModel);
    }

    public ResultViewModel<int> Post(User user)
    {
        _userRepository.Post(user);

        return ResultViewModel<int>.Success(user.Id);
    }
}

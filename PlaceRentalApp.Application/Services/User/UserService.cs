using PlaceRentalApp.Application.Models;
using PlaceRentalApp.Core.Entities;
using PlaceRentalApp.Core.Repositories;

namespace PlaceRentalApp.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository) => _userRepository = userRepository;

    public ResultViewModel<UserDetailsViewModel?> GetById(int id)
    {
        User user = _userRepository.GetById(id)!;

        return ResultViewModel<UserDetailsViewModel?>.Success(
            UserDetailsViewModel.FromEntity(user)
        )!;
    }

    public ResultViewModel<int> Post(User user)
    {
        _userRepository.Post(user);

        return ResultViewModel<int>.Success(user.Id);
    }
}

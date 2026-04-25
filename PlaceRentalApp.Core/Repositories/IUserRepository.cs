using System;
using PlaceRentalApp.Core.Entities;

namespace PlaceRentalApp.Core.Repositories;

public interface IUserRepository
{
    User? GetById(int id);
    int Post(User user);
    User? GetUserByAuth(string email, string hash);
}

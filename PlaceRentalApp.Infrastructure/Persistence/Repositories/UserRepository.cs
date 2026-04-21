using System;
using PlaceRentalApp.API.Persistence;
using PlaceRentalApp.Core.Entities;
using PlaceRentalApp.Core.Repositories;

namespace PlaceRentalApp.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    protected readonly PlaceRentalDbContext _context;

    public UserRepository(PlaceRentalDbContext context) => _context = context;

    public User? GetById(int id)
    {
        User? user = _context
            .Users
            .SingleOrDefault(u => u.Id == id)!;

        return user;
    }

    public int Post(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();

        return user.Id;
    }
}
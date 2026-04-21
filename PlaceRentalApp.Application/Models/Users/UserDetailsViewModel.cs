using System;
using PlaceRentalApp.Core.Entities;

namespace PlaceRentalApp.Application.Models;

public class UserDetailsViewModel
{
    public string FullName { get; set; }
    public string Email { get; set; }

    public static UserDetailsViewModel? FromEntity(User? entity)
    => entity is null
    ? null
    : new UserDetailsViewModel
    {
        FullName = entity.FullName,
        Email = entity.Email
    };

}

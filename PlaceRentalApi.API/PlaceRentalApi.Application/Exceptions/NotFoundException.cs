using System;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PlaceRentalApi.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message = "Not Found") : base()
    { }
}

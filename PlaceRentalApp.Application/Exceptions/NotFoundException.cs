using System;

namespace PlaceRentalApp.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message = "Not Found") : base() { }
}

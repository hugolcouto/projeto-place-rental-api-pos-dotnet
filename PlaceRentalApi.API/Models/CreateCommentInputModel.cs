using System;

namespace PlaceRentalApi.API.Models;

public class CreateCommentInputModel
{
    public int IdUser { get; set; }
    public string Comment { get; set; }
}

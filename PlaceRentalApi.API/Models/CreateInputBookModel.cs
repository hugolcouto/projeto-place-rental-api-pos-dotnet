using System;

namespace PlaceRentalApi.API.Models;

public class CreateInputBookModel
{
    public int IdUser { get; set; }
    public int IdPlace { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Comments { get; set; }
}

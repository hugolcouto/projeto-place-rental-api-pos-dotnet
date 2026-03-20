using System;

namespace PlaceRentalApi.API.Models;

public class UpdatePlaceInputModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DailyPrice { get; set; }
}

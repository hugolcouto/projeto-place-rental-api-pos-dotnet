using System;

namespace PlaceRentalApi.API.Entities;

public class PlaceBook : BaseEntity
{
    protected PlaceBook() {}
    
    public PlaceBook(int idUser, int idPlace, DateTime startDate, DateTime endDate, string comments)
    : base()
    {
        IdUser = idUser;
        IdPlace = idPlace;
        StartDate = startDate;
        EndDate = endDate;
        Comments = comments;
    }

    public int IdUser { get; private set; }
    public int IdPlace { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Comments { get; private set; }
    public User User { get; private set; }
    public Place Place { get; private set; }

}

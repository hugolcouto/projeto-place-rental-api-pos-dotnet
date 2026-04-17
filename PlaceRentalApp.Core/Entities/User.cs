namespace PlaceRentalApp.Core.Entities;

public class User : BaseEntity
{
    protected User() { }
    public User(string fullName, string email, DateTime birthDate)
        : base()
    {
        FullName = fullName;
        Email = email;
        BirthDate = birthDate;

        Books = [];
        Places = [];
    }

    public string FullName { get; private set; }
    public string Email { get; private set; }
    public DateTime BirthDate { get; private set; }

    public List<PlaceBook> Books { get; private set; }
    public List<Place> Places { get; private set; }
}
namespace PlaceRentalApp.Core.Entities;

public class User : BaseEntity
{
    protected User() { }
    public User(string fullName, string email, DateTime birthDate, string password, string role)
        : base()
    {
        FullName = fullName;
        Email = email;
        Password = password;
        Role = role ?? "client";
        BirthDate = birthDate;

        Books = [];
        Places = [];
    }

    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    public string Role { get; private set; }
    public DateTime BirthDate { get; private set; }

    public List<PlaceBook> Books { get; private set; }
    public List<Place> Places { get; private set; }
}
using Microsoft.EntityFrameworkCore;
using PlaceRentalApi.Core.Entities;
namespace PlaceRentalApi.Infrastructure.Persistence;

public class PlaceRentalDbContext(DbContextOptions<PlaceRentalDbContext> options) : DbContext(options)
{
    public DbSet<Place> Places { get; set; }
    public DbSet<PlaceAmenity> PlaceAmenities { get; private set; }
    public DbSet<PlaceBook> PlaceBooks { get; private set; }
    public DbSet<User> Users { get; private set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Place>(e =>
        {
            e.HasKey(p => p.Id);

            e.HasMany(p => p.Amenities)
                .WithOne()
                .HasForeignKey(a => a.IdPlace)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(p => p.Books)
                .WithOne(b => b.Place)
                .HasForeignKey(a => a.IdPlace)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.User)
                .WithMany(u => u.Places)
                .HasForeignKey(p => p.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            e.OwnsOne(p => p.Address, a =>
            {
                a.Property(d => d.Street).HasColumnName("Street");
                a.Property(d => d.Number).HasColumnName("Number");
                a.Property(d => d.District).HasColumnName("District");
                a.Property(d => d.ZipCode).HasColumnName("StrZipCode");
                a.Property(d => d.City).HasColumnName("City");
                a.Property(d => d.State).HasColumnName("SState");
                a.Property(d => d.Country).HasColumnName("Country");
            });
        });

        builder.Entity<PlaceAmenity>(e =>
        {
            e.HasKey(p => p.Id);
        });

        builder.Entity<PlaceBook>(e =>
        {
            e.HasKey(p => p.Id);
        });

        builder.Entity<User>(e =>
        {
            e.HasKey(p => p.Id);

            e.HasMany(p => p.Books)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.IdUser)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(builder);
    }
}

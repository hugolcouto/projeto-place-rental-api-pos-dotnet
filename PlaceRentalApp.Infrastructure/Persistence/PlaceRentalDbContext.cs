using Microsoft.EntityFrameworkCore;
using PlaceRentalApp.Core.Entities;

namespace PlaceRentalApp.API.Persistence
{
    public class PlaceRentalDbContext : DbContext
    {
        public PlaceRentalDbContext(
            DbContextOptions<PlaceRentalDbContext> options
        ) : base(options) { }

        public DbSet<Place> Places { get; set; }
        public DbSet<PlaceAmenity> PlaceAmenities { get; set; }
        public DbSet<PlaceBook> PlaceBooks { get; set; }
        public DbSet<User> Users { get; set; }

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
                    .HasForeignKey(p => p.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                e.OwnsOne(p => p.Address, a =>
                {
                    a.Property(d => d.Street).HasColumnName("Street");
                    a.Property(d => d.Number).HasColumnName("Number");
                    a.Property(d => d.ZipCode).HasColumnName("ZipCode");
                    a.Property(d => d.District).HasColumnName("District");
                    a.Property(d => d.City).HasColumnName("City");
                    a.Property(d => d.State).HasColumnName("State");
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
}

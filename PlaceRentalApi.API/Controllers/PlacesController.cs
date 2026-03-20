using Microsoft.AspNetCore.Mvc;
using PlaceRentalApi.API.Entities;
using PlaceRentalApi.API.Models;
using PlaceRentalApi.API.Persistence;
using PlaceRentalApi.API.ValueObjects;

namespace PlaceRentalApi.API.Controllers
{
    [Route("api/places")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly PlaceRentalDbContext _context;

        public PlacesController(PlaceRentalDbContext context)
        {
            _context = context;
        }

        // GET api/places?search=<search>
        [HttpGet]
        public IActionResult Search(string? search, DateTime? startDate, DateTime? endDate)
        {
            IQueryable<Place> availablePlaces = _context
            .Places
            .Where(place =>
                (string.IsNullOrEmpty(search) || place.Title.Contains(search)) &&
                !place.Books.Any(placeBook =>
                    (startDate >= placeBook.StartDate && startDate <= placeBook.EndDate) ||
                    (endDate >= placeBook.StartDate && endDate <= placeBook.EndDate) ||
                    (startDate <= placeBook.StartDate && endDate >= placeBook.EndDate))
                && !place.IsDeleted
            );

            return Ok(availablePlaces);
        }

        // GET api/places/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            Place? place = _context.Places.SingleOrDefault(place => place.Id == id);

            if (place is null) NotFound();

            return Ok(place);
        }

        // POST api/places
        [HttpPost]
        public IActionResult Post(CreatePlaceInputModel model)
        {
            Address address = new Address(
                model.Address.Street,
                model.Address.Number,
                model.Address.District,
                model.Address.ZipCode,
                model.Address.City,
                model.Address.State,
                model.Address.Country
            );

            Place place = new Place(
                model.Title,
                model.Description,
                model.DailyPrice,
                address,
                model.AllowedNumberPerson,
                model.AllowPets,
                model.CreatedBy
            );

            _context.Places.Add(place);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = place.Id }, model);
        }

        // PUT api/places/{id}
        [HttpPut("{id}")]
        public IActionResult Put(int id, UpdatePlaceInputModel model)
        {
            Place? place = _context.Places.SingleOrDefault(place => place.Id == id);

            if (place is null)
            {
                return NotFound();
            }

            place.Update(model.Title, model.Description, model.DailyPrice);
            _context.Places.Update(place);
            _context.SaveChanges();

            return NoContent();
        }

        // POST api/places/{id}/amenities
        [HttpPost("{id}/amenities")]
        public IActionResult PostAmenity(int id, CreatePlaceAmenityInputModel model)
        {
            bool exists = _context.Places.Any(place => place.Id == id);

            if (!exists) NotFound();

            PlaceAmenity amenity = new PlaceAmenity(model.Description, id);
            _context.PlaceAmenities.Add(amenity);
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE api/places/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Place? place = _context.Places.SingleOrDefault(place => place.Id == id);

            if (place is null) NotFound();

            place?.SetAsDeleted();
            _context.SaveChanges();

            return NoContent();
        }

        // POST api/places/{id}/books
        [HttpPost("{id}/books")]
        public IActionResult PostBook(int id, CreateInputBookModel model)
        {
            Place? place = _context.Places.SingleOrDefault(place => place.Id == id);

            if (place is null) NotFound();

            PlaceBook book = new PlaceBook(
                model.IdUser,
                model.IdPlace,
                model.StartDate,
                model.EndDate,
                model.Comments
            );

            _context.PlaceBooks.Add(book);
            _context.SaveChanges();

            return NoContent();
        }

        // POST api/places/{id}/comments
        [HttpPost("{id}/comments")]
        public IActionResult PostComment(int id, CreateCommentInputModel model)
        {
            return NoContent();
        }

        // POST api/places/{id}/photos
        [HttpPut("{id}/photos")]
        public IActionResult PostPlacePhotos(int id, IFormFile file)
        {
            var description = $"File: {file.FileName}\nSize: {file.Length}";

            using var ms = new MemoryStream();

            file.CopyTo(ms);
            var fileBytes = ms.ToArray();
            var base64 = Convert.ToBase64String(fileBytes);
            return Ok(new { description, base64 });

        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlaceRentalApi.API.Entities;
using PlaceRentalApi.API.Models;
using PlaceRentalApi.API.Persistence;

namespace PlaceRentalApi.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly PlaceRentalDbContext _context;

        public UsersController(PlaceRentalDbContext context)
        {
            _context = context;
        }

        // GET api/users/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            User? user = _context.Users.FirstOrDefault(user => user.Id == id);

            if (user is null) NotFound();

            return Ok(user);
        }

        // POST api/users
        [HttpPost]
        public IActionResult Post(CreateUserInputModel model)
        {
            User user = new User(
                model.FullName,
                model.Email,
                model.BirthDate
            );

            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, model);
        }
    }
}

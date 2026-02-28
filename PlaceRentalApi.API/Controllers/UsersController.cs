using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlaceRentalApi.API.Models;

namespace PlaceRentalApi.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult Post(CreateUserInputModel model)
        {
            return CreatedAtAction(nameof(GetById), new { id = 1 }, model);
        }
    }
}

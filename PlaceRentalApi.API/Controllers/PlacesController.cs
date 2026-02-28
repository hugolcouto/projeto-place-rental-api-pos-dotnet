using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlaceRentalApi.API.Models;

namespace PlaceRentalApi.API.Controllers
{
    [Route("api/places")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        // GET api/places
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }

        // GET api/places/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            return Ok();
        }

        // POST api/places
        [HttpPost]
        public IActionResult Post(CreatePlaceInputModel model)
        {
            return CreatedAtAction(nameof(GetById), new { id = 1 }, model);
        }

        // PUT api/places/{id}
        [HttpPut("{id}")]
        public IActionResult Put(int id, UpdatePlaceInputModel model)
        {
            return NoContent();
        }

        // POST api/places/{id}/amenities
        [HttpPost("{id}/amenities")]
        public IActionResult PostAmenity(int id, CreatePlaceAmenityInputModel model)
        {
            return NoContent();
        }

        // DELETE api/places/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return NoContent();
        }

        [HttpPost("{id}/books")]
        public IActionResult PostBook(int id, CreateInputBookModel model)
        {
            return NoContent();
        }

        [HttpPost("{id}")]
        public IActionResult PostComment(int id, CreateCommentInputModel model)
        {
            return NoContent();
        }
    }
}

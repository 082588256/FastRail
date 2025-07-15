using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.DTOs;
using Project.Services.Carriage;
using System.Reflection.Metadata.Ecma335;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarriageController : ControllerBase
    {
        private ICarriageService carriageService;

        public CarriageController(ICarriageService carriageService)
        {
            this.carriageService = carriageService;
        }
        [HttpGet]
        public async Task<IActionResult> getAll()
        {
            var result= await carriageService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> getCarriageAsync([FromRoute] int id)
        {
            var result= await carriageService.GetByIdAsync(id);
            if(result == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> createNewCarriage([FromBody] CarriageDto carriageDto)
        {
            var result= await carriageService.CreateAsync(carriageDto);
            return result.Success ? Ok(result) : BadRequest(result.Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> updateCarriage([FromRoute] int id, [FromBody] CarriageDto carriageDto)
        {
            var result = await carriageService.UpdateAsync(id, carriageDto);
            return result.Success ? Ok(result.Message) : BadRequest(result.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteCarriage([FromRoute] int id)
        {
            var result= await carriageService.DeleteAsync(id);
            return result ? NoContent() : BadRequest();
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.DTOs;
using Project.Services.Train;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainController : ControllerBase
    {
        private readonly ITrainService _trainService;

        public TrainController(ITrainService trainService)
        {
            _trainService = trainService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTrains()
        {
            var trainList = await _trainService.GetAllTrainsAsync();
            return Ok(trainList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrain(int id)
        {
            var train = await _trainService.GetTrainByIdAsync(id);
            return train == null ? NotFound() : Ok(train);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrain([FromBody] TrainDTO trainDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result= await _trainService.CreateTrainAsync(trainDTO);
            if (!result.success) return BadRequest(result.message);
            return CreatedAtAction(nameof(CreateTrain), new {id= result.id }, trainDTO);
            
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrain([FromBody] TrainDTO trainDTO, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result= await _trainService.UpdateTrainAsync(trainDTO, id);
            if (!result.success) return BadRequest(result.message);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrain(int id)
        {
            var delete= await _trainService.DeleteTrainAsync(id);
            if(!delete) return NotFound();
            return NoContent();
        }
    }
}

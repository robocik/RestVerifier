using Microsoft.AspNetCore.Mvc;
using RestVerifier.Tests.AspNetCore.Model;

namespace RestVerifier.Tests.AspNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetMethod1")]
        public IEnumerable<WeatherForecast> GetMethod1()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("GetMethod2")]
        public void GetMethod2()
        {

        }

        [HttpGet("GetPerson")]
        public PersonDTO GetPerson(Guid id)
        {
            return null;
        }
        [HttpGet("GetPersonWithException")]
        public PersonDTO GetPersonWithException(Guid id)
        {
            throw new ObjectDisposedException("test");
        }
        
        [HttpGet("GetPersonAction")]
        public IActionResult GetPersonAction(Guid id)
        {
            return null;
        }

        [HttpGet("GetPersonTaskAction")]
        public Task<IActionResult> GetPersonTaskAction(Guid id)
        {
            return null;
        }

        [HttpPost("UpdatePersonName")]
        public Task<IActionResult> UpdatePersonName([FromBody] (Guid id, string personName) param)
        {
            return null;
        }

        [HttpPost("GetFile")]
        public Task<IActionResult> GetFile()
        {
            return null;
        }

        [HttpGet("ParametersOrder")]
        [Produces("text/json")]
        public string ParametersOrder(string name,string address)
        {
            return null;
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> DeleteNote(Guid? id)
        {
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet("GetFileContent")]
        public FileStreamResult GetFileContent(string name)
        {
            return null;
        }

        [HttpGet("GetStatus")]
        public int GetStatus(int value, byte testMode)
        {
            return 0;
        }
    }
}
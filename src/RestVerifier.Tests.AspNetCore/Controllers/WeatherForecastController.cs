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
    }
}
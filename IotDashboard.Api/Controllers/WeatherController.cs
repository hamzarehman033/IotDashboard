using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;
using System.Net.Mime;

namespace IotDashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : BaseController<WeatherVM>
    {
        private readonly IWeatherHandler _weatherHandler;
        public WeatherController(IWeatherHandler weatherHandler):base(weatherHandler) 
        {
            _weatherHandler = weatherHandler;
        }

        [HttpGet("Report")]
        public async Task<IActionResult> Report()
        {
            return File(await _weatherHandler.GetReport(), "application/pdf", "weather.pdf");
        }
    }
}

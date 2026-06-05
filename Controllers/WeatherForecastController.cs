using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ANGDDEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
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
        [HttpGet]
        [ServiceFilter(typeof(AuthorizeUser))]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            //string strCurrentUser = _httpContextAccessor.HttpContext.Session.GetString("CurrentUser");
           // string[] strLoginName = _httpContextAccessor.HttpContext.User.Identity.Name.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);

            //if (string.IsNullOrEmpty(strCurrentUser))
            //{
            //    //add to session
            //    _httpContextAccessor.HttpContext.Session.SetString("CurrentUser", "current user json");
            //}

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
        .ToArray();
        }
        [HttpPost]
        //[Consumes("text/plain")]
        public string PostTest(string id)
        {
            return "Success. Id=" + (id ?? "");
        }
    }
}

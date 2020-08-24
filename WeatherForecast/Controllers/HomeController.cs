using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using WeatherForecast.Models;
using WeatherForecast.Models.API;

namespace WeatherForecast.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static HttpClient client = new HttpClient() { BaseAddress = new Uri("http://www.7timer.info") } ;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Make an API GET Call to the following URL: 
            // http://www.7timer.info/bin/api.pl?lon=-76.8867&lat=40.2731&product=civillight&output=json

            try
            {
                var queryStringParams = new Dictionary<string, string>()
                {
                    {"lon", "-76.8867" },
                    {"lat", "40.2731" },
                    {"product","civillight" },
                    {"output", "json" }
                };
                var pathAndQuerystring = QueryHelpers.AddQueryString("/bin/api.pl", queryStringParams);
                var httpResponseMessage = await client.GetAsync(pathAndQuerystring);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var jsonResult = await httpResponseMessage.Content.ReadAsStringAsync();
                    return View(WeatherForecastResponse.FromJson(jsonResult));
                }
                else
                    return View("Error", new ErrorViewModel() { ErrorMessage = "API call failed. Please try again later." });
            }
            catch(Exception ex)
            {
                //TODO: log ex.Message
                return View("Error", new ErrorViewModel() { ErrorMessage = "An unexpected error occurred. Please try again later." });
            }
        }

        public IActionResult Source()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string errorMessage = "")
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorMessage = errorMessage });
        }
    }
}

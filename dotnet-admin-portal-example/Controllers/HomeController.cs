using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WorkOS.AdminPortalExampleApp.Models;
using WorkOS; // Import WorkOS Package

namespace WorkOS.AdminPortalExampleApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("provision")]
        public async Task<IActionResult> Provision()
        {
            // Initialize the WorkOS client with your WorkOS API Key.
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));
            // Initialize Portal Service.
            var portalService = new PortalService();

            // Pull Organization ID from user input and pass into API call options.
            var OrganizationId = Request.Form["OrganizationId"].ToString();
            var options = new GenerateLinkOptions
            {
                Intent = Intent.SSO,
                Organization = OrganizationId,
            };

            // Make API call to generate Admin Portal session link.
            var portalLink = await portalService.GenerateLink(options);

            // Redirect user to Admin Portal link.
            return Redirect(portalLink);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

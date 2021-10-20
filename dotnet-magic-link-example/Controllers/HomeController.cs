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
using WorkOS.MagicLinkExampleApp.Models;
using WorkOS; // Import WorkOS Package

namespace WorkOS.MagicLinkExampleApp.Controllers
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

        [HttpPost("passwordless")]
        public async Task<IActionResult> Passwordless()
        {
            // Initialize the WorkOS client with your WorkOS API Key.
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));

            var email = Request.Form["email"].ToString();

            // Initialize Passwordless Service.
            var passwordlessService = new PasswordlessService();

            // Set options for Create Passwordless Session API call.
            var options = new CreatePasswordlessSessionOptions
            {
                Email = email,
                Type = PasswordlessSessionType.MagicLink,
            };

            // API call to generate new Passwordless session.
            var session = await passwordlessService.CreateSession(options);
            // API call to send email, passing in ID of session generated above.
            await passwordlessService.SendSession(session.Id);

            return Redirect("Confirmation");
        }

        [HttpGet("confirmation")]
        public IActionResult Confirmation()
        {
            return View();
        }

        [HttpGet("success")]
        public async Task<IActionResult> Success([FromQuery(Name = "code")] string code)
        {
            // Set WorkOS Client ID.
            string clientId = Environment.GetEnvironmentVariable("WORKOS_CLIENT_ID");

            // Initialize SSO Service.
            var ssoService = new SSOService();

            // Set options for GetProfileAndToken API call.
            var options = new GetProfileAndTokenOptions
            {
                ClientId = clientId,
                Code = code,
            };
            // Fetch user profile using GetProfileAndToken API call.
            var profileAndToken = await ssoService.GetProfileAndToken(options);

            var profile = profileAndToken.Profile;

            // Pass profile to view.
            ViewBag.Profile = JsonSerializer.Serialize(profile);

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

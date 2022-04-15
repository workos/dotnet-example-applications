using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkOS.SSOExampleApp.Models;
using WorkOS; // Import WorkOS Package

namespace WorkOS.SSOExampleApp.Controllers
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
            // Redirect the user to the url generated above.
            return View("Index");
        }

        [Route("login")]
        public IActionResult Initialize(){
            // Initialize the WorkOS client with your WorkOS API Key.
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));

            // Initialize SSOService()
            var ssoService = new SSOService();

            /*
            Set option for the getAuthorizationURL call, passing the domain (or Connection ID),
            the redirect URI (optional, otherwise it will use your default set in the Dashboard)
            and the clientID. Store the resulting URL in a `url` variable.
            se domain associated to the organization in which your SSO Connection resides.
            Alternatively, if your Organization has multiple SSO Connections within it,
            you can pass the Connection ID instead of the domain.
            */
            var options = new GetAuthorizationURLOptions
            {
                ClientId = Environment.GetEnvironmentVariable("WORKOS_CLIENT_ID"),
                Connection = "conn_01FKEGPNAQ7D2E8XGESC75YDEZ",
                RedirectURI = "https://localhost:5001/Home/Callback",
            };

            // Call GetAuthorizationURL and store the resulting URL in a `url` variable.
            string url = ssoService.GetAuthorizationURL(options);

            return Redirect(url);
        }

        // Capture and save the `code` passed as a querystring in the Redirect URI.
        public async Task<IActionResult> Callback([FromQuery(Name = "code")] string code)
        {
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));
            // Store client ID.
            string clientId = Environment.GetEnvironmentVariable("WORKOS_CLIENT_ID");

            // Initialize SSOService().
            var ssoService = new SSOService();

            // Set options for GetProfileAndToken call. The client ID and the code stored above are required.
            var options = new GetProfileAndTokenOptions
            {
                ClientId = clientId,
                Code = code,
            };

            //
            // Make a call to getProfileAndToken, store result in `profileAndToken`.
            var profileAndToken = await ssoService.GetProfileAndToken(options);

            // Extract profile and store in `profile`.
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

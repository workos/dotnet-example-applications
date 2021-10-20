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
using WorkOS.DSyncExampleApp.Models;
using WorkOS; // Import WorkOS Package

namespace WorkOS.DSyncExampleApp.Controllers
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
            //Pull and store Directory ID from environment variables.
            var directoryId = Environment.GetEnvironmentVariable("WORKOS_DIRECTORY_ID");
            ViewBag.Id = directoryId;

            return View();
        }

        // Capture and save the `code` passed as a querystring in the Redirect URI.
        public async Task<IActionResult> Users()
        {
            // Initialize the WorkOS client with your WorkOS API Key.
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));
            // Initialize WorkOS Directory Service.
            var directorySync = new DirectorySyncService();
            // Set Directory ID as option for our API call
            var options = new ListUsersOptions
            {
                Directory = Environment.GetEnvironmentVariable("WORKOS_DIRECTORY_ID"),
            };
            // API Call to list all users within our specified directory.
            WorkOSList<User> UserList = await directorySync.ListUsers(options);
            // Parse response and send to view.
            var users = JObject.Parse(JsonSerializer.Serialize(UserList))["Data"];
            ViewBag.Users = users;


            return View();

        }

        public async Task<IActionResult> Groups()
        {
            // Initialize the WorkOS client with your WorkOS API Key.
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));
            // Initialize WorkOS Directory Service.
            var directorySync = new DirectorySyncService();
            // Set Directory ID as option for our API call.
            var groupsOptions = new ListGroupsOptions
            {
                Directory = Environment.GetEnvironmentVariable("WORKOS_DIRECTORY_ID"),
            };
            // API Call to list all groups within our specified directory.
            WorkOSList<Group> GroupList = await directorySync.ListGroups(groupsOptions);
            // Parse response and send to view.
            var groups = JObject.Parse(JsonSerializer.Serialize(GroupList))["Data"];
            ViewBag.Groups = groups;

            return View();

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

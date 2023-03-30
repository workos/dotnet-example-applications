using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SocketIOClient;
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

        [Route("/{cursor?}/{type?}")]
        public async Task<IActionResult> Index(string cursor, string type)
        {
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));

            var directorySync = new DirectorySyncService();

            var cursorId = cursor;
            var cursorType = type;
            var options = new ListDirectoriesOptions {
                Limit = 5
            };
            if (cursorType == "before")
            {
                options = new ListDirectoriesOptions {
                    Limit = 5,
                    Before = cursorId,
                };
            }
            else if (cursorType == "after")
            {
                options = new ListDirectoriesOptions {
                    Limit = 5,
                    After = cursorId,
                };
            }
            List<Directory> directoryList = new List<Directory>();
            var directories = await directorySync.ListDirectories(options);

            ViewData["directoryList"] = directories.Data;
            ViewData["before"] = directories.ListMetadata.Before;
            ViewData["after"] = directories.ListMetadata.After;

            return View();
        }

        [Route("directory/{id?}")]
        [HttpGet]
        public async Task<IActionResult> Directory(string id)
        {
            var directorySync = new DirectorySyncService();

            var directory = await directorySync.GetDirectory(id);
            var serializeOptions = new JsonSerializerOptions { WriteIndented = true };
            ViewBag.CurrentDirectory = JsonSerializer.Serialize(directory, serializeOptions);
            ViewBag.CurrentDirectoryId = id;
            ViewBag.CurrentDirectoryName = JsonSerializer.Serialize(directory.Name, serializeOptions);
            ViewBag.CurrentDirectoryType = JsonSerializer.Serialize(directory.Type, serializeOptions);
            ViewBag.CurrentDirectoryDomain = JsonSerializer.Serialize(directory.Domain, serializeOptions);
            ViewBag.CurrentDirectoryCreatedAt = JsonSerializer.Serialize(directory.CreatedAt, serializeOptions);
            return View();

        }

        [Route("user/{id?}/{directoryId}")]
        public async Task<IActionResult> User(string id, string directoryId)
        {
            var directorySync = new DirectorySyncService();

            var directId = directoryId;
            var directory = await directorySync.GetDirectory(directId);
            var userId = id;
            var user = await directorySync.GetUser(userId);

            var serializeOptions = new JsonSerializerOptions { WriteIndented = true };
            ViewBag.Directory = directory;
            ViewBag.DirectoryName = directory.Name;
            ViewBag.FirstName = user.FirstName;
            ViewBag.LastName = user.LastName;
            ViewBag.User = JsonSerializer.Serialize(user, serializeOptions);

            return View();
        }

        [Route("group/{id?}/{directoryId?}")]
        public async Task<IActionResult> Group(string id, string directoryId)
        {
            var directorySync = new DirectorySyncService();

            var directId = directoryId;
            var directory = await directorySync.GetDirectory(directId);

            var directoryGroupId = id;
            var directoryGroup = await directorySync.GetGroup(directoryGroupId);

            var serializeOptions = new JsonSerializerOptions { WriteIndented = true };
            ViewBag.Directory = directory;
            ViewBag.DirectoryName = directory.Name;
            ViewBag.GroupName = directoryGroup.Name;
            ViewBag.Group = JsonSerializer.Serialize(directoryGroup, serializeOptions);

            return View();
        }

        [Route("groupsandusers/{id?}")]
        public async Task<IActionResult> UsersGroups(string id)
        {
            var serializeOptions = new JsonSerializerOptions { WriteIndented = true };

            var directorySync = new DirectorySyncService();

            var directory = await directorySync.GetDirectory(id);

            // Group Logic
            var groupOptions = new ListGroupsOptions
            {
                Directory = id,
            };

            var groups = await directorySync.ListGroups(groupOptions);

            List<Group> groupList = new List<Group>();
            groupList = groups.Data;
            ViewData["groupList"] = groupList;
            ViewBag.Groups = groups;
            ViewBag.CurrentDirectoryName = JsonSerializer.Serialize(directory.Name, serializeOptions);

            // User logic
            var userOptions = new ListUsersOptions
            {
                Directory = id,
            };
            var users = await directorySync.ListUsers(userOptions);

            List<User> userList = new List<User>();
            userList = users.Data;
            ViewData["userList"] = userList;
            ViewBag.Users = users;

            return View();
        }


        [HttpPost]
        [Route("webhook")]
        public async Task<IActionResult> Webhook()
        {
            SocketIO client = new SocketIO("https://localhost:5000/");
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Console.WriteLine(json);
            // Webhooks Validation variables
            var webhookEvent = json;
            var signatureHeader = Request.Headers["WorkOS-Signature"];
            var secret = Environment.GetEnvironmentVariable("WORKOS_WEBHOOK_SECRET");
            var test = new WebhookService();
            // Validate webhook and return deseralized object of payload in testResults
            Webhook testResults = test.ConstructEvent(json, signatureHeader, secret, 300);

            await client.EmitAsync("webhook_recieved", webhookEvent);

            return Ok();
        }

        [HttpGet]
        [Route("webhooks")]
        public async Task<IActionResult> Webhooks()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

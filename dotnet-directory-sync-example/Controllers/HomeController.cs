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

        public async Task<IActionResult> Index()
        {
            // Initialize the WorkOS client with your WorkOS API Key.
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));
            // Initialize WorkOS Directory Service.
            var directorySync = new DirectorySyncService();
            //Pull and store Directory ID from environment variables.
            List<Directory> directoryList = new List<Directory>();
            var directories = await directorySync.ListDirectories();
            directoryList = directories.Data;
            ViewData["directoryList"] = directoryList;
            return View();
        }

        [Route("directory/{id?}")]
        [HttpGet]
        public async Task<IActionResult> Directory(string id)
        {
            // Initialize WorkOS Directory Service.
            var directorySync = new DirectorySyncService();
            // Get Directory
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

        [Route("user/{id?}")]
        public async Task<IActionResult> Users(string id)
        {
            // Initialize WorkOS Directory Service.
            var directorySync = new DirectorySyncService();
            // Set Directory ID as option for our API call
            var options = new ListUsersOptions
            {
                Directory = id,
            };
            // API Call to list all users within our specified directory.
            var users = await directorySync.ListUsers(options);
            // Parse response and send to view.
            List<User> userList = new List<User>();
            userList = users.Data;
            ViewData["userList"] = userList;
            ViewBag.Users = users;

            return View();
        }

        [Route("group/{id?}/{directoryId?}")]
        public async Task<IActionResult> Group(string id, string directoryId)
        {
            // Initialize WorkOS Directory Service.
            var directorySync = new DirectorySyncService();
            // Set Directory ID as option for our API call
            var directId = directoryId;
            var directory = await directorySync.GetDirectory(directId);
            var directoryGroupId = id;
            // API Call to list all users within our specified directory.
            var directoryGroup = await directorySync.GetGroup(directoryGroupId);
            // Parse response and send to view.
            // List<Group> groupList = new List<Group>();
            // groupList = directoryGroup;
            var serializeOptions = new JsonSerializerOptions { WriteIndented = true };
            ViewData["directory"] = directory;
            ViewBag.Directory = directory;
            ViewBag.DirectoryName = directory.Name;
            ViewData["group"] = directoryGroup;
            ViewBag.GroupName = directoryGroup.Name;
            ViewBag.Group = JsonSerializer.Serialize(directoryGroup, serializeOptions);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.Directory);
            Console.WriteLine(json);

            return View();
        }

        [Route("groupsandusers/{id?}")]
        public async Task<IActionResult> Groups(string id)
        {
            var serializeOptions = new JsonSerializerOptions { WriteIndented = true };
            // Initialize WorkOS Directory Service.
            var directorySync = new DirectorySyncService();
            // Grab directory to populate view details
            var directory = await directorySync.GetDirectory(id);
            // Set Directory ID as option for our API call.
            var groupOptions = new ListGroupsOptions
            {
                Directory = id,
            };
            // API Call to list all groups within our specified directory.
            var groups = await directorySync.ListGroups(groupOptions);
            // Parse response and send to view.
            List<Group> groupList = new List<Group>();
            groupList = groups.Data;
            ViewData["groupList"] = groupList;
            ViewBag.Groups = groups;
            ViewBag.CurrentDirectoryName = JsonSerializer.Serialize(directory.Name, serializeOptions);
            // string json = Newtonsoft.Json.JsonConvert.SerializeObject(ViewData["groupList"]);
            // Console.WriteLine(json);

            // User logic
            var userOptions = new ListUsersOptions
            {
                Directory = id,
            };
            var users = await directorySync.ListUsers(userOptions);
            // Parse response and send to view.
            List<User> userList = new List<User>();
            userList = users.Data;
            ViewData["userList"] = userList;
            ViewBag.Users = users;
            // string json = Newtonsoft.Json.JsonConvert.SerializeObject(ViewData["userList"]);
            // Console.WriteLine(json);
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

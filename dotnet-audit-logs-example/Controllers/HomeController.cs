using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WorkOS.AuditLogExampleApp.Constants;
using WorkOS.AuditLogExampleApp.Models;
using WorkOS; // Import WorkOS Package

namespace WorkOS.AuditLogExampleApp.Controllers
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
            // Initialize the WorkOS client with your WorkOS API Key.
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));
            // Initialize WorkOS Audit Logs and Organization Service.
            var auditLogs = new AuditLogsService();
            var organizationsService = new OrganizationsService();
            var cursorId = cursor;
            var cursorType = type;
            var options = new ListOrganizationsOptions {
                Limit = 5
            };
            if (cursorType == "before")
            {
                options = new ListOrganizationsOptions {
                    Limit = 5,
                    Before = cursorId,
                };
            }
            else if (cursorType == "after")
            {
                options = new ListOrganizationsOptions {
                    Limit = 5,
                    After = cursorId,
                };
            }
            var organizations = await organizationsService.ListOrganizations(options);
            //Check if an organization is already established in session.

            ViewData["organizationList"] = organizations.Data;
            ViewData["before"] = organizations.ListMetadata.Before;
            ViewData["after"] = organizations.ListMetadata.After;

            // string json = Newtonsoft.Json.JsonConvert.SerializeObject(ViewData["before"]);
            // Console.WriteLine(json);

            return View();
        }

        [Route("/send_event")]
        public IActionResult SendEvents()
        {
            // Initialize WorkOS Audit Logs Service.
            var auditLogs = new AuditLogsService();
            // Recieve event type.
            var eventtype = Request.Form["event"].ToString();
            AuditLogEvent eventPayload = null;
            switch (eventtype)
            {
                case "user_signed_in":
                    eventPayload = AuditLogMockEvents.UserSignedIn;
                    break;
                case "user_logged_out":
                    eventPayload = AuditLogMockEvents.UserLoggedOut;
                    break;
                case "user_org_deleted":
                    eventPayload = AuditLogMockEvents.UserOrganizationDeleted;
                    break;
                case "user_connection_deleted":
                    eventPayload = AuditLogMockEvents.UserConnectionDeleted;
                    break;
            }
            var options = new CreateAuditLogEventOptions()
            {
                OrganizationId = HttpContext.Session.GetString("organization_id"),
                Event = eventPayload,
            };
            auditLogs.CreateEvent(options);
            //Set ViewData for orgId and orgName.
            ViewData["OrgId"] = HttpContext.Session.GetString("organization_id");
            ViewData["OrgName"] = HttpContext.Session.GetString("organization_name");
            return View();
        }

        [Route("/set_org/{orgId?}")]
        public async Task<IActionResult> SendEvents(string orgId)
        {
            // Initialize WorkOS Organization Service.
            var organizationsService = new OrganizationsService();
            // Get the organization.
            var organizationId = orgId;
            var org = await organizationsService.GetOrganization(organizationId);
            ViewData["OrgId"] = org.Id;
            ViewData["OrgName"] = org.Name;
            // Set the org name and id in the session.
            HttpContext.Session.SetString("organization_id", org.Id);
            HttpContext.Session.SetString("organization_name", org.Name);
            return View();
        }

        [Route("/export_events")]
        public IActionResult ExportEvents()
        {
            //Set ViewData for orgId and orgName.
            ViewData["OrgId"] = HttpContext.Session.GetString("organization_id");
            ViewData["OrgName"] = HttpContext.Session.GetString("organization_name");
            return View();
        }

        [Route("/generate_csv")]
        public async Task<IActionResult> GenerateCSV()
        {
            //Initialize WorkOS Audit Logs Service.
            var auditLogs = new AuditLogsService();
            //Get time range of the past month.
            var currentDate = DateTime.Now;
            var dateOneMonthAgo = currentDate.AddMonths(-1);
            var options = new CreateAuditLogExportOptions()
            {
                OrganizationId = HttpContext.Session.GetString("organization_id"),
                RangeStart = DateTime.Now.AddMonths(-1),
                RangeEnd = DateTime.Now,
            };
            var auditLogExport = await auditLogs.CreateExport(options);
            HttpContext.Session.SetString("export_id", auditLogExport.Id);
            //Set ViewData for orgId and orgName.
            ViewData["OrgId"] = HttpContext.Session.GetString("organization_id");
            ViewData["OrgName"] = HttpContext.Session.GetString("organization_name");
            return View("ExportEvents");
        }

        [Route("/access_csv")]
        public async Task<IActionResult> AccessCSV()
        {
            var auditLogs = new AuditLogsService();
            var auditLogExport = await auditLogs.GetExport(HttpContext.Session.GetString("export_id"));
            //Set ViewData for orgId and orgName.
            ViewData["OrgId"] = HttpContext.Session.GetString("organization_id");
            ViewData["OrgName"] = HttpContext.Session.GetString("organization_name");
            return Redirect(auditLogExport.Url);
        }

        [Route("/logout")]
        public IActionResult Logout()
        {
            //Set ViewData for orgId and orgName.
            HttpContext.Session.SetString("organization_id", "");
            HttpContext.Session.SetString("organization_name", "");
            return View("Index");
        }

    }
}

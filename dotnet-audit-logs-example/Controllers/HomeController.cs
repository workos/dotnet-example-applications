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
            var eventAction = Request.Form["event-action"].ToString();
            var eventVersion = Int32.Parse(Request.Form["event-version"]);
            var actorName = Request.Form["actor-name"].ToString();
            var actorType = Request.Form["actor-type"].ToString();
            var targetName = Request.Form["target-name"].ToString();
            var targetType = Request.Form["target-type"].ToString();

            // Initialize WorkOS Audit Logs Service.
            var auditLogs = new AuditLogsService();
            var orgId = HttpContext.Session.GetString("organization_id");
            var idempotencyKey = "884793cd-bef4-46cf-8790-ed49257a09c6";

            var auditLogEvent = new AuditLogEvent {
                Action = eventAction,
                OccurredAt = DateTime.Now,
                Version = eventVersion,
                Actor =
                    new AuditLogEventActor {
                        Id = "user_01GBNJC3MX9ZZJW1FSTF4C5938",
                        Type = actorType,
                        Name = actorName
                    },
                Targets =
                    new List<AuditLogEventTarget>() {
                        new AuditLogEventTarget {
                            Id = "team_01GBNJD4MKHVKJGEWK42JNMBGS",
                            Type = targetType,
                            Name = targetName
                        },
                    },
                Context =
                    new AuditLogEventContext {
                        Location = "123.123.123.123",
                        UserAgent = "Chrome/104.0.0.0",
                    },
            };

            var options = new CreateAuditLogEventOptions() {
                OrganizationId = orgId,
                Event = auditLogEvent
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
            var now = DateTime.Now.ToUniversalTime();
            var monthAgo = now.AddMonths(-1).ToUniversalTime();

            ViewData["OrgId"] = org.Id;
            ViewData["OrgName"] = org.Name;
            ViewData["RangeStart"] = monthAgo.ToString("o");
            ViewData["RangeEnd"] = now.ToString("o");
            // Set the org name and id in the session.
            HttpContext.Session.SetString("organization_id", org.Id);
            HttpContext.Session.SetString("organization_name", org.Name);
            return View();
        }

        [Route("/csv")]
        public async Task<IActionResult> CSV()
        {
            var buttonClicked = Request.Form["event"].ToString();

            //Initialize WorkOS Audit Logs Service.
            var auditLogs = new AuditLogsService();

            if (buttonClicked == "generate_csv")
            {
                var rangeStart = Request.Form["range-start"].ToString();
                var rangeEnd = Request.Form["range-end"].ToString();
                var filterActions = Request.Form["filter-actions"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList();
                var filterActors = Request.Form["filter-actors"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList();
                var filterTargets = Request.Form["filter-targets"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList();

                // string json = Newtonsoft.Json.JsonConvert.SerializeObject(filterActions);
                // Console.WriteLine(json);

                var options = new CreateAuditLogExportOptions()
                {
                    OrganizationId = HttpContext.Session.GetString("organization_id"),
                    RangeStart = DateTime.Now.AddMonths(-1),
                    RangeEnd = DateTime.Now,
                };

                if (filterActions?.Count > 0)
                {
                    options.Actions = filterActions;
                }

                if (filterActors?.Count > 0)
                {
                    options.Actors = filterActors;
                }

                if (filterTargets?.Count > 0)
                {
                    options.Targets = filterTargets;
                }

                var auditLogExport = await auditLogs.CreateExport(options);


                HttpContext.Session.SetString("export_id", auditLogExport.Id);

                ViewData["OrgId"] = HttpContext.Session.GetString("organization_id");
                ViewData["OrgName"] = HttpContext.Session.GetString("organization_name");
                return View("SendEvents");
            }
            else if (buttonClicked == "access_csv")
            {
                var auditLogExport = await auditLogs.GetExport(HttpContext.Session.GetString("export_id"));
                //Set ViewData for orgId and orgName.
                ViewData["OrgId"] = HttpContext.Session.GetString("organization_id");
                ViewData["OrgName"] = HttpContext.Session.GetString("organization_name");
                return Redirect(auditLogExport.Url);
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("/events/{intent?}")]
        public async Task<IActionResult> Events(string intent)
        {
            var intentType = intent;
            var orgId = HttpContext.Session.GetString("organization_id");

            var portalService = new PortalService();

            var options = new GenerateLinkOptions {
                Organization = orgId
            };

            if (intentType == "AuditLogs")
                {
                    options.Intent = Intent.AuditLogs;
                }
            if (intentType == "LogStreams")
            {
                options.Intent = Intent.LogStreams;
            }

            var link = await portalService.GenerateLink(options);

            return Redirect(link);
        }

        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            var organizationsService = new OrganizationsService();

            var options = new ListOrganizationsOptions {
                Limit = 5
            };

            var organizations = await organizationsService.ListOrganizations(options);
            //Check if an organization is already established in session.

            ViewData["organizationList"] = organizations.Data;
            ViewData["before"] = organizations.ListMetadata.Before;
            ViewData["after"] = organizations.ListMetadata.After;
            //Set ViewData for orgId and orgName.
            HttpContext.Session.SetString("organization_id", "");
            HttpContext.Session.SetString("organization_name", "");
            return View("Index");
        }

    }
}

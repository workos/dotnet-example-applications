using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Globalization;
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
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));

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

            ViewData["organizationList"] = organizations.Data;
            ViewData["before"] = organizations.ListMetadata.Before;
            ViewData["after"] = organizations.ListMetadata.After;

            return View();
        }

        [Route("/send_event")]
        public IActionResult SendEvents()
        {
            var eventVersion = Int32.Parse(Request.Form["event-version"]);
            var actorName = Request.Form["actor-name"].ToString();
            var actorType = Request.Form["actor-type"].ToString();
            var targetName = Request.Form["target-name"].ToString();
            var targetType = Request.Form["target-type"].ToString();

            var auditLogs = new AuditLogsService();
            var orgId = HttpContext.Session.GetString("organization_id");
            var idempotencyKey = "884793cd-bef4-46cf-8790-ed49257a09c6";

            var auditLogEvent = new AuditLogEvent {
                Action = "user.organization_deleted",
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

            ViewData["OrgId"] = HttpContext.Session.GetString("organization_id");
            ViewData["OrgName"] = HttpContext.Session.GetString("organization_name");
            return View();
        }

        [Route("/set_org/{orgId?}")]
        public async Task<IActionResult> SendEvents(string orgId)
        {
            var organizationsService = new OrganizationsService();

            var organizationId = orgId;
            var org = await organizationsService.GetOrganization(organizationId);
            var now = DateTime.Now.ToUniversalTime();
            var monthAgo = now.AddMonths(-1).ToUniversalTime();

            ViewData["OrgId"] = org.Id;
            ViewData["OrgName"] = org.Name;
            ViewData["RangeStart"] = monthAgo.ToString("o");
            ViewData["RangeEnd"] = now.ToString("o");

            HttpContext.Session.SetString("organization_id", org.Id);
            HttpContext.Session.SetString("organization_name", org.Name);
            return View();
        }

        [Route("/csv")]
        public async Task<IActionResult> CSV()
        {
            var buttonClicked = Request.Form["event"].ToString();

            var auditLogs = new AuditLogsService();

            if (buttonClicked == "generate_csv")
            {
                var rangeStart = Request.Form["range-start"].ToString();
                var rangeStartParsed = DateTime.ParseExact(rangeStart, "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                var rangeEnd = Request.Form["range-end"].ToString();
                var rangeEndParsed = DateTime.ParseExact(rangeEnd, "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                var filterActions = Request.Form["filter-actions"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList();
                var filterActors = Request.Form["filter-actors"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList();
                var filterTargets = Request.Form["filter-targets"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList();
                var dateTimeTest = DateTime.Now;

                var options = new CreateAuditLogExportOptions()
                {
                    OrganizationId = "org_01GB5X5TNYM2961QJQ23WPEGE0",
                    RangeStart = rangeStartParsed,
                    RangeEnd = rangeEndParsed,
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

                var auditLogExportJson = JsonConvert.SerializeObject(auditLogExport);

                HttpContext.Session.SetString("export_id", auditLogExport.Id);

                ViewData["OrgId"] = HttpContext.Session.GetString("organization_id");
                ViewData["OrgName"] = HttpContext.Session.GetString("organization_name");
                return View("SendEvents");
            }
            else if (buttonClicked == "access_csv")
            {
                var auditLogExport = await auditLogs.GetExport(HttpContext.Session.GetString("export_id"));

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

            ViewData["organizationList"] = organizations.Data;
            ViewData["before"] = organizations.ListMetadata.Before;
            ViewData["after"] = organizations.ListMetadata.After;

            HttpContext.Session.SetString("organization_id", "");
            HttpContext.Session.SetString("organization_name", "");
            return View("Index");
        }

    }
}

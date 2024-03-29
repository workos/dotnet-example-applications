﻿using System;
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

        [Route("provision")]
        [HttpPost]
        public async Task<IActionResult> Provision()
        {
            // Initialize the WorkOS client with your WorkOS API Key.
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));
            // Initialize Organziation Service.
            var organizationService = new OrganizationsService();

            // Pull Organization name and domains.
            var OrganizationName = Request.Form["org"].ToString();
            var DomainString = Request.Form["domain"].ToString();
            var OrganizationDomains = DomainString.Split(' ');

            // Check if a matching domain already exists

            var listOptions = new ListOrganizationsOptions
            {
                Domains =
                    new string[] {
                        DomainString,
                    },
            };

            var organizationList = await organizationService.ListOrganizations(listOptions);

            if (organizationList.Data.Count > 0)
            {
                TempData["OrganizationId"] = organizationList.Data[0].Id;
            }
            // Otherwise, create a new Organization
            else
            {
                var orgOptions = new CreateOrganizationOptions
                {
                    Name = OrganizationName,
                    Domains = OrganizationDomains,
                };

                var newOrganization = await organizationService.CreateOrganization(orgOptions);
                TempData["OrganizationId"] = newOrganization.Id;
            }




            // Redirect user to Admin Portal link.
            return View("LoggedIn");
        }

        [Route("sso")]
        [HttpPost]
        public async Task<IActionResult> AdminPortalSSO()
        {
            var portalService = new PortalService();
            var organizationId = TempData["OrganizationId"].ToString();
            var options = new GenerateLinkOptions
            {
                Intent = Intent.SSO,
                Organization = organizationId,
            };
            var portalLink = await portalService.GenerateLink(options);
            return Redirect(portalLink);
        }

        [Route("dsync")]
        [HttpPost]
        public async Task<IActionResult> AdminPortalDSync()
        {
            var portalService = new PortalService();
            var organizationId = TempData["OrganizationId"].ToString();
            var options = new GenerateLinkOptions
            {
                Intent = Intent.DSync,
                Organization = organizationId,
            };
            var portalLink = await portalService.GenerateLink(options);
            return Redirect(portalLink);
        }

        [Route("auditlogs")]
        [HttpPost]
        public async Task<IActionResult> AdminPortalAuditLogs()
        {
            var portalService = new PortalService();
            var organizationId = TempData["OrganizationId"].ToString();
            var options = new GenerateLinkOptions
            {
                Intent = Intent.AuditLogs,
                Organization = organizationId,
            };
            var portalLink = await portalService.GenerateLink(options);
            return Redirect(portalLink);
        }

        [Route("logstreams")]
        [HttpPost]
        public async Task<IActionResult> AdminPortalLogStreams()
        {
            var portalService = new PortalService();
            var organizationId = TempData["OrganizationId"].ToString();
            var options = new GenerateLinkOptions
            {
                Intent = Intent.LogStreams,
                Organization = organizationId,
            };
            var portalLink = await portalService.GenerateLink(options);
            return Redirect(portalLink);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

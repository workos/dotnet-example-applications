using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using WorkOS;

namespace WorkOS.DSyncExampleApp.Controllers
{
  [Route("webhook")]
  [ApiController]
  public class WebhookController : Controller
  {
    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        Console.WriteLine(json);
        var webhookEvent = json;
        var signatureHeader = Request.Headers["WorkOS-Signature"];
        return Ok();
    }
  }
}
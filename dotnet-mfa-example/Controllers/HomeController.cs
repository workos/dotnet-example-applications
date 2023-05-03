using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WorkOS.MFAExampleApp.Models;
using WorkOS;

namespace WorkOS.MFAExampleApp.Controllers
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

            // Initialize the WorkOS client with your WorkOS API Key.
            WorkOS.SetApiKey(Environment.GetEnvironmentVariable("WORKOS_API_KEY"));
            var service = new MfaService();
            List<Factor> factors = new List<Factor>();
            string value = HttpContext.Session.GetString("factors");
            if (value != null)
            {
                factors = JsonConvert.DeserializeObject<List<Factor>>(value);
            }
            ViewBag.factors = factors;

            // Direct to Challenge Factor View
            return View();
        }

        [HttpGet]
        [Route("EnrollFactor")]
        public async Task<IActionResult> EnrollFactor()
        {
            return View();
        }

        [HttpGet]
        [Route("ClearFactors")]
        public async Task<IActionResult> ClearFactors()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }

        [HttpPost]
        [Route("EnrollSmsFactor")]
        public async Task<IActionResult> EnrollSmsFactor()
        {
            var service = new MfaService();
            var type = Request.Form["type"].ToString();

            if (type == "sms")
            {
                var phoneNumber = "+1" + Request.Form["phone_number"].ToString();
                var options = new EnrollSmsFactorOptions(phoneNumber);

                //Enroll sms factor
                var newFactor = await service.EnrollFactor(options);

                //Add factor to factors list in session
                List<Factor> smsFactors = new List<Factor>();
                string sessionFactors = HttpContext.Session.GetString("factors");
                if (sessionFactors != null)
                {
                    smsFactors = JsonConvert.DeserializeObject<List<Factor>>(sessionFactors);
                }
                smsFactors.Add(newFactor);
                HttpContext.Session.SetString("factors", Newtonsoft.Json.JsonConvert.SerializeObject(smsFactors));
                //Redirect to Index
                return RedirectToAction("Index");
            }
            else
            {

                //Type not sms, return error view
                return View("Error");
            }
        }

        [Route("EnrollTotpFactor")]
        public async Task<IActionResult> EnrollTotpFactor()
        {
             // Get the JSON object from the request body
            using (var reader = new StreamReader(Request.Body))
            {
                var requestBody = await reader.ReadToEndAsync();

                // Deserialize the JSON object into an object with the same properties
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(requestBody);

                // Access the properties of the JSON object and assign them to variables
                var type = jsonObject.type.ToString();
                var issuer = jsonObject.issuer.ToString();
                var user = jsonObject.user.ToString();

                Console.WriteLine("Type: " + type);
                Console.WriteLine("Issuer: " + issuer);
                Console.WriteLine("User: " + user);

                var service = new MfaService();

                if (type == "totp")
                {
                    var options = new EnrollTotpFactorOptions(issuer, user);

                    // enroll totp factor
                    var newFactor = await service.EnrollFactor(options);
                    Console.WriteLine("This is the new totp factor: " + JsonConvert.SerializeObject(newFactor));

                    //Add factor to factors list in session
                    List<Factor> totpFactors = new List<Factor>();
                    string sessionFactors = HttpContext.Session.GetString("factors");
                    if (sessionFactors != null)
                    {
                        totpFactors = JsonConvert.DeserializeObject<List<Factor>>(sessionFactors);
                    }
                    totpFactors.Add(newFactor);
                    HttpContext.Session.SetString("factors", Newtonsoft.Json.JsonConvert.SerializeObject(totpFactors));

                    // Return the factor to the front-end for QR code scanning
                    return Json(newFactor);
                }
                else
                {
                    //Type not totp, return error view
                    return View("Error");
                }
            }
        }

        [Route("factor_detail/{id?}")]
        [HttpGet]
        public async Task<IActionResult> FactorDetail(string id)
        {
            Console.WriteLine("factor_detail route hit");
            var service = new MfaService();
            List<Factor> factors = new List<Factor>();
            string sessionFactors = HttpContext.Session.GetString("factors");
            factors = JsonConvert.DeserializeObject<List<Factor>>(sessionFactors);
            var selectedFactor = factors.Find(sFactor => sFactor.Id.Contains(id));
            HttpContext.Session.SetString("currentFactor", JsonConvert.SerializeObject(selectedFactor));
            ViewBag.currentFactor = selectedFactor;
            return View(nameof(FactorDetail));
        }

        [Route("ChallengeFactor")]
        [HttpPost]
        public IActionResult ChallengeFactor()
        {
            var service = new MfaService();
            var currentFactor = JsonConvert.DeserializeObject<Factor>(HttpContext.Session.GetString("currentFactor"));
            if (currentFactor.Type == "sms")
            {
                var message = Request.Form["sms_message"].ToString();
                HttpContext.Session.SetString("smsMessage", message);
                var options = new ChallengeSmsFactorOptions(message)
                {
                    FactorId = currentFactor.Id
                };
                var challenge = service.ChallengeFactor(options).Result;
                HttpContext.Session.SetString("challengeId", challenge.Id);
            }
            else if (currentFactor.Type == "totp")
            {
                var options = new ChallengeFactorOptions()
                {
                    FactorId = currentFactor.Id
                };
                var challenge = service.ChallengeFactor(options).Result;
                HttpContext.Session.SetString("challengeId", challenge.Id);
            }
            return View();
        }

        [Route("VerifyChallenge")]
        [HttpPost]
        public async Task<IActionResult> VerifyChallenge()
        {
            var service = new MfaService();
            var challengeId = HttpContext.Session.GetString("challengeId");
            var code1 = Request.Form["code-1"].ToString();
            var code2 = Request.Form["code-2"].ToString();
            var code3 = Request.Form["code-3"].ToString();
            var code4 = Request.Form["code-4"].ToString();
            var code5 = Request.Form["code-5"].ToString();
            var code6 = Request.Form["code-6"].ToString();
            var code = code1 + code2 + code3 + code4 + code5 + code6;
            var options = new VerifyChallengeOptions()
            {
                ChallengeId = challengeId,
                Code = code
            };
            var response = await service.VerifyChallenge(options);
            if (response is VerifyChallengeResponseSuccess successResponse)
            {
                ViewBag.successFactor = response;
                //Successful response, return to success view
                return View("ChallengeSuccess");
            }
            else return View("Error");
        }

        [Route("ChallengeSuccess")]
        [HttpGet]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

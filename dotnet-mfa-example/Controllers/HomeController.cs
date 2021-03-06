using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
            if(value != null ){
                factors = JsonConvert.DeserializeObject<List<Factor>>(value);
            }
            ViewBag.factors = factors;
            foreach(var factor in factors){
            }

            // Direct to Challenge Factor View
            return View();
        }

        [HttpPost]
        [Route("EnrollSmsFactor")]
        public async Task<IActionResult> EnrollSmsFactor()
        {
            var service = new MfaService();
            var type = Request.Form["type"].ToString();

            if(type == "sms"){
                var phoneNumber = "+1" + Request.Form["phone_number"].ToString();
                var options = new EnrollSmsFactorOptions(phoneNumber);

                //Enroll sms factor
                var newFactor = await service.EnrollFactor(options);

                //Add factor to factors list in session
                List<Factor> factors = new List<Factor>();
                string sessionFactors = HttpContext.Session.GetString("factors");
                if(sessionFactors != null ){
                    factors = JsonConvert.DeserializeObject<List<Factor>>(sessionFactors);
                }
                factors.Add(newFactor);
                HttpContext.Session.SetString("factors", Newtonsoft.Json.JsonConvert.SerializeObject(factors));
                //Redirect to Index
                return RedirectToAction("Index");
            } else{

            //Type not sms, return error view
            return View("Error");
            }
        }

        [Route("EnrollTotpFactor")]
        public async Task<IActionResult> EnrollTotpFactor()
        {
            var service = new MfaService();
            var type = Request.Form["type"].ToString();

            if (type == "totp"){
                var issuer = Request.Form["totp_issuer"].ToString();
                var user = Request.Form["totp_user"].ToString();
                var options = new EnrollTotpFactorOptions(issuer, user);

                // enroll totp factor
                var newFactor = await service.EnrollFactor(options);

                //Add factor to factors list in session
                List<Factor> factors = new List<Factor>();
                string sessionFactors = HttpContext.Session.GetString("factors");
                if(sessionFactors != null ){
                    factors = JsonConvert.DeserializeObject<List<Factor>>(sessionFactors);
                }
                factors.Add(newFactor);
                HttpContext.Session.SetString("factors", Newtonsoft.Json.JsonConvert.SerializeObject(factors));
                return RedirectToAction("Index");
            }else{

            //Type not totp, return error view
            return View("Error");
            }
        }

        [Route("factor_detail/{id?}")]
        [HttpGet]
        public async Task<IActionResult> FactorDetail(string id){
            var service = new MfaService();
            List<Factor> factors = new List<Factor>();
            string sessionFactors = HttpContext.Session.GetString("factors");
            factors = JsonConvert.DeserializeObject<List<Factor>>(sessionFactors);
            var selectedFactor = await service.GetFactor(id);
            HttpContext.Session.SetString("currentFactor", JsonConvert.SerializeObject(selectedFactor));
            ViewBag.currentFactor = selectedFactor;
            return View(nameof(FactorDetail));
        }

        [Route("ChallengeFactor")]
        [HttpPost]
        public IActionResult ChallengeFactor(){
            var service = new MfaService();
            var currentFactor = JsonConvert.DeserializeObject<Factor>(HttpContext.Session.GetString("currentFactor"));
            if (currentFactor.Type == "sms"){
                var message = Request.Form["sms_message"].ToString();
                HttpContext.Session.SetString("smsMessage", message);
                var options = new ChallengeSmsFactorOptions(message){
                    FactorId = currentFactor.Id
                };
                var challenge = service.ChallengeFactor(options).Result;
                HttpContext.Session.SetString("challengeId", challenge.Id);
            }
            else if (currentFactor.Type == "totp"){
                var options = new ChallengeFactorOptions(){
                    FactorId = currentFactor.Id
                };
                var challenge = service.ChallengeFactor(options).Result;
                HttpContext.Session.SetString("challengeId", challenge.Id);
            }
            return View();
        }

        [Route("VerifyFactor")]
        [HttpPost]
        public async Task<IActionResult> VerifyFactor(){
            var service = new MfaService();
            var challengeId = HttpContext.Session.GetString("challengeId");
            var code = Request.Form["code"].ToString();
            var options = new VerifyFactorOptions(){
                ChallengeId = challengeId,
                Code = code
            };
            var response = await service.VerifyFactor(options);
            if (response is VerifyFactorResponseSuccess successResponse)
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

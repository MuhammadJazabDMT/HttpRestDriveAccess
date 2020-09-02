using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HttpRestDriveAccess.Models;
using HttpRestDriveAccess.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using GData = Google.Apis.Drive.v3.Data;
using HttpRestDriveAccess.Helpers;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace HttpRestDriveAccess.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeServices homeServices;

        public HomeController(ILogger<HomeController> logger, IHomeServices _homeServices)
        {
            homeServices = _homeServices;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            string accessToken = HttpContext.Session.GetSession("_accessTokens");

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return RedirectToAction("Login","Home");
            }
            else
            {
                if (file == null || file.Length == 0)
                    return Content("file not selected");

                string uploadingFolder = $"{Path.GetPathRoot(Environment.SystemDirectory)}Google Drive Uploades";
                uploadingFolder = HomeServices.SaveFile(uploadingFolder, file);

                string serverPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploades");
                serverPath = HomeServices.SaveFile(serverPath, file);

                homeServices.UploadFile(file, accessToken);

                var list = homeServices.LoadFiles(accessToken);

                return View("Index", list);
            }
        }

        public IActionResult Download(string fileId)
        {
            string accessToken = HttpContext.Session.GetSession("_accessTokens");

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                homeServices.DownloadFile(fileId, accessToken);

                var list = homeServices.LoadFiles(accessToken);

                return View("Index", list);
            }
        }

        [HttpGet]
        [Route("google-login")]
        public IActionResult Login(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetSession("_accessTokens")))
            {
                AuthenticationProperties authenticationProperties = new AuthenticationProperties();
                authenticationProperties.RedirectUri = Url.Action("LoginCallback", "Home", new { returnUrl });

                return new ChallengeResult(GoogleDefaults.AuthenticationScheme, authenticationProperties);
            }
            else
            {
                string _accessTokens = HttpContext.Session.GetSession("_accessTokens");

                List<GoogleDriveFiles> googleDriveFiles = homeServices.LoadFiles(_accessTokens);

                return View("Index", googleDriveFiles);
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoginCallback(string returnUrl = "/")
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("Application");

            if (!authenticateResult.Succeeded)
                return Content(authenticateResult.Failure.Message); // TODO: Handle this better.

            string _accessTokens = authenticateResult.Properties.GetTokens().ToList().Find(x => x.Name == "access_token").Value;

            HttpContext.Session.SetSession("_accessTokens", _accessTokens);

            List<GoogleDriveFiles> googleDriveFiles = homeServices.LoadFiles(_accessTokens);

            return View("Index", googleDriveFiles);
        }

        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }
    }
}

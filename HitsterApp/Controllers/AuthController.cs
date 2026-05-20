using Google.Cloud.Firestore;
using HitsterApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace HitsterApp.Controllers;

public class AuthController : Controller
{
    private FirestoreDb GetDb()
    {
        string path = Path.Combine(
            Directory.GetCurrentDirectory(),
            "json",
            "serviceAccountKey.json"
        );

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

        return FirestoreDb.Create("hitsterapp-1902d");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
	public IActionResult Login(string username)
	{
		if (string.IsNullOrWhiteSpace(username))
		{
			return View();
		}

		HttpContext.Session.SetString("Username", username.Trim());

		return RedirectToAction("Index", "Home");
	}

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
namespace UnitedGrid.Controllers;

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UnitedGrid.ViewModels;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Chat");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuickTaxi.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}

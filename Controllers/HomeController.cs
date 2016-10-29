using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreAngular.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
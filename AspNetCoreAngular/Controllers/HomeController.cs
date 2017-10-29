using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreAngular.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var fileName = "home.html";
            var contentType = "text/html";
            return File(fileName, contentType);
        }
    }
}

using Business_School.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;

namespace Business_School.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()

        // El error no estaba en las cookies ni en ASP.NET Identity, ya que la sesión se mantenía correctamente
        // (la cookie seguía siendo válida tras reiniciar la aplicación). El problema estaba en la ruta inicial:
        // Home/Index redirigía siempre a Account/Login sin comprobar si el usuario ya estaba autenticado.
        // Esto ignoraba una sesión activa y daba la falsa sensación de que se había cerrado. La solución fue
        // comprobar User.Identity.IsAuthenticated en HomeController y redirigir al Dashboard cuando la sesión
        // ya existe, recuperando así el estado correcto de la aplicación.



        {
            return RedirectToAction("Index", "Dashboard");//POR ESO ES PARA LA COOKIE PARA QUE CUANDO CIERRE Y VUELVE A ENTRAR SE VAYA AL DASHBOARD
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
}

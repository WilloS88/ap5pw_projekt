using Microsoft.AspNetCore.Mvc;
using AP5PW_Helpdesk.Models;

namespace AP5PW_Helpdesk.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            // dočasný fake data, jen aby tabulka něco zobrazila
            var demo = new List<User> {
                new User { Id=1, FirstName="Jan", LastName="Novák", Nickname="jnovak", Email="jan@firma.cz" },
                new User { Id=2, FirstName="Petr", LastName="Dvořák", Nickname="pdvorak", Email="petr@firma.cz" }
            };
            return View(demo); // Views/Users/Index.cshtml
        }

        public IActionResult Create() => View();       // Views/Users/Create.cshtml
        public IActionResult Edit(int id) => View();    // Views/Users/Edit.cshtml
        public IActionResult Details(int id) => View(); // Views/Users/Details.cshtml
    }
}

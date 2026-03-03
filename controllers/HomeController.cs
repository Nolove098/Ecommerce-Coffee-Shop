using Microsoft.AspNetCore.Mvc;

namespace SaleStore.Controllers
{

    public class HomeController : Controller
    {

        public IActionResult Index() => View();

    }

}
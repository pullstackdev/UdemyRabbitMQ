using Microsoft.AspNetCore.Authorization; //for Authorize
using Microsoft.AspNetCore.Mvc;

namespace UdemyRabbitMQWeb.ExcelCreate.Controllers
{
    [Authorize] //sadece authenticate olanlar girebilecek
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

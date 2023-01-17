using Microsoft.AspNetCore.Authorization; //for Authorize
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UdemyRabbitMQWeb.ExcelCreate.Models;
using UdemyRabbitMQWeb.ExcelCreate.Services;

namespace UdemyRabbitMQWeb.ExcelCreate.Controllers
{
    [Authorize] //sadece authenticate olanlar girebilecek
    public class ProductController : Controller
    {
        private readonly AppDbContext _appDbContext; //db
        private readonly UserManager<IdentityUser> _userManager; //user işlemleri(IS)
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public ProductController(AppDbContext appDbContext, UserManager<IdentityUser> userManager, RabbitMQPublisher rabbitMQPublisher)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _rabbitMQPublisher= rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel() //async yaptık async ve task ile
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";
            //create edilecek excel için db'ye kayıt datası
            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };
            await _appDbContext.UserFiles.AddAsync(userFile); //await ile beklettim ama async olduğu için blocklama demiş oldum
            await _appDbContext.SaveChangesAsync();

            //PUBLİSHER ile rabbitmq message gönder
            _rabbitMQPublisher.Publish(new Shared.CreateExcelMessage() { FileId = userFile.Id });

            TempData["StartCreatingExcel"] = true; //bir requestten diğerine tempdata ile data taşınabilir (cookiede tuttuğu için)

            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            
            return View(await _appDbContext.UserFiles.Where(x=>x.UserId == user.Id).ToListAsync()); //file'ları döndü
        }
    }
}

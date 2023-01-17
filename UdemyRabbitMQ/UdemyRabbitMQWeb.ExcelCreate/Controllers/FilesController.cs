using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using UdemyRabbitMQWeb.ExcelCreate.Models;

namespace UdemyRabbitMQWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _appDbContext; //file'a ulaşmak için

        public FilesController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        //workservice excel'i oluşturunca clouda atmadığımız için wwwroot'a upload etmesi gerekiyor, dış dünyaya da api açılıyor, bu endpointe istek atsın ve upload etsin
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int fileId)//excel dosyası, hangi dosya için
        {
            //file'ı alıp wwwroota kaydet
            if (file is not { Length: > 0 })
            {
                return BadRequest();
            }

            var userFile = await _appDbContext.UserFiles.FirstAsync(x=>x.Id == fileId);
            var filePath = userFile.FileName + Path.GetExtension(file.FileName);
            
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath); //buraya kaydedilecek
            using FileStream stream = new(path, FileMode.Create);
            await file.CopyToAsync(stream);

            //dbDE ilgili alanlar update
            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;

            await _appDbContext.SaveChangesAsync();

            //signalr notification

            return Ok();
        }
    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore; //for IdentityDbContext
using Microsoft.EntityFrameworkCore;

namespace UdemyRabbitMQWeb.ExcelCreate.Models
{
    public class AppDbContext: IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {

        }
        public DbSet<UserFile> UserFiles { get; set; }
        //Identity ile ilgili diğer dbset'ler zaten IdentityDbContext içinde mevcuttur
    }
}

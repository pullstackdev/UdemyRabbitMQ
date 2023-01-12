using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using UdemyRabbitMQWeb.ExcelCreate.Models;

namespace UdemyRabbitMQWeb.ExcelCreate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            //proje aya�a kalkarken data seed edelim, using scope kulland�m ki bitince i�i ramden silsin
            using (var scope = host.Services.CreateScope())
            {
                var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>(); //db'ye gitti
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(); //IdentityServer UserManager'� kullaNd�k
                appDbContext.Database.Migrate(); //program aya�a kalkarken yukar�daki  appDbContext.Database.Migrate(); ifadesi migration update-migration edecek

                if (!appDbContext.Users.Any()) //user yok ise user datas� olu�tural�m
                {
                    userManager.CreateAsync(new IdentityUser() { UserName = "deneme", Email = "deneme@outlook.com" }, "Pas12*").Wait(); //WA�T ile beklettim, async'u sync'a �evirdim
                    userManager.CreateAsync(new IdentityUser() { UserName = "deneme2", Email = "deneme2@outlook.com" }, "Pas12*").Wait(); //WA�T ile beklettim, async'u sync'a �evirdim

                }
                //add-migration bas�nca datalar olu�tu seed edildi yani
                //update-migration'a gerek yok ��nk� program aya�a kalkarken yukar�daki  appDbContext.Database.Migrate(); ifadesi migration update edecek
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

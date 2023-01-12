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
            //proje ayaða kalkarken data seed edelim, using scope kullandým ki bitince iþi ramden silsin
            using (var scope = host.Services.CreateScope())
            {
                var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>(); //db'ye gitti
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(); //IdentityServer UserManager'ý kullaNdýk
                appDbContext.Database.Migrate(); //program ayaða kalkarken yukarýdaki  appDbContext.Database.Migrate(); ifadesi migration update-migration edecek

                if (!appDbContext.Users.Any()) //user yok ise user datasý oluþturalým
                {
                    userManager.CreateAsync(new IdentityUser() { UserName = "deneme", Email = "deneme@outlook.com" }, "Pas12*").Wait(); //WAÝT ile beklettim, async'u sync'a çevirdim
                    userManager.CreateAsync(new IdentityUser() { UserName = "deneme2", Email = "deneme2@outlook.com" }, "Pas12*").Wait(); //WAÝT ile beklettim, async'u sync'a çevirdim

                }
                //add-migration basýnca datalar oluþtu seed edildi yani
                //update-migration'a gerek yok çünkü program ayaða kalkarken yukarýdaki  appDbContext.Database.Migrate(); ifadesi migration update edecek
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

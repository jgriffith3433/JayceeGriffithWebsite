using ContainerNinja.Migrations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ContainerNinja
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args).Build();

            using var scope = builder.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            if (db.Database.GetPendingMigrations().Any())
            {
                db.Database.Migrate();
            }
            if (!db.Users.Any())
            {
                Task.Run(async () =>
                {
                    await db.Users.AddAsync(new Contracts.Data.Entities.User
                    {
                        EmailAddress = "admin@admin.com",
                        Password = "admin",
                        Role = Contracts.Enum.UserRole.Owner
                    });
                    await db.SaveChangesAsync();
                });
            }
            builder.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseUrls("http://0.0.0.0:5000/");//, "https://0.0.0.0:5001/"
                });
    }
}

using System;
using Apoteka.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neo4jClient;
using Neo4j.AspNetCore.Identity;
using Apoteka.Models;
using Microsoft.Extensions.Options;

[assembly: HostingStartup(typeof(Apoteka.Areas.Identity.IdentityHostingStartup))]
namespace Apoteka.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            //builder.ConfigureServices((context, services) => {
            //    services.AddDbContext<ApotekaContext>(options =>
            //        options.UseSqlServer(
            //            context.Configuration.GetConnectionString("ApotekaContextConnection")));

            //    services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //        .AddEntityFrameworkStores<ApotekaContext>();

            builder.ConfigureServices((services) =>
            {
                services.AddScoped<IGraphClient, GraphClient>(provider =>
                {
                    var options = provider.GetService<IOptions<Neo4jDbSettings>>();
                    var client = new GraphClient(new Uri(options.Value.uri),
                        username: options.Value.username, password: options.Value.password);
                    client.Connect();
                    return client;
                });

                services.AddNeo4jAnnotations<Neo4JContext>(); //services.AddNeo4jAnnotations();

                //services.AddIdentity<Korisnik, Neo4j.AspNetCore.Identity.IdentityRole>(options =>
                //{
                //    //var dataProtectionPath = Path.Combine(HostingEnvironment.WebRootPath, "identity-artifacts");
                //    //options.Cookies.ApplicationCookie.AuthenticationScheme = "ApplicationCookie";
                //    //options.Cookies.ApplicationCookie.DataProtectionProvider = DataProtectionProvider.Create(dataProtectionPath);

                //    options.Lockout.AllowedForNewUsers = true;

                //    // User settings
                //    options.User.RequireUniqueEmail = true;
                //})
                //.AddUserStore<UserStore<Korisnik>>()
                //.AddRoleStore<RoleStore<Neo4j.AspNetCore.Identity.IdentityRole>>()
                //.AddDefaultTokenProviders();

            });
        }
    }
}
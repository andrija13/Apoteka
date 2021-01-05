using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Neo4jClient.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Neo4j.AspNetCore.Identity;
using Neo4jClient;
using Apoteka.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Apoteka
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.Configure<Neo4jDbSettings>(Configuration.GetSection("Neo4jDbSettings"));
            services.AddScoped<IGraphClient, GraphClient>(provider =>
            {
                var options = provider.GetService<IOptions<Neo4jDbSettings>>();
                var client = new GraphClient(new Uri(options.Value.uri),
                    username: options.Value.username, password: options.Value.password);
                client.Connect();
                return client;
            });

            services.AddNeo4jAnnotations<Neo4JContext>(); //services.AddNeo4jAnnotations();

            services.AddIdentity<Korisnik, Neo4j.AspNetCore.Identity.IdentityRole>(options =>
            {
                //var dataProtectionPath = Path.Combine(HostingEnvironment.WebRootPath, "identity-artifacts");
                //options.Cookies.ApplicationCookie.AuthenticationScheme = "ApplicationCookie";
                //options.Cookies.ApplicationCookie.DataProtectionProvider = DataProtectionProvider.Create(dataProtectionPath);

                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddUserStore<UserStore<Korisnik>>()
            .AddRoleStore<RoleStore<Neo4j.AspNetCore.Identity.IdentityRole>>()
            .AddDefaultTokenProviders();


            //// Services used by identity
            ////services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
            //services.AddAuthentication(options =>
            //{
            //    // This is the Default value for ExternalCookieAuthenticationScheme
            //    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; //new IdentityCookieOptions().ExternalCookieAuthenticationScheme;
            //});

            // Hosting doesn't add IHttpContextAccessor by default
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddOptions();
            services.AddDataProtection();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}

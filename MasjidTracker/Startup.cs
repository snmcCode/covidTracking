using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using FrontEnd.Interfaces;
using FrontEnd.Services;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Azure.Identity;

namespace MasjidTracker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddControllersWithViews().AddRazorPagesOptions(options => {
                options.Conventions.AuthorizeFolder("Events");
            });
           
            services.AddMemoryCache();
            services.AddProgressiveWebApp();
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => { options.SlidingExpiration = true;
                options.ExpireTimeSpan = new TimeSpan(14, 1, 1, 1);
                options.LoginPath = "/Home/Index";
                });

            AddDependencies(services);
           
            services.AddDataProtection()
                   .PersistKeysToAzureBlobStorage(new Uri(Configuration["AZ_STORAGE_URL"]))
                  .ProtectKeysWithAzureKeyVault(new Uri(Configuration["VAULT_URL"]), new DefaultAzureCredential())
                  .SetDefaultKeyLifetime(new TimeSpan(720,02,02,02));


        }
        private void AddDependencies(IServiceCollection services)
        {
            services.AddTransient<ICacheableService, CacheableService>();
            
            services.AddHttpClient();

            services.AddTransient<IEventsService, EventsService>();
            services.AddTransient<IUserService, UserService>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.UseStatusCodePagesWithRedirects("Errors/{0}/");
             app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
            // app.UseStatusCodePagesWithReExecute("/Errors", "?statusCode={0}");


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
          
            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                // endpoints.MapRazorPages();
            });
        }
    }
}

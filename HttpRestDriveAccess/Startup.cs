using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using HttpRestDriveAccess.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HttpRestDriveAccess
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
            services.AddTransient<IHomeServices, HomeServices>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthentication(opts =>
            {
                opts.DefaultScheme = "Application";
                opts.DefaultChallengeScheme = "External";
            })
                .AddCookie("Application")
                .AddCookie("External")
                .AddGoogle(opts =>
                {                   
                    opts.ClientId = "202128850418-fekofnga40sd7mnsjtrejl6tcbjonlsi.apps.googleusercontent.com";
                    opts.ClientSecret = "VBt2dOwvIzqEmsDY_Gt-rwQQ";
                    opts.SaveTokens = true;
                    opts.Scope.Add(DriveService.Scope.Drive);
                    opts.Scope.Add(DriveService.Scope.DriveFile);
                    opts.Scope.Add(DriveService.Scope.DriveMetadata);
                });

            services.AddControllersWithViews();

            services.AddSession(opts =>
            {
                opts.IdleTimeout = TimeSpan.FromMinutes(10);
            });

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAll", opts =>
            //    {
            //        opts.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            //    });
            //});
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
            }

            app.UseRouting();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

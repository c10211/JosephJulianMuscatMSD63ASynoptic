using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using BlankWebApplication.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Google.Cloud.SecretManager.V1;
using Newtonsoft.Json.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace BlankWebApplication
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
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("GoogleConsolePostgresConnection")));

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();

            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\shortitle\\jjmsynoptic-c226b4a074b5.json");

            var clientId = GetGoogleIdOrSecret("Authentication:Google:ClientId");
            var clientSecret = GetGoogleIdOrSecret("Authentication:Google:ClientSecret");

            services.AddAuthentication().AddGoogle(options =>
            {
                IConfigurationSection googleAuthNSection = Configuration.GetSection("Authentication:Google");

                // Get Client ID and Secret from the cloud console secrets
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        public string GetGoogleIdOrSecret(string key)
        {
            SecretManagerServiceClient client = SecretManagerServiceClient.Create();
            
            SecretVersionName secretVersionName = new SecretVersionName("jjmsynoptic", "ClientIDandSecret", "1");

            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);

            string payload = result.Payload.Data.ToStringUtf8();

            JObject jObject = JObject.Parse(payload);
            JToken jKey = jObject[key];
            return jKey.ToString();
        }
    }
}
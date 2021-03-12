using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Authentication.Apis
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
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(GetDbConnectionString(), optionsBuilder => {
                    optionsBuilder.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name);
                }));

            services.AddIdentityCore<IdentityUser>(options => {
                options.User.AllowedUserNameCharacters = "hojn._";
            });

            services.AddScoped<IUserStore<IdentityUser>, UserOnlyStore<IdentityUser, IdentityDbContext>>();
            
            services.AddAuthentication(defaultScheme: CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Events.OnRedirectToAccessDenied = ReplaceRedirector(HttpStatusCode.Forbidden, options.Events.OnRedirectToAccessDenied);
                    options.Events.OnRedirectToLogin = ReplaceRedirector(HttpStatusCode.Unauthorized, options.Events.OnRedirectToLogin);
                });
            
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IdentityDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            dbContext.Database.EnsureCreated();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        static Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceRedirector(
            HttpStatusCode statusCode,
            Func<RedirectContext<CookieAuthenticationOptions>, Task> existingRedirector) => context =>
            {
                if (context.Request.Path.StartsWithSegments("/accounts"))
                {
                    context.Response.StatusCode = (int) statusCode;
                    return Task.CompletedTask;
                }
                return existingRedirector(context);
            };

        private string GetDbConnectionString()
        {
            var dbServer = Configuration["Db:Server"];
            var dbPort = Configuration["Db:Port"];
            var dbUsername = Configuration["Db:Username"];
            var dbPassword = Configuration["Db:Password"];
            var dbName = Configuration["Db:Name"];

            if (!string.IsNullOrWhiteSpace(dbPort))
            {
                return $"Server={dbServer},{dbPort};Database={dbName};User Id={dbUsername};password={dbPassword};trusted_connection=no;MultipleActiveResultSets=true;";
            }

            return $"Server={dbServer};Database={dbName};User Id={dbUsername};password={dbPassword};trusted_connection=no;MultipleActiveResultSets=true;";
        }
    }
}

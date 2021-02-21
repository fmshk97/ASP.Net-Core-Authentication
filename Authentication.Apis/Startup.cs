using Authentication.Apis.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Authentication.Apis
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityCore<AppUser>(options => {
                options.User.AllowedUserNameCharacters = "hojn._";
            });

            services.AddScoped<IUserStore<AppUser>, AppUserStore>();
            
            services.AddAuthentication(defaultScheme: CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Events.OnRedirectToAccessDenied = ReplaceRedirector(HttpStatusCode.Forbidden, options.Events.OnRedirectToAccessDenied);
                    options.Events.OnRedirectToLogin = ReplaceRedirector(HttpStatusCode.Unauthorized, options.Events.OnRedirectToLogin);
                });
            
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
    }
}

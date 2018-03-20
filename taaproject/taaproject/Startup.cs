using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using taaproject.Data;
using taaproject.Models;
using taaproject.Services;
using Microsoft.AspNetCore.Identity;

namespace taaproject
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
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //services.AddIdentity<ApplicationUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

            services.AddIdentityWithMongoStores(Configuration.GetConnectionString("DefaultConnection")).AddDefaultTokenProviders();
            //services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            var emailsettings = Configuration.GetSection(nameof(EmailSettings)).Get<EmailSettings>();
            // Add application services.
            services.AddTransient<IEmailSettings>(x => emailsettings);
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ProjectService, ProjectService>();
            services.AddTransient<MembershopServices, MembershopServices>();
            services.AddTransient<WorkService, WorkService>();
            services.AddTransient<IServiceConfigurations>(x => Configuration.GetSection(nameof(ServiceConfigurations)).Get<ServiceConfigurations>());

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

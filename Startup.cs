using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MakeASurvey.Data;
using Microsoft.OpenApi.Models;

namespace MakeASurvey
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
            services.AddControllers();

            services.AddDbContext<MakeASurveyContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("acbasic")));

            //Add Swagger relates setting  
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Demo Employee API",
                    Version = "v1.1",
                    Description = "API to unerstand request and response schema.",
                });
            });

            services.AddCors(options =>
                 options.AddDefaultPolicy(
                     builder => builder.AllowAnyOrigin()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(builder => builder
                        .WithOrigins("http://localhost:3000", "http://localhost:4200", "https://lemon-tree-0f0786803.1.azurestaticapps.net", "https://acsurvey.azurewebsites.net", "https://takeasurvey.azurewebsites.net")
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithHeaders("*")
                        );

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo Employee API");
            });
        }
    }
}

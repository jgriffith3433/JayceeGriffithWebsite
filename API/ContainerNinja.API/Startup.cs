using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ContainerNinja.Infrastructure;
using ContainerNinja.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using ContainerNinja.API.Filters;
using ContainerNinja.Core.Common;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using ContainerNinja.Contracts.Enum;
using System.Collections.Generic;

namespace ContainerNinja
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
            services.AddCors();

            services.AddPersistence(Configuration);
            services.AddCore(Configuration);

            // prevents Mvc to throw 400 on invalid RequestBody
            // this is since we're using Fluent to do the same
            // within the Action Method
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddJwtBearerAuthentication();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            //services.AddResponseCaching();

            services.AddControllers(options =>
            {
                options.Filters.Add<AddHandlerHostHeaderResponseFilter>();
                options.Filters.Add<ApiExceptionFilterAttribute>();
            });

            services.AddSwaggerWithVersioning();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            License.RegisterLicense(Environment.GetEnvironmentVariable("JsonNETSchemaLicense"));
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new KitchenUnitTypeStringEnumConverter()
                }
            };

            app.UseCors(builder =>
            {
                builder.AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod();
            });

            app.UseSwaggerWithVersioning(provider);

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            //app.UseCaching();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}

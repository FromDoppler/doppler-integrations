using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Doppler.Integrations.Services;
using Doppler.Integrations.Services.Interfaces;
using Doppler.Integrations.Helpers.Interfaces;
using Doppler.Integrations.Helpers;
using Doppler.Integrations.Mapper;
using Doppler.Integrations.Mapper.Interfaces;

namespace Doppler.Integrations
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddMvc();

            services.AddSingleton<IMapperSubscriber, MapperSubscriber>();

            services.AddSingleton<IDopplerURLs>(
                new DopplerURLs(Configuration["DopplerAPI:Base_URL"]));

            services.AddScoped<IDopplerService, DopplerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();
            app.UseMvc();
        }
    }
}
